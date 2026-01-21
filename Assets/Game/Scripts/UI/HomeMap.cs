using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using System;
using System.Collections.Generic;

namespace Game
{
    public class HomeMap : MonoBehaviour
    {
        private Map map;
        
        private Image tileImage;
        private Image borderImage;
        private Text nameText;

        void Start()
        {
            GetComponent<Button>().onClick.AddListener(OnClick);
            Data.Instance.after.Register(Data.Type.Area, OnAreaChanged);
            Data.Instance.after.Register(Data.Type.SceneScale, OnSceneScaleChanged);
            Data.Instance.after.Register(Data.Type.WorldMap, OnWorldMapChanged);
            Data.Instance.after.Register(Data.Type.SceneName, OnSceneNameChanged);
            
            EnsureComponentsCached();
        }

        void OnDestroy()
        {
            Data.Instance.after.Unregister(Data.Type.Area, OnAreaChanged);
            Data.Instance.after.Unregister(Data.Type.SceneScale, OnSceneScaleChanged);
            Data.Instance.after.Unregister(Data.Type.WorldMap, OnWorldMapChanged);
            Data.Instance.after.Unregister(Data.Type.SceneName, OnSceneNameChanged);
        }

        private void EnsureComponentsCached()
        {
            if (tileImage == null)
                tileImage = GetComponent<Image>();
            if (borderImage == null)
                borderImage = transform.Find("Border").GetComponent<Image>();
            if (nameText == null)
                nameText = transform.Find("Name").GetComponent<Text>();
        }

        public void Refresh(Map map)
        {
            this.map = map;
            
            EnsureComponentsCached();
            
            bool parseSuccess = ColorUtility.TryParseHtmlString(map.color, out var c);
            tileImage.color = parseSuccess ? c : Color.white;
            borderImage.color = new Color(255, 255, 255, Enumerable.SequenceEqual(map.pos, Data.Instance.Pos) ? 16 : 0);
            nameText.fontSize = (int)(Data.Instance.SceneContainerSize * 38 / 180 / Data.Instance.SceneScale);
            
            
            InitializeTransparency();
        }

        private void OnAreaChanged(params object[] args)
        {
            List<int[]> newArea = (List<int[]>)args[0];
            UpdateTransparency();
        }

        private void OnSceneScaleChanged(params object[] args)
        {
            if (nameText != null)
            {
                nameText.fontSize = (int)(Data.Instance.SceneContainerSize * 38 / 180 / Data.Instance.SceneScale);
            }
            UpdateTransparency();
        }

        private void OnWorldMapChanged(params object[] args)
        {
            UpdateTransparency();
        }

        private void OnSceneNameChanged(params object[] args)
        {
            UpdateTransparency();
        }

        private bool ContainsPos(List<int[]> area, int[] pos)
        {
            if (area == null || pos == null)
            {
                return false;
            }
            
            bool found = area.Any(p =>
                p != null && p.Length == pos.Length && 
                p.Length >= 3 && pos.Length >= 3 &&
                p[0] == pos[0] && p[1] == pos[1] && p[2] == pos[2]);
            
            return found;
        }

        private bool IsWorldMapView()
        {
            float maxScale = Data.Instance.GetMaxSceneScale();
            return Data.Instance.SceneScale >= maxScale && 
                   Data.Instance.WorldMap != null && 
                   Data.Instance.WorldMap.scenes != null && 
                   Data.Instance.WorldMap.scenes.Count > 0;
        }

        private void InitializeTransparency()
        {
            if (map != null)
            {
                EnsureComponentsCached();
                UpdateTransparency();
            }
        }

        private void UpdateTransparency()
        {
            if (map != null)
            {
                EnsureComponentsCached();
                
                var color = tileImage.color;
                
                if (IsWorldMapView())
                {
                    nameText.text = "";
                    
                    var currentSceneName = Data.Instance.SceneName;
                    bool isCurrentScene = !string.IsNullOrEmpty(currentSceneName) && 
                                         map.name == currentSceneName;
                    
                    color.a = isCurrentScene ? 1f : 0.15f;
                }
                else
                {
                    bool isWalkable = ContainsPos(Data.Instance.Area, map.pos);
                    color.a = isWalkable ? 1f : 0.04f;
                    nameText.text = color.a < 1f ? "" : map.name;
                }
                
                tileImage.color = color;
            }
        }


        private void OnClick()
        {
            int[] targetPos = new int[] { map.pos[0], map.pos[1], map.pos[2] };
            Utils.Debug.Log("HomeMap", $"[DEBUG-ClickMap] 点击格子 pos=[{targetPos[0]},{targetPos[1]},{targetPos[2]}], IsWorldMapView={IsWorldMapView()}, mapName={map?.name}");
            
            if (IsWorldMapView())
            {
                string playerSceneKey = $"{Data.Instance.Pos[0]},{Data.Instance.Pos[1]},{Data.Instance.Pos[2]}";
                if (Data.Instance.SceneCache.ContainsKey(playerSceneKey))
                {
                    var playerScene = Data.Instance.SceneCache[playerSceneKey];
                    Data.Instance.Maps = playerScene.sortedMaps;
                    Data.Instance.SceneName = playerScene.sceneName;
                }
                
                Data.Instance.SceneScale = Data.Instance.SceneScaleBeforeWorldMap;
            }
            
            Utils.Debug.Log("HomeMap", $"[DEBUG-ClickMap] 准备发送ClickMap到服务器");
            Net.Instance.Send(new Protocol.ClickMap(targetPos));
            Utils.Debug.Log("HomeMap", $"[DEBUG-ClickMap] 已发送ClickMap");
        }
    }
}