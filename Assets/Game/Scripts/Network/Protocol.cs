using Google.Protobuf.WellKnownTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Game.Protocol
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
            TimeSpan roundTrip = DateTime.Now - Game.Data.Instance.Ping;
            TimeSpan serverDelta = dateTime - Game.Data.Instance.Ping;
            Utils.Debug.LogHeartbeat("Protocol", $"Pong已处理。服务器时间: {dateTime}, 往返时间: {roundTrip.TotalMilliseconds:F2}毫秒, 服务器处理时间: {serverDelta.TotalMilliseconds:F2}毫秒");
#endif
            Game.Data.Instance.Pong = dateTime;
        }
    }

    public class Login : Base
    {
        public Login(Account account)
        {
            id = account.Id;
            password = account.Password;
            version = Game.Data.Instance.AppVersion;
            platform = Application.platform.ToString();
            device = PlayerPrefs.GetString("DEVICE");
            language = Game.Data.Instance.Language.ToString();
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
        public override void Processed()
        {
            Utils.Debug.Log("Protocol", $"LoginResponse.Processed() called: code={code}, message={message}");
            Game.Data.Instance.LoginResponseMessage = message;
            Game.Data.Instance.LoginResponse = code;
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
            Game.Data.Instance.Initialize = this;
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
            Game.Data.Instance.InitialResponseMessage = message;
            Game.Data.Instance.InitialResponse = code;
        }
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
            
            string sceneKey = $"{pos[0]},{pos[1]},{pos[2]}";
            Game.Data.Instance.SceneCache[sceneKey] = this;
            
            Game.Data.Instance.Maps = sortedMaps;
            Game.Data.Instance.Pos = pos;
            Game.Data.Instance.SceneName = sceneName;
        }
    }
    public class Characters : Base
    {
        public class CharacterData
        {
            public string name;
            public double progress;
            public int hash;
        }
        
        public List<CharacterData> content;
        public override void Processed()
        {
            Game.Data.Instance.Characters = content;
        }
    }
    public class BattleProgress : Base
    {
        public List<KeyValuePair<string, double>> content;
        public override void Processed()
        {
            Game.Data.Instance.BattleProgress = content;
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
            Game.Data.Instance.Home = this;
            Game.Data.Instance.Area = area;
            if (!string.IsNullOrEmpty(scene.sceneName))
            {
                Game.Data.Instance.SceneName = scene.sceneName;
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
                Game.Data.Instance.Area = area;
            }
            Game.Data.Instance.Pos = content;
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
            List<Information> informations = new List<Information>();
            foreach (Information i in Game.Data.Instance.Informations) { informations.Add(i); }
            informations.Add(this);
            if (informations.Count > 250) { informations.RemoveAt(0); }
            Game.Data.Instance.Informations = informations;
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
            var currentOption = Game.Data.Instance.Option;
            if (currentOption == null || !AreEqual(currentOption))
            {
                Game.Data.Instance.Option = this;
            }
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
            Game.Data.Instance.Change(type, new { value = value, color = color });
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
            Game.Data.Instance.StoryDialogues = dialogues;
        }
    }
    public class Tutorial : Base
    {
        public int id;
        public override void Processed()
        {
            switch (id)
            {
                case 1:
                    Game.Data.Instance.Tutorial = new List<((string, string, int, bool), string, string)>
                    {
                        (Config.UI.Home, "Area/Viewport/Content/Item(Clone)", "请点击【自己】，打开功能。"),
                        (Config.UI.Option,"Right/Viewport/Content/OptionButton(Clone)", "请点击【记载】，打开任务。"),
                        (Config.UI.Option,"Right/Viewport/Content/OptionButton(Clone)", "请点击【任务】，打开任务详情。"),
                        (Config.UI.Option,"Right/Viewport/Content/OptionButton(Clone)", "请点击【自动】，等待任务自动完成获得的奖励。"),
                    };
                    Game.Data.Instance.TutorialIndex = 0;
                    break;
            }
        }
    }

    public class Dark : Base
    {

        public string text;

        public override void Processed()
        {
            Game.Data.Instance.Dark = text;
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
            Game.Data.Instance.WorldMap = this;
        }
    }

    public class FlyTip : Base
    {
        public string text;

        public override void Processed()
        {
            if (!string.IsNullOrEmpty(text))
            {
                Game.Data.Instance.Tip = (UI.Tips.Fly, text);
            }
        }
    }

    public class LanguageChanged : Base
    {
        public string language;

        public override void Processed()
        {
            if (System.Enum.TryParse(language, out Game.Data.Languages lang))
            {
                Game.Data.Instance.Language = lang;
            }
        }
    }

    public class ScreenAdaptation : Base
    {
        public int value;

        public override void Processed()
        {
            Game.Data.Instance.User.ScreenUIAdaptation = value / 100f;
            Game.Local.Instance.Save(Game.Data.Instance.User);

            var uiManager = UnityEngine.Object.FindObjectOfType<Game.UI>();
            if (uiManager != null)
            {
                uiManager.ApplyScreenUIAdaptation();
            }
        }
    }

}