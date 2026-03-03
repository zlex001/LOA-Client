using System.Collections.Generic;
using System.Linq;
using Game.Data;

namespace Game.Net.Protocol
{
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
            public int configId;
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
            if (area != null)
            {
                DataManager.Instance.Area = area;
            }
            DataManager.Instance.Pos = content;
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
}
