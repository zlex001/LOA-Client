using UnityEngine;
using UnityEngine.UI;
using SuperScrollView;
using Game.Basic;
using Game.Data;
using Game.Net;
using System.Linq;
using UnityEditor;
using System.Collections.Generic;

namespace Game.Presentation
{
    using Protocol = Game.Net.Protocol;

    public class Home : UI.Core
    {
        private const float UnitHeight = 83f;

        public enum Event
        {
            ChannelToggle,
            SceneZoomIn,
            SceneZoomOut,
        }

        private bool IsWorldMapView => DataManager.Instance.SceneScale >= GetMaxVisibleCells() && DataManager.Instance.WorldMap != null && DataManager.Instance.WorldMap.scenes != null && DataManager.Instance.WorldMap.scenes.Count > 0;

        private float previousScale = 1.0f;
        private MapTransition mapTransition;

        public List<MapData> Maps
        {
            get
            {
                if (IsWorldMapView)
                {
                    var scenes = DataManager.Instance.WorldMap.scenes;
                    if (scenes == null || scenes.Count == 0)
                        return new List<MapData>();

                    int minX = scenes.Select(s => s.pos[0]).Min();
                    int maxX = scenes.Select(s => s.pos[0]).Max();
                    int minY = scenes.Select(s => s.pos[1]).Min();
                    int maxY = scenes.Select(s => s.pos[1]).Max();

                    List<MapData> result = new List<MapData>();
                    for (int y = maxY; y >= minY; y--)
                    {
                        for (int x = minX; x <= maxX; x++)
                        {
                            var scene = scenes.Find(s => s.pos[0] == x && s.pos[1] == y);
                            if (scene != null)
                            {
                                result.Add(new MapData
                                {
                                    pos = scene.pos,
                                    name = scene.sceneName,
                                    color = scene.color
                                });
                            }
                            else
                            {
                                result.Add(null);
                            }
                        }
                    }
                    return result;
                }
                return DataManager.Instance.Maps ?? DataManager.Instance.Home.scene.maps;
            }
        }

        public List<CharacterData> Characters => DataManager.Instance.Characters ?? DataManager.Instance.Home.characters;

        private int MinX => Maps != null && Maps.Count > 0 && Maps.Any(m => m != null) ? Maps.Where(m => m != null).Select(m => m.pos[0]).Min() : 0;
        private int MaxX => Maps != null && Maps.Count > 0 && Maps.Any(m => m != null) ? Maps.Where(m => m != null).Select(m => m.pos[0]).Max() : 0;
        private int MinY => Maps != null && Maps.Count > 0 && Maps.Any(m => m != null) ? Maps.Where(m => m != null).Select(m => m.pos[1]).Min() : 0;
        private int MaxY => Maps != null && Maps.Count > 0 && Maps.Any(m => m != null) ? Maps.Where(m => m != null).Select(m => m.pos[1]).Max() : 0;

        private bool _isFocused = true;

        private void ApplyAbsoluteLayout()
        {
            const float GoldenRatio = 0.618f;
            
            float screenWidth = GetComponent<RectTransform>().rect.width;
            float screenHeight = GetComponent<RectTransform>().rect.height;
            float sceneSize = screenWidth * GoldenRatio;
            float resourceHeight = UnitHeight * 1.5f;
            float fixedHeight = UnitHeight * 3.5f + sceneSize;
            float informationHeight = screenHeight - fixedHeight;

            SetRect("Chat", 0, UnitHeight * 2, screenWidth);
            SetRect("Information", UnitHeight * 2, informationHeight, screenWidth);
            SetRect("Scene", UnitHeight * 2 + informationHeight, sceneSize, sceneSize);
            SetRect("Area", UnitHeight * 2 + informationHeight, sceneSize, screenWidth - sceneSize, sceneSize);
            SetRect("Resource", screenHeight - resourceHeight, resourceHeight, screenWidth);
        }

        private void SetRect(string name, float y, float height, float width, float x = 0)
        {
            var rect = transform.Find(name).GetComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.zero;
            rect.pivot = Vector2.zero;
            rect.anchoredPosition = new Vector2(x, y);
            rect.sizeDelta = new Vector2(width, height);
        }

        #region Lifecycle 
        public override void OnCreate(params object[] args)
        {
            ApplyAbsoluteLayout();

            var scenePanel = transform.Find("Scene").gameObject;
            mapTransition = scenePanel.AddComponent<MapTransition>();
            UpdateSceneContainerSize();
            previousScale = DataManager.Instance.SceneScale;
            foreach (var resouse in DataManager.Instance.Home.resouse)
            {
                if (resouse.Key == "Pp") continue;

                float per = (float)resouse.Value.currentValue / resouse.Value.maxValue;
                transform.Find($"Resource/{resouse.Key}/Progress").GetComponent<Image>().fillAmount = per;
                transform.Find($"Resource/{resouse.Key}/Progress/Value").GetComponent<Text>().text = $"{resouse.Value.currentValue}/{resouse.Value.maxValue}";

                // 使用服务端发送的颜色
                if (!string.IsNullOrEmpty(resouse.Value.color))
                {
                    if (ColorUtility.TryParseHtmlString($"#{resouse.Value.color.TrimStart('#')}", out Color color))
                    {
                        transform.Find($"Resource/{resouse.Key}/Progress").GetComponent<Image>().color = color;
                    }
                }

                string resourceLabel = DataManager.Instance.GetText($"resource{resouse.Key}");
                if (!string.IsNullOrEmpty(resourceLabel))
                {
                    transform.Find($"Resource/{resouse.Key}/Title").GetComponent<Text>().text = resourceLabel;
                }
            }

            string[] channelNames = { "System", "Private", "Local", "Battle", "Broadcast", "Rumor", "Automation" };
            foreach (var channelName in channelNames)
            {
                string channelLabel = DataManager.Instance.GetText($"channel{channelName}");
                if (!string.IsNullOrEmpty(channelLabel))
                {
                    transform.Find($"Chat/Channel/{channelName}/Label").GetComponent<Text>().text = channelLabel;
                }
            }

            string chatPlaceholder = DataManager.Instance.GetText("chatPlaceholder");
            if (!string.IsNullOrEmpty(chatPlaceholder))
            {
                var placeholder = transform.Find("Chat/Input/Placeholder").GetComponent<Text>();
                if (placeholder != null)
                {
                    placeholder.text = chatPlaceholder;
                }
            }
            var sceneGridView = transform.Find("Scene").GetComponent<LoopGridView>();
            sceneGridView.InitGridView(Maps.Count, OnGetMapByIndex);
            sceneGridView.ItemSize = CalculateFillLayout();
            sceneGridView.SetGridFixedGroupCount(GridFixedType.ColumnCountFixed, MaxX - MinX + 1);
            sceneGridView.MovePanelToItemByRowColumn(MaxY - DataManager.Instance.Pos[1], DataManager.Instance.Pos[0] - MinX, transform.Find("Scene").GetComponent<RectTransform>().rect.width / 2, -transform.Find("Scene").GetComponent<RectTransform>().rect.height / 2);
            sceneGridView.mOnBeginDragAction = OnSceneBeginDrag;
            transform.Find("Scene/Focus").GetComponent<Button>().onClick.AddListener(OnSceneFocusClick);
            transform.Find("Scene/ZoomIn").GetComponent<Button>().onClick.AddListener(OnSceneZoomInClick);
            transform.Find("Scene/ZoomOut").GetComponent<Button>().onClick.AddListener(OnSceneZoomOutClick);
            transform.Find("Area").GetComponent<LoopListView2>().InitListView(Characters.Count, OnGetCharacterByIndex);
            transform.Find("Information").GetComponent<LoopListView2>().InitListView(DataManager.Instance.Informations.Count, OnGetMessageByIndex);
            foreach (Toggle toggle in transform.Find("Chat/Channel").GetComponentsInChildren<Toggle>(true)) { toggle.onValueChanged.AddListener((isOn) => Game.Basic.Event.Instance.Fire(Event.ChannelToggle, toggle.gameObject.name, isOn)); }
            Game.Basic.Event.Instance.Add(Event.ChannelToggle, OnChannelToggle);
            transform.Find("Chat/Input").GetComponent<InputField>().onEndEdit.AddListener(OnChatEndEdit);

            UpdateSceneInfo();

            DataManager.Instance.after.Register(Protocol.DataPair.Type.Hp, OnAfterHpChange);
            DataManager.Instance.after.Register(Protocol.DataPair.Type.Mp, OnAfterMpChange);
            DataManager.Instance.after.Register(Protocol.DataPair.Type.Lp, OnAfterLpChange);
            DataManager.Instance.after.Register(Protocol.DataPair.Type.Shield, OnAfterShieldChange);
            DataManager.Instance.after.Register(DataManager.Type.Pos, OnAfterPosChanged);
            DataManager.Instance.after.Register(DataManager.Type.SceneName, OnAfterSceneNameChanged);
            DataManager.Instance.after.Register(DataManager.Type.Maps, OnAfterMapsChanged);
            DataManager.Instance.after.Register(DataManager.Type.Characters, OnAfterCharactersChanged);
            DataManager.Instance.after.Register(DataManager.Type.BattleProgress, OnAfterBattleProgressChanged);
            DataManager.Instance.after.Register(DataManager.Type.Informations, OnAfterInformationsChanged);
            DataManager.Instance.after.Register(DataManager.Type.SceneScale, OnAfterSceneScaleChanged);
            DataManager.Instance.after.Register(DataManager.Type.Home, OnAfterHomeChanged);
            DataManager.Instance.after.Register(DataManager.Type.WorldMap, OnAfterWorldMapChanged);
            DataManager.Instance.after.Register(DataManager.Type.UILock, OnAfterUILockChanged);

            // Apply UILock state on creation
            ApplyUILock();
        }

        public override void OnClose()
        {
            DataManager.Instance.after.Unregister(Protocol.DataPair.Type.Hp, OnAfterHpChange);
            DataManager.Instance.after.Unregister(Protocol.DataPair.Type.Mp, OnAfterMpChange);
            DataManager.Instance.after.Unregister(Protocol.DataPair.Type.Lp, OnAfterLpChange);
            DataManager.Instance.after.Unregister(Protocol.DataPair.Type.Shield, OnAfterShieldChange);
            DataManager.Instance.after.Unregister(DataManager.Type.Pos, OnAfterPosChanged);
            DataManager.Instance.after.Unregister(DataManager.Type.SceneName, OnAfterSceneNameChanged);
            DataManager.Instance.after.Unregister(DataManager.Type.Maps, OnAfterMapsChanged);
            DataManager.Instance.after.Unregister(DataManager.Type.Characters, OnAfterCharactersChanged);
            DataManager.Instance.after.Unregister(DataManager.Type.BattleProgress, OnAfterBattleProgressChanged);
            DataManager.Instance.after.Unregister(DataManager.Type.Informations, OnAfterInformationsChanged);
            DataManager.Instance.after.Unregister(DataManager.Type.Home, OnAfterHomeChanged);
            DataManager.Instance.after.Unregister(DataManager.Type.WorldMap, OnAfterWorldMapChanged);
            DataManager.Instance.after.Unregister(DataManager.Type.UILock, OnAfterUILockChanged);
        }

        public override void OnScreenAdaptationChanged()
        {
            ApplyAbsoluteLayout();
            transform.Find("Scene").GetComponent<LoopGridView>().ItemSize = CalculateFillLayout();
            transform.Find("Scene").GetComponent<LoopGridView>().RefreshAllShownItem();
            transform.Find("Area").GetComponent<LoopListView2>().RefreshAllShownItem();
            transform.Find("Information").GetComponent<LoopListView2>().RefreshAllShownItem();
        }
        #endregion

        private void OnAfterHpChange(params object[] args)
        {
            var data = args[0];
            int[] value = ((dynamic)data).value;
            string color = ((dynamic)data).color;

            float per = (float)value[0] / value[1];
            transform.Find("Resource/Hp/Progress").GetComponent<Image>().fillAmount = per;
            transform.Find("Resource/Hp/Progress/Value").GetComponent<Text>().text = $"{value[0]}   /   {value[1]}";

            // 使用服务端发送的颜色
            if (!string.IsNullOrEmpty(color))
            {
                if (ColorUtility.TryParseHtmlString($"#{color.TrimStart('#')}", out Color progressColor))
                {
                    transform.Find("Resource/Hp/Progress").GetComponent<Image>().color = progressColor;
                }
            }
        }

        private void OnAfterMpChange(params object[] args)
        {
            var data = args[0];
            int[] value = ((dynamic)data).value;
            string color = ((dynamic)data).color;

            float per = (float)value[0] / value[1];
            transform.Find("Resource/Mp/Progress").GetComponent<Image>().fillAmount = per;
            transform.Find("Resource/Mp/Progress/Value").GetComponent<Text>().text = $"{value[0]}/{value[1]}";

            // 使用服务端发送的颜色
            if (!string.IsNullOrEmpty(color))
            {
                if (ColorUtility.TryParseHtmlString($"#{color.TrimStart('#')}", out Color progressColor))
                {
                    transform.Find("Resource/Mp/Progress").GetComponent<Image>().color = progressColor;
                }
            }
        }

        private void OnAfterLpChange(params object[] args)
        {
            var data = args[0];
            int[] value = ((dynamic)data).value;
            string color = ((dynamic)data).color;

            float per = (float)value[0] / value[1];
            transform.Find("Resource/Lp/Progress").GetComponent<Image>().fillAmount = per;
            transform.Find("Resource/Lp/Progress/Value").GetComponent<Text>().text = $"{value[0]}/{value[1]}";

            // 使用服务端发送的颜色
            if (!string.IsNullOrEmpty(color))
            {
                if (ColorUtility.TryParseHtmlString($"#{color.TrimStart('#')}", out Color progressColor))
                {
                    transform.Find("Resource/Lp/Progress").GetComponent<Image>().color = progressColor;
                }
            }
        }


        private void OnAfterShieldChange(params object[] args)
        {
            int[] value = (int[])args[0];
            float per = (float)value[0] / value[1];
            transform.Find("Resource/Hp/Progress1").GetComponent<Image>().fillAmount = per;
            transform.Find("Resource/Hp/Progress1/Value").GetComponent<Text>().text = $"{value[0]}/{value[1]}";
        }


        private void OnSceneFocusClick()
        {
            _isFocused = true;
            Utils.SmoothScroll.MoveToRowColumnWithTime(this, transform.Find("Scene").GetComponent<LoopGridView>(), MaxY - DataManager.Instance.Pos[1], DataManager.Instance.Pos[0] - MinX, transform.Find("Scene").GetComponent<RectTransform>().rect.width / 2, -transform.Find("Scene").GetComponent<RectTransform>().rect.height / 2, 0.145924f);
        }

        private void OnSceneBeginDrag(UnityEngine.EventSystems.PointerEventData eventData)
        {
            _isFocused = false;
        }
        private void OnSceneZoomInClick()
        {
            Game.Basic.Event.Instance.Fire(Event.SceneZoomIn);
        }
        private void OnSceneZoomOutClick()
        {
            Game.Basic.Event.Instance.Fire(Event.SceneZoomOut);
        }
        private LoopGridViewItem OnGetMapByIndex(LoopGridView loop, int index, int row, int column)
        {
            if (index < 0 || index >= Maps.Count) { return null; }
            MapData map = Maps[index];
            LoopGridViewItem item = loop.NewListViewItem("Item");
            Vector2 itemSize = CalculateFillLayout();
            item.GetComponent<RectTransform>().sizeDelta = itemSize;
            if (map != null)
            {
                item.GetComponent<HomeMap>().Refresh(map);
            }
            else
            {
                item.gameObject.SetActive(false);
            }
            return item;
        }

        private void OnAfterPosChanged(params object[] args)
        {
            transform.Find("Scene").GetComponent<LoopGridView>().RefreshAllShownItem();
            UpdateSceneInfo();

            if (_isFocused)
            {
                transform.Find("Scene").GetComponent<LoopGridView>().MovePanelToItemByRowColumn(MaxY - DataManager.Instance.Pos[1], DataManager.Instance.Pos[0] - MinX, transform.Find("Scene").GetComponent<RectTransform>().rect.width / 2, -transform.Find("Scene").GetComponent<RectTransform>().rect.height / 2);
            }
        }

        private void OnAfterSceneNameChanged(params object[] args)
        {
            UpdateSceneInfo();
        }

        private void UpdateSceneInfo()
        {
            var sceneInfoText = transform.Find("Resource/Title/Text")?.GetComponent<Text>();
            if (sceneInfoText != null)
            {
                string sceneName = DataManager.Instance.SceneName;
                int[] pos = DataManager.Instance.Pos;
                
                string mapName = "";
                var maps = DataManager.Instance.Home?.scene?.maps;
                
                if (maps != null && pos != null)
                {
                    var currentMap = maps.FirstOrDefault(m => m != null && Enumerable.SequenceEqual(m.pos, pos));
                    mapName = currentMap?.name ?? "";
                }
                
                sceneInfoText.text = string.IsNullOrEmpty(mapName) 
                    ? $"{sceneName} {pos[0]},{pos[1]},{pos[2]}"
                    : $"{sceneName}-{mapName} {pos[0]},{pos[1]},{pos[2]}";
            }
        }

        private void OnAfterMapsChanged(params object[] args)
        {
            RefreshMapView();
        }
        private LoopListViewItem2 OnGetCharacterByIndex(LoopListView2 loop, int index)
        {
            if (index < 0 || index >= Characters.Count) { return null; }
            LoopListViewItem2 item = loop.NewListViewItem("Item");
            item.GetComponent<HomeCharacter>().Refresh(Characters[index]);
            item.GetComponent<RectTransform>().sizeDelta = new Vector2(transform.Find("Area").gameObject.GetComponent<RectTransform>().rect.width, transform.Find("Area").gameObject.GetComponent<RectTransform>().rect.height / 6);
            return item;
        }

        private void OnAfterCharactersChanged(params object[] args)
        {
            var areaListView = transform.Find("Area").GetComponent<LoopListView2>();
            var scrollRect = areaListView.ScrollRect;
            float originalPosition = scrollRect.verticalNormalizedPosition;

            areaListView.SetListItemCount(Characters.Count);
            scrollRect.verticalNormalizedPosition = originalPosition;
            areaListView.RefreshAllShownItem();
        }

        private void OnAfterBattleProgressChanged(params object[] args)
        {
            List<KeyValuePair<string, double>> battleProgress = (List<KeyValuePair<string, double>>)args[0];
            UpdateBattleProgress(battleProgress);
        }

        private void UpdateBattleProgress(List<KeyValuePair<string, double>> battleProgress)
        {
            LoopListView2 listView = transform.Find("Area").GetComponent<LoopListView2>();

            for (int i = 0; i < battleProgress.Count; i++)
            {
                LoopListViewItem2 item = listView.GetShownItemByIndex(i);
                if (item != null)
                {
                    HomeCharacter homeCharacter = item.GetComponent<HomeCharacter>();
                    if (homeCharacter != null)
                    {
                        homeCharacter.UpdateBattleProgress(battleProgress[i].Value);
                    }
                }
            }
        }
        private LoopListViewItem2 OnGetMessageByIndex(LoopListView2 listView, int index)
        {
            List<InformationData> informationDataList = DataManager.Instance.Informations;
            if (index < 0 || index >= informationDataList.Count) { return null; }
            LoopListViewItem2 item = listView.NewListViewItem("Item");
            item.transform.Find("Text").GetComponent<Text>().text = informationDataList[index].message;
            item.GetComponent<RectTransform>().sizeDelta = new Vector2(transform.Find("Information/Viewport").GetComponent<RectTransform>().rect.width, item.transform.Find("Text").GetComponent<Text>().preferredHeight + 38);
            item.transform.Find("Text").GetComponent<RectTransform>().sizeDelta = new Vector2(transform.Find("Information/Viewport").GetComponent<RectTransform>().rect.width, item.transform.Find("Text").GetComponent<Text>().preferredHeight);
            return item;
        }
        private void OnAfterInformationsChanged(params object[] args)
        {
            var informationListView = transform.Find("Information").GetComponent<LoopListView2>();
            var scrollRect = informationListView.ScrollRect;

            bool isAtBottom = scrollRect.verticalNormalizedPosition <= 1E-6f;
            float originalPosition = scrollRect.verticalNormalizedPosition;

            informationListView.SetListItemCount(DataManager.Instance.Informations.Count);

            if (isAtBottom)
            {
                informationListView.MovePanelToItemIndex(DataManager.Instance.Informations.Count - 1, 0);
            }
            else
            {
                scrollRect.verticalNormalizedPosition = originalPosition;
            }

            informationListView.RefreshAllShownItem();
        }
        private Vector2 CalculateFillLayout()
        {
            RectTransform containerRect = transform.Find("Scene").GetComponent<RectTransform>();
            float containerSize = Mathf.Min(containerRect.rect.width, containerRect.rect.height);

            if (IsWorldMapView)
            {
                var scenes = DataManager.Instance.WorldMap.scenes;
                if (scenes != null && scenes.Count > 0)
                {
                    int minX = scenes.Select(s => s.pos[0]).Min();
                    int maxX = scenes.Select(s => s.pos[0]).Max();
                    int minY = scenes.Select(s => s.pos[1]).Min();
                    int maxY = scenes.Select(s => s.pos[1]).Max();

                    int columnCount = maxX - minX + 1;
                    int rowCount = maxY - minY + 1;
                    int maxCellCount = Mathf.Max(columnCount, rowCount);

                    float cellSize = containerSize / maxCellCount;
                    return new Vector2(cellSize, cellSize);
                }
            }

            float cellSize2 = containerSize / DataManager.Instance.SceneScale;
            return new Vector2(cellSize2, cellSize2);
        }

        public float GetMaxVisibleCells()
        {
            return DataManager.Instance.GetMaxSceneScale();
        }

        private void UpdateSceneContainerSize()
        {
            RectTransform containerRect = transform.Find("Scene").GetComponent<RectTransform>();
            DataManager.Instance.SceneContainerSize = Mathf.Min(containerRect.rect.width, containerRect.rect.height);
        }

        private void OnAfterSceneScaleChanged(params object[] args)
        {
            float newScale = (float)args[0];
            float maxScale = GetMaxVisibleCells();

            if (newScale < maxScale)
            {
                DataManager.Instance.SceneScaleBeforeWorldMap = newScale;
            }

            bool wasWorldMap = previousScale >= maxScale;
            bool isWorldMap = newScale >= maxScale;

            if (wasWorldMap != isWorldMap)
            {
                if (mapTransition != null)
                {
                    mapTransition.PlayTransition(() =>
                    {
                        RefreshMapView();
                    });
                }
                else
                {
                    RefreshMapView();
                }
            }
            else
            {
                transform.Find("Scene").GetComponent<LoopGridView>().ItemSize = CalculateFillLayout();
            }

            previousScale = newScale;
        }

        private void OnAfterWorldMapChanged(params object[] args)
        {
            RefreshMapView();
        }

        private void RefreshMapView()
        {
            var sceneGridView = transform.Find("Scene").GetComponent<LoopGridView>();
            int mapCount = Maps.Count;
            int columnCount = MaxX - MinX + 1;

            sceneGridView.ItemSize = CalculateFillLayout();
            sceneGridView.SetListItemCount(mapCount);
            sceneGridView.SetGridFixedGroupCount(GridFixedType.ColumnCountFixed, columnCount);

            int[] currentPos = DataManager.Instance.Pos;
            if (IsWorldMapView && currentPos != null && currentPos.Length >= 2)
            {
                sceneGridView.MovePanelToItemByRowColumn(MaxY - currentPos[1], currentPos[0] - MinX, transform.Find("Scene").GetComponent<RectTransform>().rect.width / 2, -transform.Find("Scene").GetComponent<RectTransform>().rect.height / 2);
            }
            else if (!IsWorldMapView && currentPos != null && currentPos.Length >= 2)
            {
                sceneGridView.MovePanelToItemByRowColumn(MaxY - currentPos[1], currentPos[0] - MinX, transform.Find("Scene").GetComponent<RectTransform>().rect.width / 2, -transform.Find("Scene").GetComponent<RectTransform>().rect.height / 2);
            }

            sceneGridView.RefreshAllShownItem();
        }
        private void OnChannelToggle(params object[] args)
        {
            transform.Find("Information").GetComponent<LoopListView2>().SetListItemCount(DataManager.Instance.Informations.Count);
            transform.Find("Information").GetComponent<LoopListView2>().MovePanelToItemIndex(DataManager.Instance.Informations.Count - 1, 0);
            transform.Find("Information").GetComponent<LoopListView2>().RefreshAllShownItem();
        }

        private void OnChatEndEdit(string text)
        {
            NetManager.Instance.Send(new Protocol.Chat(text));
        }

        void Update()
        {
            if (Input.GetKeyDown(KeyCode.F1))
            {
                Story.Test();
            }
        }

        private void OnAfterHomeChanged(params object[] args)
        {
            Protocol.Home home = (Protocol.Home)args[0];

            string[] resourceKeys = { "Hp", "Mp", "Lp" };
            foreach (var key in resourceKeys)
            {
                string label = DataManager.Instance.GetText($"resource{key}");
                if (!string.IsNullOrEmpty(label))
                {
                    var titleObj = transform.Find($"Resource/{key}/Title");
                    if (titleObj != null)
                    {
                        titleObj.GetComponent<Text>().text = label;
                    }
                }
            }

            string[] channelNames = { "System", "Private", "Local", "Battle", "Broadcast", "Rumor", "Automation" };
            foreach (var channelName in channelNames)
            {
                string channelLabel = DataManager.Instance.GetText($"channel{channelName}");
                if (!string.IsNullOrEmpty(channelLabel))
                {
                    var labelObj = transform.Find($"Chat/Channel/{channelName}/Label");
                    if (labelObj != null)
                    {
                        labelObj.GetComponent<Text>().text = channelLabel;
                    }
                }
            }

            string chatPlaceholder = DataManager.Instance.GetText("chatPlaceholder");
            if (!string.IsNullOrEmpty(chatPlaceholder))
            {
                var placeholder = transform.Find("Chat/Input/Placeholder")?.GetComponent<Text>();
                if (placeholder != null)
                {
                    placeholder.text = chatPlaceholder;
                }
            }

            if (home.scene?.maps != null)
            {
                transform.Find("Scene").GetComponent<LoopGridView>().RefreshAllShownItem();
            }

            UpdateSceneInfo();
        }

        private void ApplyUILock()
        {
            var uiLock = DataManager.Instance.UILock;
            
            // If no UILock or empty list, show all panels (compatible with old players)
            if (uiLock == null || uiLock.unlockedPanels == null || uiLock.unlockedPanels.Count == 0)
            {
                SetAllPanelsVisible(true);
                ApplyAbsoluteLayout();
                return;
            }
            
            // Check which panels are unlocked
            bool sceneUnlocked = uiLock.unlockedPanels.Contains("Home.Scene");
            bool areaUnlocked = uiLock.unlockedPanels.Contains("Home.Area");
            bool infoUnlocked = uiLock.unlockedPanels.Contains("Home.Information");
            bool resourceUnlocked = uiLock.unlockedPanels.Contains("Home.Resource");
            bool chatUnlocked = uiLock.unlockedPanels.Contains("Home.Chat");
            
            // Set panel visibility
            transform.Find("Scene").gameObject.SetActive(sceneUnlocked);
            transform.Find("Area").gameObject.SetActive(areaUnlocked);
            transform.Find("Information").gameObject.SetActive(infoUnlocked);
            transform.Find("Resource").gameObject.SetActive(resourceUnlocked);
            transform.Find("Chat").gameObject.SetActive(chatUnlocked);
            
            // Adjust layout based on unlocked state
            if (sceneUnlocked && !areaUnlocked && !infoUnlocked && !resourceUnlocked && !chatUnlocked)
            {
                // Only Scene: fullscreen layout
                ApplyFullScreenSceneLayout();
            }
            else
            {
                // Other cases: standard layout
                ApplyAbsoluteLayout();
            }
        }

        private void SetAllPanelsVisible(bool visible)
        {
            transform.Find("Scene").gameObject.SetActive(visible);
            transform.Find("Area").gameObject.SetActive(visible);
            transform.Find("Information").gameObject.SetActive(visible);
            transform.Find("Resource").gameObject.SetActive(visible);
            transform.Find("Chat").gameObject.SetActive(visible);
        }

        private void ApplyFullScreenSceneLayout()
        {
            float screenWidth = GetComponent<RectTransform>().rect.width;
            float screenHeight = GetComponent<RectTransform>().rect.height;
            
            // Scene fullscreen
            SetRect("Scene", 0, screenHeight, screenWidth, 0);
            
            // Update Scene GridView
            var sceneGridView = transform.Find("Scene").GetComponent<LoopGridView>();
            if (sceneGridView != null)
            {
                sceneGridView.ItemSize = CalculateFillLayout();
                sceneGridView.RefreshAllShownItem();
            }
        }

        private void OnAfterUILockChanged(params object[] args)
        {
            ApplyUILock();
        }
    }
}