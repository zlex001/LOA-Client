using System;
using System.Collections.Generic;
using System.Linq;
using Game.Basic;

namespace Game.Data
{
    public partial class DataManager
    {
        #region Game Data Properties
        public InitializeData Initialize { get => Get<InitializeData>(Type.Initialize); set => Change(Type.Initialize, value); }
        public int InitialResponse { get => Get<int>(Type.InitialResponse); set => Change(Type.InitialResponse, value); }
        public string InitialResponseMessage { get => Get<string>(Type.InitialResponseMessage); set => Change(Type.InitialResponseMessage, value); }
        public HomeData Home { get => Get<HomeData>(Type.Home); set => Change(Type.Home, value); }
        public List<MapData> Maps { get => Get<List<MapData>>(Type.Maps); set => Change(Type.Maps, value); }
        public int[] Pos { get => Get<int[]>(Type.Pos) ?? Home?.scene?.pos; set => Change(Type.Pos, value); }
        public string SceneName { get => Get<string>(Type.SceneName) ?? Home?.scene?.sceneName ?? ""; set => Change(Type.SceneName, value); }
        public List<int[]> Area { get => Get<List<int[]>>(Type.Area) ?? Home?.area; set => Change(Type.Area, value); }
        public List<CharacterData> Characters { get => Get<List<CharacterData>>(Type.Characters); set => Change(Type.Characters, value); }
        public List<KeyValuePair<string, double>> BattleProgress { get => Get<List<KeyValuePair<string, double>>>(Type.BattleProgress); set => Change(Type.BattleProgress, value); }
        public DateTime OptionReturn { get => Get<DateTime>(Type.OptionReturn); set => Change(Type.OptionReturn, value); }
        public List<((string, string, int, bool), string, string)> Tutorial { get => Get<List<((string, string, int, bool), string, string)>>(Type.Tutorial); set => Change(Type.Tutorial, value); }
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

        public Dictionary<string, string> Texts { get => Get<Dictionary<string, string>>(Type.Texts); set => Change(Type.Texts, value); }
        #endregion

        #region Home Event Handlers

        private void OnHomeChannelToggle(params object[] args)
        {
            string name = (string)args[0];
            bool isOn = (bool)args[1];
            HomeChannelToggle[name] = isOn;
        }

        private void OnInitializeConfirmClick(params object[] args)
        {
            string name = (string)args[0];
            Basic.Event.Instance.Fire(InitializeClick.Confirm, name);
        }

        private void OnInitializeRandomClick(params object[] args)
        {
            Basic.Event.Instance.Fire(InitializeClick.Random);
        }

        #endregion

        #region Data Utilities

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
