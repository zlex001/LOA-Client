using DG.Tweening;
using Framework;
using Game.Data;
using Game.Utils;
using Config = Game.Data.Config;
using LitJson;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using Sequence = DG.Tweening.Sequence;
using UnityTimer;

namespace Game.Presentation
{
    public class Option : UI.Core
    {
        private const float UnitHeight = 83f;
        private const float GoldenRatio = 0.618f;
        private const float GoldenRatioSmall = 0.382f;
        private const float PanelHeightUnits = 10f;

        private OptionData lastOption;

        private const float PanelGap = UnitHeight * 0.25f;
        private const float AnimDuration = 0.1f;
        
        private void Animate(OptionData option)
        {
            int count = (option.lefts != null ? 1 : 0) + (option.rights != null ? 1 : 0);
            bool hasLeft = option.lefts != null;
            bool hasRight = option.rights != null;
            
            var parentRect = GetComponent<RectTransform>();
            float screenWidth = parentRect.rect.width;
            float panelWidth = screenWidth * GoldenRatioSmall;
            float panelHeight = UnitHeight * PanelHeightUnits;
            
            var leftRect = transform.Find("Left").GetComponent<RectTransform>();
            var rightRect = transform.Find("Right").GetComponent<RectTransform>();
            var leftContent = transform.Find("Left/Viewport/Content")?.GetComponent<RectTransform>();
            var rightContent = transform.Find("Right/Viewport/Content")?.GetComponent<RectTransform>();
            
            SetupFixedSizePanel(leftRect, panelWidth, panelHeight);
            SetupFixedSizePanel(rightRect, panelWidth, panelHeight);
            
            float centerX = 0f;
            float slideOffset = (panelWidth + PanelGap) / 2;
            float leftX = -slideOffset;
            float rightX = slideOffset;
            
            transform.Find("Left").SetAsLastSibling();
            
            if (count == 0)
            {
                bool isCenter = Mathf.Abs(leftRect.anchoredPosition.x - centerX) < 1f;
                
                if (isCenter)
                {
                    Close();
                }
                else
                {
                    var sequence = DOTween.Sequence();
                    sequence.Append(leftRect.DOAnchorPosX(centerX, AnimDuration).SetEase(Ease.InQuad));
                    sequence.Join(rightRect.DOAnchorPosX(centerX, AnimDuration).SetEase(Ease.InQuad));
                    sequence.OnUpdate(() => ForceRebuildContentLayout(leftContent, rightContent));
                    sequence.OnComplete(Close);
                }
                return;
            }
            
            transform.Find("Left").gameObject.SetActive(hasLeft || count == 2);
            transform.Find("Right").gameObject.SetActive(hasRight || count == 2);
            
            if (count == 1)
            {
                var activeRect = hasLeft ? leftRect : rightRect;
                float currentX = activeRect.anchoredPosition.x;
                bool needsSlide = Mathf.Abs(currentX - centerX) > 1f;
                
                if (needsSlide)
                {
                    var sequence = DOTween.Sequence();
                    sequence.Append(activeRect.DOAnchorPosX(centerX, AnimDuration).SetEase(Ease.OutQuad));
                    sequence.OnUpdate(() => ForceRebuildContentLayout(leftContent, rightContent));
                }
                else
                {
                    activeRect.anchoredPosition = new Vector2(centerX, 0);
                    ForceRebuildContentLayout(leftContent, rightContent);
                }
            }
            else
            {
                bool leftAtCenter = Mathf.Abs(leftRect.anchoredPosition.x - centerX) < 1f;
                bool rightAtCenter = Mathf.Abs(rightRect.anchoredPosition.x - centerX) < 1f;
                bool needsSlide = !(Mathf.Abs(leftRect.anchoredPosition.x - leftX) < 1f && 
                                    Mathf.Abs(rightRect.anchoredPosition.x - rightX) < 1f);
                
                if (!leftAtCenter && !rightAtCenter && !needsSlide)
                {
                    ForceRebuildContentLayout(leftContent, rightContent);
                    return;
                }
                
                leftRect.anchoredPosition = new Vector2(centerX, 0);
                rightRect.anchoredPosition = new Vector2(centerX, 0);
                ForceRebuildContentLayout(leftContent, rightContent);
                
                var openSequence = DOTween.Sequence();
                openSequence.Append(leftRect.DOAnchorPosX(leftX, AnimDuration).SetEase(Ease.OutQuad));
                openSequence.Join(rightRect.DOAnchorPosX(rightX, AnimDuration).SetEase(Ease.OutQuad));
                openSequence.OnUpdate(() => ForceRebuildContentLayout(leftContent, rightContent));
            }
        }
        
        private void SetupFixedSizePanel(RectTransform rect, float width, float height)
        {
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = new Vector2(width, height);
        }
        
        private void ForceRebuildContentLayout(RectTransform leftContent, RectTransform rightContent)
        {
            Canvas.ForceUpdateCanvases();
            if (leftContent != null && leftContent.gameObject.activeInHierarchy)
                LayoutRebuilder.ForceRebuildLayoutImmediate(leftContent);
            if (rightContent != null && rightContent.gameObject.activeInHierarchy)
                LayoutRebuilder.ForceRebuildLayoutImmediate(rightContent);
        }



        private Dictionary<OptionItemType, Func<GameObject, string, OptionItemData, GameObject>> creators;



        #region Fields & Properties

        private struct ElementIndices
        {
            public int text, progressWithValue, progress, radar, button, titleButton, titleButtonWithProgress, iapButton, slider, filter, input, toggleGroup, confirm, amount;
            public void Reset() { this = default; }
        }

        private ElementIndices indices;

        float PerButtonHeight => UnitHeight;
        #endregion

        #region Unity Lifecycle Methods
        public override void OnCreate(params object[] args)
        {
            creators = new()
            {
                { OptionItemType.Text,     (obj, path, item) => Text(obj, indices.text++, path, item) },
                { OptionItemType.Progress, (obj, path, item) => Progress(obj, indices.progress++, path, item) },
                { OptionItemType.ProgressWithValue, (obj, path, item) => ProgressWithValue(obj, indices.progressWithValue++, path, item) },
                { OptionItemType.Radar,    (obj, path, item) => Radar(obj, indices.radar++, path, item) }, // 保留 width
                { OptionItemType.Button,   (obj, path, item) => Button(obj, indices.button++, path, item) },
                { OptionItemType.TitleButton,   (obj, path, item) => TitleButton(obj, indices.titleButton++, path, item) },
                { OptionItemType.TitleButtonWithProgress,   (obj, path, item) => TitleButtonWithProgress(obj, indices.titleButtonWithProgress++, path, item) },
                { OptionItemType.IAPButton,(obj, path, item) => IAPButton(obj, indices.iapButton++, path, item) },
                { OptionItemType.Slider,   (obj, path, item) => Slider(obj, indices.slider++, path, item) },
                { OptionItemType.Input,    (obj, path, item) => Input(obj, indices.input++, path, item) },
                { OptionItemType.Filter,   (obj, path, item) => Filter(obj, indices.filter++, path, item) },
                { OptionItemType.ToggleGroup,(obj, path, item) => ToggleGroup(obj, indices.toggleGroup++, path, item) },
                { OptionItemType.Confirm,  (obj, path, item) => Confirm(obj, indices.confirm++, path, item) },
                { OptionItemType.Amount,   (obj, path, item) => Amount(obj, indices.amount++, path, item) },
            };
            lastOption = DataManager.Instance.Option;
            Refresh(transform.Find("Left/Viewport/Content").gameObject, DataManager.Instance.Option.lefts, 0);  // side = 0
            Refresh(transform.Find("Right/Viewport/Content").gameObject, DataManager.Instance.Option.rights, 1); // side = 1
            Animate(DataManager.Instance.Option);
            transform.Find("Return").GetComponent<Button>().onClick.AddListener(OnReturnClick);
            DataManager.Instance.after.Register(DataManager.Type.Option, OnAfterOptionChanged);
        }
        public override void OnClose()
        {
            DataManager.Instance.after.Unregister(DataManager.Type.Option, OnAfterOptionChanged);
        }

        public override void OnScreenAdaptationChanged()
        {
            StartCoroutine(DelayedAnimate());
        }

        private IEnumerator DelayedAnimate()
        {
            yield return null;
            if (lastOption != null)
            {
                Animate(lastOption);
            }
        }
        #endregion
        private void OnAfterOptionChanged(params object[] args)
        {
            var currentOption = DataManager.Instance.Option;
            if (lastOption != currentOption)
            {
                lastOption = currentOption;
                Animate(currentOption);
                Refresh(transform.Find("Left/Viewport/Content").gameObject, currentOption.lefts, 0);  // side = 0
                Refresh(transform.Find("Right/Viewport/Content").gameObject, currentOption.rights, 1); // side = 1
            }
        }

        private void OnReturnClick()
        {
            DataManager.Instance.OptionReturn = DateTime.Now;
        }

        #region Core Methods
        private void Refresh(GameObject content, List<OptionItemData> items, int side)
        {
            indices.Reset();
            if (items != null)
            {
                content.ClearChildren();
                List<GameObject> children = new List<GameObject>();
                for (int i = 0; i < items.Count; i++)
                {
                    children.Add(Create(content, "Prefabs/UI", items[i], side, i));
                }
                
                // 使用 ContentSizeFitter 让 Unity 自动计算高度
                var contentRect = content.GetComponent<RectTransform>();
                var layoutGroup = content.GetComponent<VerticalLayoutGroup>();
                
                // 给 VerticalLayoutGroup 添加 bottom padding，确保最后一行完全可见
                if (layoutGroup != null)
                {
                    var padding = layoutGroup.padding;
                    padding.bottom = (int)UnitHeight;
                    layoutGroup.padding = padding;
                }
                
                // 确保 Content 有 ContentSizeFitter 组件
                var sizeFitter = content.GetComponent<ContentSizeFitter>();
                if (sizeFitter == null)
                {
                    sizeFitter = content.AddComponent<ContentSizeFitter>();
                }
                sizeFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
                
                // 强制刷新布局
                LayoutRebuilder.ForceRebuildLayoutImmediate(contentRect);
            }
        }


        private GameObject Create(GameObject obj, string path, OptionItemData item, int side, int index)
        {
            var go = creators.TryGetValue(item.type, out var creator) ? creator(obj, path, item) : null;

            if (go != null && go.TryGetComponent<OptionItem>(out var itemComp))
            {
                itemComp.index = index;
                itemComp.SetSide(side);
            }

            return go;
        }
        #endregion

        private GameObject Text(GameObject obj, int index, string path, OptionItemData item)
        {
            GameObject text = obj.AddPrefab(path, "OptionText");
            var textComp = text.GetComponent<Text>();
            textComp.text = item.data["Text"];
            textComp.alignment = index == 0 ? TextAnchor.MiddleCenter : TextAnchor.MiddleLeft;
            textComp.fontStyle = index == 0 ? FontStyle.Bold : FontStyle.Normal;
            var rect = text.GetComponent<RectTransform>();
            text.GetComponent<RectTransform>().sizeDelta = new Vector2(0, 0);
            rect.sizeDelta = new Vector2(0,  textComp.preferredHeight);
            return text;
        }

        private GameObject Progress(GameObject obj, int index, string path, OptionItemData item)
        {
            GameObject progress = obj.AddPrefab(path, "OptionProgress");
            progress.transform.Find("Title").GetComponent<Text>().text = item.data["Text"];
            
            var values = JsonMapper.ToObject<int[]>(item.data["SliderValues"]);
            progress.transform.Find("Progress/Value").GetComponent<Image>().fillAmount = (float)values[0] / values[1];
            //progress.transform.Find("Progress/Text").GetComponent<Text>().text = item.data["ProgressText"];
            
            // 使用服务端发送的颜色（服务端现在总是发送颜色）
            if (item.data.ContainsKey("ValueColor") && !string.IsNullOrEmpty(item.data["ValueColor"]))
            {
                progress.transform.Find("Progress/Value").GetComponent<Image>().color =
                    ColorUtility.TryParseHtmlString($"#{item.data["ValueColor"].TrimStart('#')}", out var color) ? color : Color.white;
            }
            
            progress.GetComponent<RectTransform>().sizeDelta = new Vector2(0, PerButtonHeight * 0.5f);
            return progress;
        }

        private GameObject ProgressWithValue(GameObject obj, int index, string path, OptionItemData item)
        {
            GameObject progress = obj.AddPrefab(path, "OptionProgressWithValue");
            progress.transform.Find("Title").GetComponent<Text>().text = item.data["Text"];
            
            var values = JsonMapper.ToObject<int[]>(item.data["SliderValues"]);
            progress.transform.Find("Progress/Value").GetComponent<Image>().fillAmount = (float)values[0] / values[1];
            progress.transform.Find("Progress/Text").GetComponent<Text>().text = $"{values[0]}/{values[1]}";
            
            // 使用服务端发送的颜色（服务端现在总是发送颜色）
            if (item.data.ContainsKey("ValueColor") && !string.IsNullOrEmpty(item.data["ValueColor"]))
            {
                progress.transform.Find("Progress/Value").GetComponent<Image>().color =
                    ColorUtility.TryParseHtmlString($"#{item.data["ValueColor"].TrimStart('#')}", out var color) ? color : Color.white;
            }
            
            progress.GetComponent<RectTransform>().sizeDelta = new Vector2(0, PerButtonHeight * 0.5f);
            return progress;
        }

        private GameObject Radar(GameObject obj, int index, string path, OptionItemData item)
        {
            GameObject radar = obj.AddPrefab(path, "OptionRadar");
            OptionRadar option = radar.GetComponent<OptionRadar>();
            float width = obj.GetComponent<RectTransform>().rect.width;
            option.GetComponent<RectTransform>().sizeDelta = new Vector2(width, width);
            option.data = JsonMapper.ToObject<Dictionary<string, int>>(item.data["Radar"]);
            return radar;
        }

        private GameObject Slider(GameObject obj, int index, string path, OptionItemData item)
        {
            GameObject slider = obj.AddPrefab(path, "OptionSlider");
            OptionSlider option = slider.GetComponent<OptionSlider>();
            option.index = index;
            option.SetText(item.data["Text"]);
            option.SetValueColor(item.data["ValueColor"]);
            if (item.data.ContainsKey("Id"))
            {
                option.SetId(item.data["Id"]);
            }
            if (item.data.ContainsKey("Labels"))
            {
                option.SetLabels(item.data["Labels"]);
            }
            option.SetSlider(JsonMapper.ToObject<int[]>(item.data["SliderValues"]));
            slider.GetComponent<RectTransform>().sizeDelta = new Vector2(0, PerButtonHeight * 1.5f);
            return slider;
        }

        private GameObject Button(GameObject obj, int index, string path, OptionItemData item)
        {
            GameObject button = obj.AddPrefab(path, "OptionButton");
            OptionButton option = button.GetComponent<OptionButton>();
            option.Refresh(index, item);
            button.GetComponent<RectTransform>().sizeDelta = new Vector2(0, PerButtonHeight);
            return button;
        }
        private GameObject TitleButton(GameObject obj, int index, string path, OptionItemData item)
        {
            GameObject button = obj.AddPrefab(path, "OptionTitleButton");
            OptionTitleButton option = button.GetComponent<OptionTitleButton>();
            option.Refresh(index, item);
            button.GetComponent<RectTransform>().sizeDelta = new Vector2(0, PerButtonHeight * 0.5f);
            return button;
        }

        private GameObject TitleButtonWithProgress(GameObject obj, int index, string path, OptionItemData item)
        {
            GameObject button = obj.AddPrefab(path, "OptionTitleButtonWithProgress");
            if (button == null)
            {
                UnityEngine.Debug.LogError("预制体 OptionTitleButtonWithProgress 未找到！请在Unity中创建");
                return null;
            }
            
            OptionTitleButtonWithProgress option = button.GetComponent<OptionTitleButtonWithProgress>();
            if (option == null)
            {
                UnityEngine.Debug.LogError("预制体 OptionTitleButtonWithProgress 上缺少脚本组件！");
                return button;
            }
            
            option.Refresh(index, item);
            button.GetComponent<RectTransform>().sizeDelta = new Vector2(0, PerButtonHeight * 1f);
            return button;
        }

        private GameObject IAPButton(GameObject obj, int index, string path, OptionItemData item)
        {
            GameObject iap = obj.AddPrefab(path, "OptionIAPButton");
            OptionIAP option = iap.GetComponent<OptionIAP>();
            option.index = index;
            option.SetText(item.data["Text"]);
            option.SetProdut(item.data["ProductID"]);
            iap.GetComponent<RectTransform>().sizeDelta = new Vector2(0, PerButtonHeight);
            return iap;
        }

        private GameObject Confirm(GameObject obj, int index, string path, OptionItemData item)
        {
            GameObject confirm = obj.AddPrefab(path, "OptionConfirm");
            confirm.GetComponent<RectTransform>().sizeDelta = new Vector2(0, PerButtonHeight);
            return confirm;
        }

        private GameObject Input(GameObject obj, int index, string path, OptionItemData item)
        {
            GameObject input = obj.AddPrefab(path, "OptionInput");
            OptionInput option = input.GetComponent<OptionInput>();
            option.Refresh(index, item);
            input.GetComponent<RectTransform>().sizeDelta = new Vector2(0, PerButtonHeight);
            return input;
        }

        private GameObject Filter(GameObject obj, int index, string path, OptionItemData item)
        {
            GameObject filter = obj.AddPrefab(path, "OptionFilter");
            OptionFilter option = filter.GetComponent<OptionFilter>();
            option.Refresh(index, item);
            filter.GetComponent<RectTransform>().sizeDelta = new Vector2(0, PerButtonHeight);
            return filter;
        }

        private GameObject ToggleGroup(GameObject obj, int index, string path, OptionItemData item)
        {
            GameObject toggleGroup = obj.AddPrefab(path, "OptionToggleGroup");
            List<string> toggles = item.data["Text"].Split('、').ToList();
            
            for (int i = 0; i < toggles.Count; i++)
            {
                string t = toggles[i];
                string key = t.Split(':')[0];
                bool value = bool.Parse(t.Split(':')[1]);
                
                GameObject toggle = toggleGroup.AddPrefab("Prefabs/UI", "OptionToggle");
                toggle.GetComponent<Toggle>().group = transform.GetComponent<ToggleGroup>();
                OptionToggle toggleScript = toggle.GetComponent<OptionToggle>();
                toggleScript.text.text = key == "" ? "全部" : key;
                toggleScript.SetToggleValue(value);
            }
            
            // Use ContentSizeFitter to let Unity auto-calculate height based on GridLayoutGroup
            var sizeFitter = toggleGroup.GetComponent<ContentSizeFitter>();
            if (sizeFitter == null)
            {
                sizeFitter = toggleGroup.AddComponent<ContentSizeFitter>();
            }
            sizeFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            
            // Force layout rebuild
            LayoutRebuilder.ForceRebuildLayoutImmediate(toggleGroup.GetComponent<RectTransform>());
            return toggleGroup;
        }

        private GameObject Amount(GameObject obj, int index, string path, OptionItemData item)
        {
            GameObject amount = obj.AddPrefab(path, "OptionAmount");
            OptionAmount option = amount.GetComponent<OptionAmount>();
            option.SetTitle(item.data["Title"]);
            option.SetAmount(Convert.ToInt32(item.data["Amount"]));
            amount.GetComponent<RectTransform>().sizeDelta = new Vector2(0, PerButtonHeight * 2f);
            return amount;
        }

    }
}