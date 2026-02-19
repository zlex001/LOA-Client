using Google.Protobuf.WellKnownTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using Game.Basic;
using Game.Data;
using UnityEngine;

namespace Game.Net.Protocol
{
    public class Base
    {

        public virtual void Processed()
        {

        }

    }

    public class Ping : Base
    {
        public DateTime dateTime;
        public Ping(DateTime dateTime)
        {
            this.dateTime = dateTime;
#if UNITY_EDITOR
            Utils.Debug.LogHeartbeat("Protocol", $"Ping已创建，时间戳: {dateTime}");
#endif
        }
    }
    public class Pong : Base
    {
        public DateTime dateTime;
        public override void Processed()
        {
#if UNITY_EDITOR
            TimeSpan roundTrip = DateTime.Now - DataManager.Instance.Ping;
            TimeSpan serverDelta = dateTime - DataManager.Instance.Ping;
            Utils.Debug.LogHeartbeat("Protocol", $"Pong已处理。服务器时间: {dateTime}, 往返时间: {roundTrip.TotalMilliseconds:F2}毫秒, 服务器处理时间: {serverDelta.TotalMilliseconds:F2}毫秒");
#endif
            DataManager.Instance.Pong = dateTime;
        }
    }

    public class Login : Base
    {
        public Login(Account account)
        {
            id = account.Id;
            password = account.Password;
            version = DataManager.Instance.AppVersion;
            platform = Application.platform.ToString();
            device = PlayerPrefs.GetString("DEVICE");
            language = DataManager.Instance.Language.ToString();
        }
        public string id;
        public string password;
        public string platform;
        public string version;
        public string device;
        public string language;
    }
    public class LoginResponse : Base
    {
        public enum Code
        {
            Success,
            PasswordError,
            AppVersionUnfit,
            UnsafeAccount
        }
        public int code;
        public string message;
        public string accountId;    // For QuickStart: guest account ID
        public string password;     // For QuickStart: guest account password
        public bool isGuest;        // Is this a guest account
        public bool isNewAccount;   // Is this a newly created account
        
        public override void Processed()
        {
            Utils.Debug.Log("Protocol", $"LoginResponse.Processed() called: code={code}, message={message}");
            
            // For QuickStart: Save guest account to local if provided
            if (!string.IsNullOrEmpty(accountId) && !string.IsNullOrEmpty(password))
            {
                var account = new Account
                {
                    Id = accountId,
                    Password = password,
                    Note = isGuest ? "Guest Account" : "Bound Account"
                };
                
                // Check if account already exists
                bool accountExists = false;
                foreach (var existingAccount in DataManager.Instance.User.Accounts)
                {
                    if (existingAccount.Id == accountId)
                    {
                        accountExists = true;
                        break;
                    }
                }
                
                if (!accountExists)
                {
                    DataManager.Instance.User.Accounts.Add(account);
                    DataManager.Instance.User.SelectedAccountIndex = DataManager.Instance.User.Accounts.Count - 1;
                    Local.Instance.Save(DataManager.Instance.User);
                    Utils.Debug.Log("Protocol", $"Saved guest account: {accountId}, isNewAccount: {isNewAccount}");
                }
                else
                {
                    Utils.Debug.Log("Protocol", $"Guest account already exists: {accountId}");
                }
            }
            
            DataManager.Instance.LoginResponseMessage = message;
            DataManager.Instance.LoginResponse = code;
            Utils.Debug.Log("Protocol", $"LoginResponse data set complete");
        }
    }
    public class Initialize : Base
    {
        public class UITexts
        {
            public string namePlaceholder;
            public string randomButton;
            public string confirmButton;
            public string errorNameEmpty;
            public string errorNameUnsafe;
        }
        
        public string description;
        public Dictionary<string, int> grade = new Dictionary<string, int>();
        public UITexts ui;

        public override void Processed()
        {
            DataManager.Instance.Initialize = this.ToLogic();
        }
    }
    public class InitializeRandom : Base
    {

    }
    public class InitializeConfirm : Base
    {
        public InitializeConfirm(string name)
        {
            this.name = name;
        }
        public string name;
    }
    public class InitialResponse : Base
    {
        public enum Code
        {
            Success,
            Empty,
            TooLong,
            Unsafe,
            AlreadyExsit,
        }
        public int code;
        public string message;

        public override void Processed()
        {
            DataManager.Instance.InitialResponseMessage = message;
            DataManager.Instance.InitialResponse = code;
        }
    }
    public class Map
    {
        public int[] pos;
        public string name;
        public string color;
    }

    public class Scene : Base
    {
        public int[] pos;
        public List<Map> maps;
        public string sceneName;
        public List<Map> sortedMaps;
        
        public override void Processed()
        {
            sortedMaps = new List<Map>();
            int minX = this.maps.Select(m => m.pos[0]).Min();
            int minY = this.maps.Select(m => m.pos[1]).Min();
            int maxX = this.maps.Select(m => m.pos[0]).Max();
            int maxY = this.maps.Select(m => m.pos[1]).Max();
            for (int y = maxY; y >= minY; y--)
            {
                for (int x = minX; x <= maxX; x++)
                {
                    sortedMaps.Add(this.maps.Find(m => m.pos[0] == x && m.pos[1] == y));
                }
            }
            
            // Convert to Logic model and cache
            var sceneData = this.ToLogic();
            string sceneKey = $"{pos[0]},{pos[1]},{pos[2]}";
            DataManager.Instance.SceneCache[sceneKey] = sceneData;
            
            DataManager.Instance.Maps = sortedMaps?.Select(m => m.ToLogic()).ToList();
            DataManager.Instance.Pos = pos;
            DataManager.Instance.SceneName = sceneName;
        }
    }
    public class Characters : Base
    {
        public class CharacterData
        {
            public string name;
            public double progress;
            public int hash;
            public int configId;  // Configuration ID for tutorial targeting
        }
        
        public List<CharacterData> content;
        public override void Processed()
        {
            DataManager.Instance.Characters = content?.Select(c => c.ToLogic()).ToList();
        }
    }
    public class BattleProgress : Base
    {
        public List<KeyValuePair<string, double>> content;
        public override void Processed()
        {
            DataManager.Instance.BattleProgress = content;
        }
    }
    public class Home : Base
    {
        public class UITexts
        {
            public Dictionary<string, string> resourceLabels;
            public string[] channels;
            public string chatPlaceholder;
        }
        
        public Scene scene;
        public Characters characters;
        public Dictionary<string, (int CurrentValue, int MaxValue, string Color)> resouse;
        public List<int[]> area;
        public UITexts ui;

        public override void Processed()
        {
            // Sort maps
            List<Map> maps = new List<Map>();
            int minX = scene.maps.Select(m => m.pos[0]).Min();
            int minY = scene.maps.Select(m => m.pos[1]).Min();
            int maxX = scene.maps.Select(m => m.pos[0]).Max();
            int maxY = scene.maps.Select(m => m.pos[1]).Max();
            for (int y = maxY; y >= minY; y--)
            {
                for (int x = minX; x <= maxX; x++)
                {
                    maps.Add(scene.maps.Find(m => m.pos[0] == x && m.pos[1] == y));
                }
            }
            scene.maps = maps;
            
            // Convert to Logic model and store
            var homeData = this.ToLogic();
            DataManager.Instance.Home = homeData;
            DataManager.Instance.Area = area;
            if (!string.IsNullOrEmpty(scene.sceneName))
            {
                DataManager.Instance.SceneName = scene.sceneName;
            }
        }
    }
    public class ClickMap : Base
    {
        public int[] pos;
        public ClickMap(int[] pos)
        {
            this.pos = pos;
        }
    }
    public class ClickCharacter : Base
    {
        public int hash;
        public ClickCharacter(int hash)
        {
            this.hash = hash;
        }
    }
    public class QueryScene : Base
    {
        public int[] scenePos;
        public QueryScene(int[] scenePos)
        {
            this.scenePos = scenePos;
        }
    }
    public class Pos : Base
    {
        public int[] content;
        public List<int[]> area;  
        public override void Processed()
        {
            // 关键修复：先设置Area再设置Pos，确保RefreshAllShownItem()时Area数据已是最新
            if (area != null)
            {
                DataManager.Instance.Area = area;
            }
            DataManager.Instance.Pos = content;
        }
    }

    public class Information : Base
    {
        public enum Channel
        {
            System,
            Private,
            Local,
            Battle,
            All,
            Rumor,
            Automation,
        }
        public string message;
        public Channel channel;
        public override void Processed()
        {
            var informations = new List<InformationData>(DataManager.Instance.Informations);
            informations.Add(this.ToLogic());
            if (informations.Count > 250) { informations.RemoveAt(0); }
            DataManager.Instance.Informations = informations;
        }
    }
    public class Chat : Base
    {
        public string content;
        public Chat(string content)
        {
            this.content = content;
        }
    }
    public class Option : Base
    {
        public class Item
        {
            public enum Type
            {
                Text,
                Button,
                TitleButton,
                TitleButtonWithProgress,
                IAPButton,
                Radar,
                ProgressWithValue,
                Progress,
                Slider,
                Input,
                Filter,
                ToggleGroup,
                Confirm,
                Amount,
            }
            public Type type;
            public Dictionary<string, string> data = new Dictionary<string, string>();
        }
        public List<Item> lefts;
        public List<Item> rights;
        public override void Processed()
        {
            var currentOption = DataManager.Instance.Option;
            var newOption = this.ToLogic();
            // Simple comparison - store if different
            // TODO: Implement proper comparison logic if needed
            DataManager.Instance.Option = newOption;
        }

        private bool AreEqual(Option other)
        {
            if (other == null) return false;
            if ((lefts == null) != (other.lefts == null)) return false;
            if ((rights == null) != (other.rights == null)) return false;
            
            if (lefts != null)
            {
                if (lefts.Count != other.lefts.Count) return false;
                for (int i = 0; i < lefts.Count; i++)
                {
                    if (!ItemsEqual(lefts[i], other.lefts[i])) return false;
                }
            }
            
            if (rights != null)
            {
                if (rights.Count != other.rights.Count) return false;
                for (int i = 0; i < rights.Count; i++)
                {
                    if (!ItemsEqual(rights[i], other.rights[i])) return false;
                }
            }
            
            return true;
        }

        private bool ItemsEqual(Item a, Item b)
        {
            if (a.type != b.type) return false;
            if (a.data.Count != b.data.Count) return false;
            
            foreach (var kvp in a.data)
            {
                if (!b.data.TryGetValue(kvp.Key, out string value) || value != kvp.Value)
                    return false;
            }
            
            return true;
        }
        public bool Empty => lefts == null && rights == null;
    }
    public class OptionReturn : Base
    {

    }
    public class OptionButton : Base
    {
        public int index;
        public int side;

        public OptionButton(int index, int side) 
        {
            this.index = index;
            this.side = side;
        }
    }
    public class OptionInput : Base
    {
        public string text;
        public OptionInput(string text)
        {
            this.text = text;
        }
    }
    public class OptionConfirm : Base
    {
        public int index;
        public int side;
        public OptionConfirm(int index, int side)
        {
            this.index = index;
            this.side = side;
        }
    }

    public class JsonPurchase : Base
    {
        public string receipt;
    }
    public class OptionSlider : Base
    {
        public int id;
        public int value;
        public OptionSlider(int id, int value)
        {
            this.id = id;
            this.value = value;
        }
    }
    public class OptionAmount : Base
    {
        public int amount;
        public OptionAmount(int amount)
        {
            this.amount = amount;
        }
    }

    public class OptionFilter : Base
    {
        public int id;
        public string text;
        public OptionFilter(int id, string text)
        {
            this.id = id;
            this.text = text;
        }
    }
    public class OptionToggle : Base
    {
        public string id;
        public bool value;
        public OptionToggle(string id, bool value)
        {
            this.id = id;
            this.value = value;
        }

    }

    public class AlipayOrder : Base
    {
        public string body;
        public override void Processed()
        {
            using (AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
            using (AndroidJavaObject currentActivity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity"))
            using (AndroidJavaObject alipayClass = new AndroidJavaObject("com.alipay.sdk.app.PayTask", currentActivity))
            {
                string result = alipayClass.Call<string>("pay", body, true);
            }
        }
    }
    public class DataPair : Base
    {
        public enum Type
        {
            Hp,
            Mp,
            Lp,
            Shield
        }
        public Type type;
        public int[] value;
        public string color;  // 新增：服务端发送的颜色（十六进制，带#）
        public override void Processed()
        {
            //Game.Data.Type worldDataType = (Game.Data.Type)Enum.Parse(typeof(Game.Data.Type), type.ToString());
            DataManager.Instance.Change(type, new { value = value, color = color });
        }

    }


    public class QuitToDesktop : Base
    {
        public override void Processed()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
        }
    }

    public class Story : Base
    {
        public class Dialogue
        {
            public string character;  // 发言者名称，空字符串表示旁白
            public string words;      // 对白内容
        }

        public List<Dialogue> dialogues;

        public override void Processed()
        {
            DataManager.Instance.StoryDialogues = dialogues?.Select(d => d.ToLogic()).ToList();
        }
    }

    /// <summary>
    /// Story complete callback protocol (upstream)
    /// Client sends this when Story UI is closed
    /// </summary>
    public class StoryComplete : Base { }

    /// <summary>
    /// Tutorial guidance protocol (downstream)
    /// Server sends this to highlight specific UI elements or game objects
    /// </summary>
    public class Tutorial : Base
    {
        /// <summary>Target type enumeration</summary>
        public enum TargetType
        {
            UI = 1,
            Map = 2,
            Creature = 3,
            Item = 4
        }

        public int step;           // Step ID: 0=clear, 1-99=normal guidance
        public int targetType;     // 1=UI, 2=Map, 3=Creature, 4=Item
        public int targetId;       // Config ID (for Creature/Item)
        public string targetPath;  // UI element path (for targetType=UI)
        public int[] targetPos;    // Map coordinates [x,y,z] (for targetType=Map)
        public string hint;        // Hint text (optional)

        /// <summary>
        /// Checks if this is a clear/hide command (step=0)
        /// </summary>
        public bool IsClear => step == 0;

        public override void Processed()
        {
            string posStr = targetPos != null ? $"[{string.Join(",", targetPos)}]" : "null";
            Utils.Debug.Log("Tutorial", $"Protocol received: step={step}, targetType={targetType}, targetId={targetId}, targetPath={targetPath}, targetPos={posStr}, hint={hint}");
            
            if (IsClear)
            {
                // step=0 means clear/hide current guidance
                Utils.Debug.Log("Tutorial", "Received clear command (step=0), clearing TutorialStep");
                DataManager.Instance.TutorialStep = null;
            }
            else
            {
                // Normal guidance step - convert to Logic model
                DataManager.Instance.TutorialStep = this.ToLogic();
            }
        }
    }

    public class Dark : Base
    {

        public string text;

        public override void Processed()
        {
            DataManager.Instance.Dark = text;
        }
    }

    public class WorldMap : Base
    {
        public class SceneInfo
        {
            public int[] pos;
            public string sceneName;
            public string color;
            public string type;
        }

        public List<SceneInfo> scenes;

        public override void Processed()
        {
            DataManager.Instance.WorldMap = this.ToLogic();
        }
    }

    public class FlyTip : Base
    {
        public string text;

        public override void Processed()
        {
            if (!string.IsNullOrEmpty(text))
            {
                DataManager.Instance.Tip = (TipType.Fly, text);
            }
        }
    }

    public class LanguageChanged : Base
    {
        public string language;

        public override void Processed()
        {
            if (System.Enum.TryParse(language, out DataManager.Languages lang))
            {
                DataManager.Instance.Language = lang;
            }
        }
    }

    public class ScreenAdaptation : Base
    {
        public int value;

        public override void Processed()
        {
            DataManager.Instance.User.ScreenUIAdaptation = value / 100f;
            Local.Instance.Save(DataManager.Instance.User);

            // Notify UI layer through event instead of direct call
            Game.Basic.Event.Instance.Fire("UI.ScreenUIAdaptation.Changed");
        }
    }

    public class UILock : Base
    {
        public List<string> unlockedPanels;

        public override void Processed()
        {
            DataManager.Instance.UILock = this.ToLogic();
        }
    }

    public class QuickStartRequest : Base
    {
        public string device;
        public string version;
        public string platform;
        public string language;
    }

    public class Texts : Base
    {
        public Dictionary<string, string> data;

        public override void Processed()
        {
            var existing = DataManager.Instance.Texts ?? new Dictionary<string, string>();
            foreach (var kv in data)
                existing[kv.Key] = kv.Value;
            DataManager.Instance.Texts = existing;
        }
    }

    public class RequestTexts : Base
    {
        public string language;
        public List<string> keys;

        public RequestTexts(string language, List<string> keys)
        {
            this.language = language;
            this.keys = keys;
        }
    }

}