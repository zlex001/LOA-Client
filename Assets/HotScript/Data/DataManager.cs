using Framework;
using Game.Basic;
using System;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;

namespace Game.Data
{
    public enum TipType
    {
        Fly,
        Numerical,
        Attach,
    }

    [Serializable]
    public class DeviceAuthRequest
    {
        [JsonProperty("deviceId")]
        public string Id;

        [JsonProperty("platform")]
        public string Platform;
    }

    [Serializable]
    public class ServerInfo
    {
        [JsonProperty("id")]
        public string Id;

        [JsonProperty("name")]
        public string Name;

        [JsonProperty("ip")]
        public string Ip;

        [JsonProperty("port")]
        public int Port;
    }

    [Serializable]
    public class UICommand
    {
        [JsonProperty("name")]
        public string Name;

        [JsonProperty("data")]
        public Dictionary<string, string> Data;
    }

    [Serializable]
    public class GatewayResponse
    {
        [JsonProperty("servers")]
        public List<ServerInfo> Servers;

        [JsonProperty("ui")]
        public UICommand UI;
    }

    [Serializable]
    public class Server
    {
        public string Id;
        public string Name;
        public string Ip;
        public int Port;
    }

    public class SceneInfo
    {
        public int[] pos;
        public string sceneName;
        public string color;
        public string type;
    }

    public partial class DataManager : Singleton<DataManager>
    {
        public const float MinCellSize = 60f;
        public const float MinSceneScale = 5f;
        public const float DefaultSceneScale = 8f;

        #region Enums
        public enum Type
        {
            Ping,
            LoginAccount,
            Online,
            LoginResponse,
            LoginResponseMessage,
            Initialize,
            InitialResponse,
            InitialResponseMessage,
            Home,
            SceneScale,

            Maps,
            Pos,
            SceneName,
            Area,
            Characters,
            BattleProgress,
            Informations,
            Option,
            WorldMap,

            PlayerState,
            Shield,
            Level,
            Exp,
            MaxExp,
            Name,
            Gold,

            OptionReturn,
            Tip,

            StoryDialogues,
            Tutorial,
            TutorialIndex,
            TutorialStep,
            SocketMissedHeartbeats,
            Servers,
            Language,

            Dark,
            FontSize,
            UILock,

            StartTexts,
            StartSettingsTexts,
            AccountTexts,
            ErrorTexts,

            Texts,
        }
        #endregion

        #region Fields
        public Dictionary<Enum, object> raw = new Dictionary<Enum, object>();
        public Monitor befor = new Monitor();
        public Monitor after = new Monitor();
        #endregion

        public enum Languages
        {
            ChineseSimplified,
            ChineseTraditional,
            English,
            Japanese,
            Korean,
            French,
            German,
            Spanish,
            Portuguese,
            Russian,
            Italian,
            Polish,
            Turkish,
            Dutch,
            Danish,
            Swedish,
            Norwegian,
            Finnish,
            Thai,
            Vietnamese,
            Indonesian,
            Ukrainian,
        }

        #region Core Properties
        public UserData User { get; set; }
        public string AppVersion => PlayerPrefs.GetString("APP_VERSION");
        public string HotVersion => PlayerPrefs.GetString("HOT_VERSION");
        public string Gateway { get; set; }
        public string Device => PlayerPrefs.GetString("DEVICE");
        public DateTime Ping { get => Get<DateTime>(Type.Ping); set => Change(Type.Ping, value); }
        public DateTime Pong { get; set; }
        public bool Online { get => Get<bool>(Type.Online); set { Change(Type.Online, value); } }
        public List<Server> Servers { get => Get<List<Server>>(Type.Servers); set => Change(Type.Servers, value); }
        public Server SelectedServer => Servers[User.SelectedServerIndex];
        public string Dark { get => Get<string>(Type.Dark); set => Change(Type.Dark, value); }
        public (TipType, string) Tip { get => Get<(TipType, string)>(Type.Tip); set => Change(Type.Tip, value); }
        public int SocketMissedHeartbeats
        {
            get => Get<int>(Type.SocketMissedHeartbeats);
            set
            {
#if UNITY_EDITOR
                int oldValue = Get<int>(Type.SocketMissedHeartbeats);
                if (oldValue != value)
                {
                    Utils.Debug.LogHeartbeat("DataManager", $"SocketMissedHeartbeats changed from {oldValue} to {value}");
                    if (value >= Config.Net.MaxMissedHeartbeats)
                    {
                        Utils.Debug.LogWarning("DataManager", $"Missed heartbeats ({value}) reached threshold ({Config.Net.MaxMissedHeartbeats}), will trigger disconnection");
                    }
                }
#endif
                Change(Type.SocketMissedHeartbeats, value);
            }
        }
        #endregion

        #region Public Methods

        public void Change<T>(Enum e, T v)
        {
            if (!befor.condition.ContainsKey(e) || befor.condition[e](v))
            {
                T o = Get<T>(e);
                if (typeof(T).IsValueType && v.Equals(default(T))) { v = default(T); }
                befor.Fire(e, o, v);
                raw[e] = v;
                if (e is Type type2 && type2 == Type.LoginResponse)
                {
                    Utils.Debug.Log("DataManager", $"LoginResponse changed to: {v}, firing after event");
                }

                after.Fire(e, v);
            }
        }

        public T Get<T>(Enum @enum) => raw.ContainsKey(@enum) && raw[@enum] is T t ? t : default;

        public string GetText(string key)
        {
            var texts = Texts;
            return texts != null && texts.ContainsKey(key) ? texts[key] : null;
        }
        #endregion

        #region Unity Lifecycle
        void Awake()
        {
            raw[Type.Informations] = new List<InformationData>();
            User = Local.Instance.Load<UserData>() ?? new UserData { SceneScale = DefaultSceneScale };

            float loadedScale = User.SceneScale;
            if (loadedScale < MinSceneScale)
            {
                loadedScale = DefaultSceneScale;
            }

            raw[Type.SceneScale] = loadedScale;
            string prefsLang = PlayerPrefs.GetString("LANGUAGE", "");
            string userLang = User.Language ?? "";
            string langSource = !string.IsNullOrEmpty(prefsLang) ? prefsLang : userLang;
            Languages parsedLang = Enum.TryParse(langSource, out Languages lang) ? lang : Languages.ChineseSimplified;
            Utils.Debug.Log("Lang", $"DataManager.Awake: PlayerPrefs LANGUAGE=\"{prefsLang}\", User.Language=\"{userLang}\", langSource=\"{langSource}\", parsedLang={parsedLang}");
            raw[Type.Language] = parsedLang;
            raw[Type.FontSize] = User.FontSize > 0 ? User.FontSize : 30;

            SceneScaleBeforeWorldMap = DefaultSceneScale;

            RegisterMonitors();
            RegisterEvents();

            Framework.FontScaler.GetCurrentFontSize = () => FontSize;
        }

        private void RegisterMonitors()
        {
            RegisterAuthMonitors();
            after.Register(Type.Language, OnAfterLanguageChanged);
            after.Register(Type.FontSize, OnAfterFontSizeChanged);
        }

        private void RegisterEvents()
        {
            Basic.Event.Instance.Add("UI.Event.Click", OnUIClick);
            Basic.Event.Instance.Add("Game.Initialize.Click.Random", OnInitializeRandomClick);
            Basic.Event.Instance.Add("Game.Initialize.Click.Confirm", OnInitializeConfirmClick);
            Basic.Event.Instance.Add("Game.Home.Event.SceneZoomIn", OnHomeSceneZoomIn);
            Basic.Event.Instance.Add("Game.Home.Event.SceneZoomOut", OnHomeSceneZoomOut);
            Basic.Event.Instance.Add("Game.Home.Event.ChannelToggle", OnHomeChannelToggle);
            Basic.Event.Instance.Add("HeartbeatLog.Toggle", OnHeartbeatLogToggle);
        }

        private void OnUIClick(params object[] args)
        {
            Audio.Instance.Play(Config.Audio.Click);
        }

        public void Init()
        {
            raw[Type.Servers] = new List<Server>();
            raw[Type.Informations] = new List<InformationData>();
        }

        private void OnHeartbeatLogToggle(params object[] args)
        {
            bool isEnabled = (bool)args[0];
            Utils.Debug.EnableHeartbeatLog = isEnabled;
            Utils.Debug.Log("System", $"Network heartbeat logging {(isEnabled ? "enabled" : "disabled")}");
        }
        #endregion
    }
}
