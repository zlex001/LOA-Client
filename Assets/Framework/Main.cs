using System;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

namespace Framework
{
    public class Main : MonoBehaviour
    {
        public Config config;
        private Text _footer;
        private Text _description;
        private Image _progressFill;
        private Localization _lang;
        private bool _isProcessing = false;

        string Device
        {
            get
            {
                if (!PlayerPrefs.HasKey("DEVICE"))
                {
                    PlayerPrefs.SetString("DEVICE", Guid.NewGuid().ToString());
                    PlayerPrefs.Save();
                }
                return PlayerPrefs.GetString("DEVICE");
            }
        }

        string BaseUrl => $"http://{config.Gateway}:8880";

        void Awake()
        {
            Screen.sleepTimeout = SleepTimeout.NeverSleep;
            _footer = transform.Find("Footer").GetComponent<Text>();
            _description = transform.Find("Description").GetComponent<Text>();
            _progressFill = transform.Find("Progress/Fill").GetComponent<Image>();
            
            PlayerPrefs.SetString("APP_VERSION", config.appVersion);
            PlayerPrefs.Save();
        }

        void Start()
        {
            string language = LanguageDetector.DetermineLanguage();
            InitLanguage(language);
            _footer.text = _lang.Get("footer", config.appVersion, Device.Split('-').Last());
            
            LanguageDetector.DetectFromServer(config.Gateway, serverLanguage =>
            {
                StepConnect();
            });
        }

        void InitLanguage(string language)
        {
            if (!PlayerPrefs.HasKey("LANGUAGE"))
            {
                PlayerPrefs.SetString("LANGUAGE", language);
                PlayerPrefs.Save();
            }
            _lang = Localization.Instance;
            _lang.Init(language);
        }

        void SetDescription(string text) => _description.text = text;

        void Progress(int already, int sum = -1)
        {
            _progressFill.fillAmount = (sum > 0) ? (float)already / sum : already;
            SetDescription(sum > 0 
                ? _lang.Get("downloading_resources", FormatBytes(already), FormatBytes(sum))
                : _lang.Get("unpacking_resources", Mathf.CeilToInt(((sum > 0) ? (float)already / sum : already) * 100)));
        }

        string FormatBytes(int bytes)
        {
            if (bytes >= 1024 * 1024) return $"{(bytes / (1024f * 1024f)):0.00} MB";
            if (bytes >= 1024) return $"{(bytes / 1024f):0.00} KB";
            return $"{bytes} bytes";
        }

        void StepConnect()
        {
            SetDescription(_lang.Get("connecting_auth"));
            Http.Instance.AcceptLanguage = _lang.StandardLanguageCode;
            Http.Instance.RequestPost($"{BaseUrl}/api/authentication/device", $"{{\"deviceId\":\"{Device}\",\"platform\":\"{Application.platform}\"}}",
                (success, response) =>
                {
                    if (success)
                    {
#if UNITY_EDITOR
                        StepLaunch();
#else
                        Hot.Instance.Init(BaseUrl, config.hotVersion);
                        Hot.Instance.OnStatusChanged = SetDescription;
                        StepCheckVersion();
#endif
                    }
                    else
                    {
                        Debug.LogError($"[Main] Device auth failed: {response}");
                        SetDescription(_lang.Get("auth_failed", _lang.Get(string.IsNullOrEmpty(response) ? "connection_timeout" : "network_error")));
                    }
                });
        }

        void StepCheckVersion()
        {
            SetDescription(_lang.Get("checking_version"));
            Hot.Instance.Begin((need, error, texts) =>
            {
                if (texts != null) _lang.MergeServerTexts(texts);
                if (string.IsNullOrEmpty(error))
                    StepDownloadOrSkip();
                else
                    SetDescription(_lang.Get("version_check_failed", _lang.Get(error)));
            });
        }

        void StepDownloadOrSkip()
        {
            if (_isProcessing)
            {
                Debug.LogWarning("[Main] Already in progress");
                return;
            }
            
            _isProcessing = true;
            SetDescription(_lang.Get("comparing_resources"));
            
            Hot.Instance.Check((need, reses, texts) =>
            {
                if (texts != null) _lang.MergeServerTexts(texts);
                
                if (reses == null)
                {
                    SetDescription("[Main] Resource check failed");
                    SetDescription(_lang.Get("resource_check_failed"));
                    _isProcessing = false;
                    return;
                }
                
                SetDescription($"[Main] Check result: need={need}, files count={reses.Count}");
                
                if (need)
                    StepDownload(reses);
                else
                {
                    SetDescription(_lang.Get("resources_updated"));
                    StepLoadAssemblies();
                }
            });
        }

        void StepDownload(List<Hot.Res> reses)
        {
            SetDescription(_lang.Get("start_downloading"));
            Progress(0);

            int already = 0;
            int sum = reses.Sum(r => r.size);

            foreach (var res in reses)
            {
                string path = $"{FrameworkPath.Runtime}/{res.name}";
                if (System.IO.File.Exists(path)) System.IO.File.Delete(path);
            }

            Hot.Instance.Downloads(reses, (over, size, failedResName) =>
            {
                already += size;
                Progress(already, sum);
                if (over)
                {
                    SetDescription(_lang.Get("download_complete"));
                    StepLoadAssemblies();
                }
                else if (!string.IsNullOrEmpty(failedResName))
                    SetDescription(_lang.Get("download_failed"));
            });
        }

        void StepLoadAssemblies()
        {
#if UNITY_EDITOR
            StepLaunch();
#else
            SetDescription(_lang.Get("loading_metadata"));
            Hot.Instance.LoadAssetDependencies();

            int finished = 0;
            string[] aots = { "Framework.dll.bytes", "System.Core.dll.bytes", "mscorlib.dll.bytes" };
            
            foreach (var file in aots)
            {
                File.LoadFile(this, file, (success, data) =>
                {
                    if (success)
                    {
                        HybridCLR.RuntimeApi.LoadMetadataForAOTAssembly(data, HybridCLR.HomologousImageMode.SuperSet);
                        if (++finished == aots.Length) StepLaunch();
                    }
                    else
                    {
                        Debug.LogError($"[Main] AOT load failed: {file}");
                        SetDescription(_lang.Get("metadata_load_failed"));
                        _isProcessing = false;
                    }
                });
            }
#endif
        }

        void StepLaunch()
        {
            try
            {
                SetDescription(_lang.Get("launching_game"));
                var assemblies = LoadHotUpdateAssemblies();
                var gateAssembly = assemblies.FirstOrDefault(a => a.GetName().Name == "Start");
                if (gateAssembly == null) throw new System.Exception("Start assembly not found");

                var gateType = gateAssembly.GetType("Game.Start.Gate");
                if (gateType == null) throw new System.Exception("Game.Start.Gate not found");
                
                var entranceMethod = gateType.GetMethod("Entrance");
                if (entranceMethod == null) throw new System.Exception("Entrance not found");
                
                gameObject.SetActive(false);
                entranceMethod.Invoke(null, new object[] { config.Gateway });
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[Main] Launch failed: {ex.Message}");
                SetDescription(_lang.Get("launch_failed", _lang.Get("parse_error")));
                gameObject.SetActive(true);
                _isProcessing = false;
            }
        }

        System.Reflection.Assembly[] LoadHotUpdateAssemblies()
        {
#if UNITY_EDITOR
            string[] hotUpdateAssemblyNames = new string[] 
            {
                "Utils", "Basic", "Data", "Net", "Logic", "Presentation", "Start", "SDK"
            };
            
            var assemblies = new List<System.Reflection.Assembly>();
            foreach (var name in hotUpdateAssemblyNames)
            {
                var assembly = AppDomain.CurrentDomain.GetAssemblies()
                    .FirstOrDefault(a => a.GetName().Name == name);
                if (assembly != null)
                {
                    assemblies.Add(assembly);
                    Debug.Log($"[Main] Loaded hot update assembly: {name}");
                }
                else
                {
                    Debug.LogWarning($"[Main] Hot update assembly not found: {name}");
                }
            }
            return assemblies.ToArray();
#else
            string[] hotUpdateDlls = new string[] 
            {
                "utils.dll.bytes",
                "basic.dll.bytes",
                "data.dll.bytes",
                "net.dll.bytes",
                "logic.dll.bytes",
                "presentation.dll.bytes",
                "start.dll.bytes",
                "sdk.dll.bytes"
            };
            
            var assemblies = new List<System.Reflection.Assembly>();
            foreach (var dllFile in hotUpdateDlls)
            {
                string dllPath = $"{FrameworkPath.Scripts}/{dllFile}";
                
                if (!System.IO.File.Exists(dllPath))
                {
                    Debug.LogError($"[Main] {dllFile} not found at: {dllPath}");
                    throw new System.IO.FileNotFoundException($"{dllFile} not found");
                }
                
                var assembly = System.Reflection.Assembly.Load(System.IO.File.ReadAllBytes(dllPath));
                assemblies.Add(assembly);
                Debug.Log($"[Main] Loaded hot update dll: {dllFile}");
            }
            return assemblies.ToArray();
#endif
        }
    }
}
