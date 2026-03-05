using System;
using System.Collections.Generic;

namespace Game.Data
{
    /// <summary>
    /// Map data model - represents a map tile
    /// </summary>
    public class MapData
    {
        public int[] pos { get; set; }
        public string name { get; set; }
        public string color { get; set; }
    }

    /// <summary>
    /// Scene data model
    /// </summary>
    public class SceneData
    {
        public int[] pos { get; set; }
        public List<MapData> maps { get; set; }
        public string sceneName { get; set; }
        public List<MapData> sortedMaps { get; set; }
    }

    /// <summary>
    /// Character data model
    /// </summary>
    public class CharacterData
    {
        public string name { get; set; }
        public double progress { get; set; }
        public int hash { get; set; }
        public int configId { get; set; }
    }

    /// <summary>
    /// Resource information
    /// </summary>
    public class ResourceInfo
    {
        public int currentValue { get; set; }
        public int maxValue { get; set; }
        public string color { get; set; }
    }

    /// <summary>
    /// Home UI texts
    /// </summary>
    public class HomeUITexts
    {
        public Dictionary<string, string> resourceLabels { get; set; }
        public string[] channels { get; set; }
        public string chatPlaceholder { get; set; }
    }

    /// <summary>
    /// Home data model
    /// </summary>
    public class HomeData
    {
        public SceneData scene { get; set; }
        public List<CharacterData> characters { get; set; }
        public Dictionary<string, ResourceInfo> resouse { get; set; }  // Note: typo matches Protocol
        public List<int[]> area { get; set; }
        public HomeUITexts ui { get; set; }
    }

    /// <summary>
    /// Initialize UI texts
    /// </summary>
    public class InitializeUITexts
    {
        public string namePlaceholder { get; set; }
        public string randomButton { get; set; }
        public string confirmButton { get; set; }
        public string errorNameEmpty { get; set; }
        public string errorNameUnsafe { get; set; }
    }

    /// <summary>
    /// Initialize data model
    /// </summary>
    public class InitializeData
    {
        public string description { get; set; }
        public Dictionary<string, int> grade { get; set; }
        public InitializeUITexts ui { get; set; }
    }

    /// <summary>
    /// Option item type
    /// </summary>
    public enum OptionItemType
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

    /// <summary>
    /// Option item data model
    /// </summary>
    public class OptionItemData
    {
        public OptionItemType type { get; set; }
        public Dictionary<string, string> data { get; set; }
    }

    /// <summary>
    /// Option data model
    /// </summary>
    public class OptionData
    {
        public List<OptionItemData> lefts { get; set; }
        public List<OptionItemData> rights { get; set; }
        public bool Empty => lefts == null && rights == null;
    }

    /// <summary>
    /// Scene information for world map
    /// </summary>
    public class WorldMapSceneInfo
    {
        public int[] pos { get; set; }
        public string sceneName { get; set; }
        public string color { get; set; }
        public string type { get; set; }
    }

    /// <summary>
    /// World map data model
    /// </summary>
    public class WorldMapData
    {
        public List<WorldMapSceneInfo> scenes { get; set; }
    }

    /// <summary>
    /// UI lock data model
    /// </summary>
    public class UILockData
    {
        public List<string> unlockedPanels { get; set; }
    }

    /// <summary>
    /// Tutorial target type
    /// </summary>
    public enum TutorialTargetType
    {
        UI = 1,
        Map = 2,
        Creature = 3,
        Item = 4
    }

    /// <summary>
    /// Tutorial data model
    /// </summary>
    public class TutorialData
    {
        public int step { get; set; }
        public int targetType { get; set; }
        public int targetId { get; set; }
        public string targetPath { get; set; }
        public int[] targetPos { get; set; }
        public string hint { get; set; }
        public bool IsClear => step == 0;
    }

    /// <summary>
    /// Story dialogue data model
    /// </summary>
    public class DialogueData
    {
        public string character { get; set; }
        public string words { get; set; }
    }

    /// <summary>
    /// Information message channel
    /// </summary>
    public enum InformationChannel
    {
        System,
        Private,
        Local,
        Battle,
        All,
        Rumor,
        Automation,
    }

    /// <summary>
    /// Information message data model
    /// </summary>
    public class InformationData
    {
        public string message { get; set; }
        public InformationChannel channel { get; set; }
    }

}
