using Framework;
using Game.Basic;
using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;

namespace Game.Data
{
    /// <summary>
    /// Tip types for UI display
    /// </summary>
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







        #region Properties
        public UserData User { get; set; }
        public string AppVersion => PlayerPrefs.GetString("APP_VERSION");
        public string HotVersion => PlayerPrefs.GetString("HOT_VERSION");
        public string Gateway { get; set; }
        public string Device => PlayerPrefs.GetString("DEVICE");
        public string Dark { get => Get<string>(Type.Dark); set => Change(Type.Dark, value); }
        public Languages Language { get => Get<Languages>(Type.Language); set => Change(Type.Language, value); }
        public int FontSize { get => Get<int>(Type.FontSize); set => Change(Type.FontSize, value); }

        public DateTime Ping { get => Get<DateTime>(Type.Ping); set => Change(Type.Ping, value); }
        public DateTime Pong { get; set; }
        public Account SelectedAccount => User.Accounts[User.SelectedAccountIndex];
        public Account LoginAccount { get => Get<Account>(Type.LoginAccount); set => Change(Type.LoginAccount, value); }
        public bool Online { get => Get<bool>(Type.Online); set { Change(Type.Online, value); } }
        public List<Server> Servers { get => Get<List<Server>>(Type.Servers); set => Change(Type.Servers, value); }
        public Server SelectedServer => Servers[User.SelectedServerIndex];
        public int LoginResponse { get => Get<int>(Type.LoginResponse); set => Change(Type.LoginResponse, value); }
        public string LoginResponseMessage { get => Get<string>(Type.LoginResponseMessage); set => Change(Type.LoginResponseMessage, value); }
        public InitializeData Initialize { get => Get<InitializeData>(Type.Initialize); set => Change(Type.Initialize, value); }
        public int InitialResponse { get => Get<int>(Type.InitialResponse); set => Change(Type.InitialResponse, value); }
        public string InitialResponseMessage { get => Get<string>(Type.InitialResponseMessage); set => Change(Type.InitialResponseMessage, value); }
        public HomeData Home { get => Get<HomeData>(Type.Home); set => Change(Type.Home, value); }
        public float SceneScale { get => Get<float>(Type.SceneScale); set => Change(Type.SceneScale, value); }
        public List<MapData> Maps { get => Get<List<MapData>>(Type.Maps); set => Change(Type.Maps, value); }
        public int[] Pos { get => Get<int[]>(Type.Pos) ?? Home?.scene?.pos; set => Change(Type.Pos, value); }
        public string SceneName { get => Get<string>(Type.SceneName) ?? Home?.scene?.sceneName ?? ""; set => Change(Type.SceneName, value); }
        public List<int[]> Area { get => Get<List<int[]>>(Type.Area) ?? Home?.area; set => Change(Type.Area, value); }
        public List<CharacterData> Characters { get => Get<List<CharacterData>>(Type.Characters); set => Change(Type.Characters, value); }
        public List<KeyValuePair<string, double>> BattleProgress { get => Get<List<KeyValuePair<string, double>>>(Type.BattleProgress); set => Change(Type.BattleProgress, value); }
        public DateTime OptionReturn { get => Get<DateTime>(Type.OptionReturn); set => Change(Type.OptionReturn, value); }
        public (TipType, string) Tip { get => Get<(TipType, string)>(Type.Tip); set => Change(Type.Tip, value); }
        public List<((string, string, int, bool), string, string)> Tutorial { get => Get<List<((string, string, int, bool), string, string)>>(Type.Tutorial); set => Change(Type.Tutorial, value); }
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
        public int TutorialIndex { get => Get<int>(Type.TutorialIndex); set => Change(Type.TutorialIndex, value); }
        public TutorialData TutorialStep { get => Get<TutorialData>(Type.TutorialStep); set => Change(Type.TutorialStep, value); }
        public List<InformationData> Informations
        {
            get
            {
                List<InformationData> results = new List<InformationData>();
                foreach (InformationData information in Get<List<InformationData>>(Type.Informations))
                {
                    if (!HomeChannelToggle.ContainsKey(information.channel.ToString()) || HomeChannelToggle[information.channel.ToString()])
                    {
                        results.Add(information);
                    }
                }
                return results;
            }
            set
            {
                Change(Type.Informations, value);
            }
        }
        public OptionData Option { get => Get<OptionData>(Type.Option); set => Change(Type.Option, value); }
        public WorldMapData WorldMap { get => Get<WorldMapData>(Type.WorldMap); set => Change(Type.WorldMap, value); }
        public UILockData UILock { get => Get<UILockData>(Type.UILock); set => Change(Type.UILock, value); }

        public List<DialogueData> StoryDialogues { get => Get<List<DialogueData>>(Type.StoryDialogues); set => Change(Type.StoryDialogues, value); }
        public Dictionary<string, bool> HomeChannelToggle { get; set; } = new Dictionary<string, bool>();
        public Dictionary<string, SceneData> SceneCache { get; set; } = new Dictionary<string, SceneData>();
        public float SceneScaleBeforeWorldMap { get; set; } = 1.0f;
        
        public Dictionary<string, string> StartTexts { get => Get<Dictionary<string, string>>(Type.StartTexts); set => Change(Type.StartTexts, value); }
        public Dictionary<string, string> StartSettingsTexts { get => Get<Dictionary<string, string>>(Type.StartSettingsTexts); set => Change(Type.StartSettingsTexts, value); }
        public Dictionary<string, string> AccountTexts { get => Get<Dictionary<string, string>>(Type.AccountTexts); set => Change(Type.AccountTexts, value); }
        public Dictionary<string, string> ErrorTexts { get => Get<Dictionary<string, string>>(Type.ErrorTexts); set => Change(Type.ErrorTexts, value); }

        public Dictionary<string, string> Texts { get => Get<Dictionary<string, string>>(Type.Texts); set => Change(Type.Texts, value); }
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
            string langSource = PlayerPrefs.GetString("LANGUAGE", User.Language ?? "");
            Languages parsedLang = Enum.TryParse(langSource, out Languages lang) ? lang : Languages.ChineseSimplified;
            raw[Type.Language] = parsedLang;
            raw[Type.FontSize] = User.FontSize > 0 ? User.FontSize : 30;
            
            SceneScaleBeforeWorldMap = DefaultSceneScale;
            
            after.Register(Type.LoginAccount, OnAfterLoginAccountChanged);
            after.Register(Type.Language, OnAfterLanguageChanged);
            after.Register(Type.FontSize, OnAfterFontSizeChanged);

            Framework.FontScaler.GetCurrentFontSize = () => FontSize;

            // Setup UI Listeners
            Game.Basic.Event.Instance.Add("UI.Event.Click", OnUIClick);
            Game.Basic.Event.Instance.Add("Game.Initialize.Click.Random", OnInitializeRandomClick);
            Game.Basic.Event.Instance.Add("Game.Initialize.Click.Confirm", OnInitializeConfirmClick);
            Game.Basic.Event.Instance.Add("Game.Home.Event.SceneZoomIn", OnHomeSceneZoomIn);
            Game.Basic.Event.Instance.Add("Game.Home.Event.SceneZoomOut", OnHomeSceneZoomOut);
            Game.Basic.Event.Instance.Add("Game.Home.Event.ChannelToggle", OnHomeChannelToggle);
            //Game.Basic.Event.Instance.Add(Option.Event.Return, OnOptionReturn);

            Game.Basic.Event.Instance.Add("HeartbeatLog.Toggle", OnHeartbeatLogToggle);
        }

        private void OnUIClick(params object[] args)
        {
            Audio.Instance.Play(Config.Audio.Click);
        }

        public void Init()
        {
            raw[Type.Servers] = new List<Server>();
            raw[Type.Informations] = new List<InformationData>();
            //Servers = JsonConvert.DeserializeObject<List<Server>>(PlayerPrefs.GetString("SERVERS"));
        }

        private void OnHeartbeatLogToggle(params object[] args)
        {
            bool isEnabled = (bool)args[0];
            Utils.Debug.EnableHeartbeatLog = isEnabled;
            Utils.Debug.Log("System", $"Network heartbeat logging {(isEnabled ? "enabled" : "disabled")}");
        }
        #endregion



        private void OnAfterLoginAccountChanged(params object[] args)
        {
            Account account = (Account)args[0];
            if (!User.Accounts.Contains(account))
            {
                User.Accounts.Add(account);
                User.SelectedAccountIndex = User.Accounts.IndexOf(account);
                Local.Instance.Save(User);
            }

        }
        private void OnAfterLanguageChanged(params object[] args)
        {
            Languages language = (Languages)args[0];
            User.Language = language.ToString();
            PlayerPrefs.SetString("LANGUAGE", language.ToString());
            PlayerPrefs.Save();
            Local.Instance.Save(User);
        }

        private void OnAfterFontSizeChanged(params object[] args)
        {
            int size = (int)args[0];
            User.FontSize = size;
            Local.Instance.Save(User);
            Framework.FontScaler.NotifyFontSizeChanged(size);
        }

        #region Network Event Handlers





        #endregion

        #region UI Start
        // Server selection removed - using default server (index 0)





        #endregion

        #region Initialization Handlers

        private void OnInitializeConfirmClick(params object[] args)
        {
            string name = (string)args[0];
            // Trigger event instead of directly calling Net (respecting layer separation)
            Game.Basic.Event.Instance.Fire("Game.Initialize.Click.Confirm", name);
        }

        private void OnInitializeRandomClick(params object[] args)
        {
            // Trigger event instead of directly calling Net (respecting layer separation)
            Game.Basic.Event.Instance.Fire("Game.Initialize.Click.Random");
        }
        private void OnHomeSceneZoomIn(params object[] args)
        {
            float newScale = SceneScale - 1f;
            if (newScale < MinSceneScale)
            {
                newScale = MinSceneScale;
            }
            User.SceneScale = SceneScale = newScale;
            Local.Instance.Save(User);
        }
        private void OnHomeSceneZoomOut(params object[] args)
        {
            float maxScale = GetMaxSceneScale();
            float newScale = SceneScale + 1f;
            if (newScale > maxScale)
            {
                newScale = maxScale;
            }
            User.SceneScale = SceneScale = newScale;
            Local.Instance.Save(User);
        }

        public float SceneContainerSize { get; set; } = 600f;

        public float GetMaxSceneScale()
        {
            return SceneContainerSize / MinCellSize;
        }
        private void OnHomeChannelToggle(params object[] args)
        {
            string name = (string)args[0];
            bool isOn = (bool)args[1];
            HomeChannelToggle[name] = isOn;
        }


        #endregion

        #region Game Data Handlers

        private List<MapData> AdaptMaps(List<MapData> maps)
        {
            List<MapData> results = new List<MapData>();
            int minX = maps.Select(m => m.pos[0]).Min();
            int minY = maps.Select(m => m.pos[1]).Min();
            int maxX = maps.Select(m => m.pos[0]).Max();
            int maxY = maps.Select(m => m.pos[1]).Max();
            for (int y = maxY; y >= minY; y--)
            {
                for (int x = minX; x <= maxX; x++)
                {
                    results.Add(maps.Find(m => m.pos[0] == x && m.pos[1] == y));
                }
            }
            return results;
        }

        #endregion

    }
}


