using Game.Data;
using UnityEngine;
using UnityEngine.UI;
using System;
using System.Linq;

namespace Game.Presentation
{
    public partial class StartSettings
    {
        #region Settings UI Building

        private void BuildSettingsSection()
        {
            CreateSectionHeader(GetText("general"));
            CreateLanguageItem();
            CreateFontSizeItem();
            CreateSoundItem();
        }

        private void CreateLanguageItem()
        {
            var itemObj = new GameObject("LanguageItem");
            itemObj.transform.SetParent(_scrollContent.transform, false);

            var itemRect = itemObj.AddComponent<RectTransform>();
            itemRect.sizeDelta = new Vector2(0, UnitHeight);

            var itemImage = itemObj.AddComponent<Image>();
            if (_bottomBorderGradient != null) itemImage.sprite = _bottomBorderGradient;
            itemImage.type = Image.Type.Simple;
            itemImage.preserveAspect = false;
            itemImage.color = Color.white;

            var itemButton = itemObj.AddComponent<Button>();
            itemButton.targetGraphic = itemImage;
            itemButton.onClick.AddListener(OnLanguageItemClick);
            ApplyButtonColorBlock(itemButton);

            var labelObj = new GameObject("Label");
            labelObj.transform.SetParent(itemObj.transform, false);

            var labelRect = labelObj.AddComponent<RectTransform>();
            labelRect.anchorMin = new Vector2(0, 0);
            labelRect.anchorMax = new Vector2(0.5f, 1);
            labelRect.sizeDelta = Vector2.zero;
            labelRect.anchoredPosition = new Vector2(15, 0);

            var labelText = labelObj.AddComponent<Text>();
            labelText.text = GetText("language");
            labelText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            labelText.fontSize = 38;
            labelText.color = ColorTextPrimary;
            labelText.alignment = TextAnchor.MiddleLeft;
            labelObj.AddComponent<Framework.FontScaler>();

            var valueObj = new GameObject("Value");
            valueObj.transform.SetParent(itemObj.transform, false);

            var valueRect = valueObj.AddComponent<RectTransform>();
            valueRect.anchorMin = new Vector2(0.5f, 0);
            valueRect.anchorMax = new Vector2(1, 1);
            valueRect.sizeDelta = Vector2.zero;
            valueRect.anchoredPosition = new Vector2(-15, 0);

            var valueText = valueObj.AddComponent<Text>();
            valueText.text = GetLanguageName(DataManager.Instance.Language);
            valueText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            valueText.fontSize = 38;
            valueText.color = ColorTextSecondary;
            valueText.alignment = TextAnchor.MiddleRight;
            valueObj.AddComponent<Framework.FontScaler>();

            var padding = itemObj.AddComponent<LayoutElement>();
            padding.preferredHeight = UnitHeight;
        }

        private void CreateFontSizeItem()
        {
            var itemObj = new GameObject("FontSizeItem");
            itemObj.transform.SetParent(_scrollContent.transform, false);

            var itemRect = itemObj.AddComponent<RectTransform>();
            itemRect.sizeDelta = new Vector2(0, UnitHeight);

            var itemImage = itemObj.AddComponent<Image>();
            if (_bottomBorderGradient != null) itemImage.sprite = _bottomBorderGradient;
            itemImage.type = Image.Type.Simple;
            itemImage.preserveAspect = false;
            itemImage.color = Color.white;

            var labelObj = new GameObject("Label");
            labelObj.transform.SetParent(itemObj.transform, false);

            var labelRect = labelObj.AddComponent<RectTransform>();
            labelRect.anchorMin = new Vector2(0, 0);
            labelRect.anchorMax = new Vector2(0.5f, 1);
            labelRect.sizeDelta = Vector2.zero;
            labelRect.anchoredPosition = new Vector2(15, 0);

            var labelText = labelObj.AddComponent<Text>();
            labelText.text = GetText("font_size");
            labelText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            labelText.fontSize = 38;
            labelText.color = ColorTextPrimary;
            labelText.alignment = TextAnchor.MiddleLeft;
            labelObj.AddComponent<Framework.FontScaler>();

            var controlObj = new GameObject("Control");
            controlObj.transform.SetParent(itemObj.transform, false);

            var controlRect = controlObj.AddComponent<RectTransform>();
            controlRect.anchorMin = new Vector2(0.5f, 0.5f);
            controlRect.anchorMax = new Vector2(1, 0.5f);
            controlRect.pivot = new Vector2(1, 0.5f);
            controlRect.sizeDelta = new Vector2(-15, 50);
            controlRect.anchoredPosition = new Vector2(-15, 0);

            var layoutGroup = controlObj.AddComponent<HorizontalLayoutGroup>();
            layoutGroup.childAlignment = TextAnchor.MiddleRight;
            layoutGroup.childControlWidth = false;
            layoutGroup.childControlHeight = false;
            layoutGroup.childForceExpandWidth = false;
            layoutGroup.childForceExpandHeight = false;
            layoutGroup.spacing = 10;

            var minusButtonObj = new GameObject("MinusButton");
            minusButtonObj.transform.SetParent(controlObj.transform, false);

            var minusButtonRect = minusButtonObj.AddComponent<RectTransform>();
            minusButtonRect.sizeDelta = new Vector2(40, 40);

            var minusButtonImage = minusButtonObj.AddComponent<Image>();
            minusButtonImage.color = ColorTransparent;

            var minusButton = minusButtonObj.AddComponent<Button>();
            minusButton.targetGraphic = minusButtonImage;
            minusButton.onClick.AddListener(OnFontSizeDecrease);
            ApplyButtonColorBlock(minusButton, transparent: true);

            var minusTextObj = new GameObject("Text");
            minusTextObj.transform.SetParent(minusButtonObj.transform, false);

            var minusTextRect = minusTextObj.AddComponent<RectTransform>();
            minusTextRect.anchorMin = Vector2.zero;
            minusTextRect.anchorMax = Vector2.one;
            minusTextRect.sizeDelta = Vector2.zero;

            var minusText = minusTextObj.AddComponent<Text>();
            minusText.text = "-";
            minusText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            minusText.fontSize = 48;
            minusText.color = ColorTextPrimary;
            minusText.alignment = TextAnchor.MiddleCenter;

            var valueTextObj = new GameObject("ValueText");
            valueTextObj.transform.SetParent(controlObj.transform, false);

            var valueTextRect = valueTextObj.AddComponent<RectTransform>();
            valueTextRect.sizeDelta = new Vector2(200, 40);

            var valueText = valueTextObj.AddComponent<Text>();
            valueText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            valueText.fontSize = 36;
            valueText.color = ColorTextSecondary;
            valueText.alignment = TextAnchor.MiddleCenter;
            valueTextObj.AddComponent<Framework.FontScaler>();

            int[] fontSizes = { 25, 30, 35, 40 };
            string[] fontLabels = {
                GetText("font_size_small"),
                GetText("font_size_medium"),
                GetText("font_size_large"),
                GetText("font_size_extra_large")
            };
            int currentFontSize = DataManager.Instance.FontSize;
            int currentIndex = System.Array.IndexOf(fontSizes, currentFontSize);
            if (currentIndex < 0) currentIndex = 1;
            valueText.text = fontLabels[currentIndex];

            var plusButtonObj = new GameObject("PlusButton");
            plusButtonObj.transform.SetParent(controlObj.transform, false);

            var plusButtonRect = plusButtonObj.AddComponent<RectTransform>();
            plusButtonRect.sizeDelta = new Vector2(40, 40);

            var plusButtonImage = plusButtonObj.AddComponent<Image>();
            plusButtonImage.color = ColorTransparent;

            var plusButton = plusButtonObj.AddComponent<Button>();
            plusButton.targetGraphic = plusButtonImage;
            plusButton.onClick.AddListener(OnFontSizeIncrease);
            ApplyButtonColorBlock(plusButton, transparent: true);

            var plusTextObj = new GameObject("Text");
            plusTextObj.transform.SetParent(plusButtonObj.transform, false);

            var plusTextRect = plusTextObj.AddComponent<RectTransform>();
            plusTextRect.anchorMin = Vector2.zero;
            plusTextRect.anchorMax = Vector2.one;
            plusTextRect.sizeDelta = Vector2.zero;

            var plusText = plusTextObj.AddComponent<Text>();
            plusText.text = "+";
            plusText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            plusText.fontSize = 48;
            plusText.color = ColorTextPrimary;
            plusText.alignment = TextAnchor.MiddleCenter;

            var padding = itemObj.AddComponent<LayoutElement>();
            padding.preferredHeight = UnitHeight;
        }

        private void CreateSoundItem()
        {
            var itemObj = new GameObject("SoundItem");
            itemObj.transform.SetParent(_scrollContent.transform, false);

            var itemRect = itemObj.AddComponent<RectTransform>();
            itemRect.sizeDelta = new Vector2(0, UnitHeight);

            var itemImage = itemObj.AddComponent<Image>();
            if (_bottomBorderGradient != null) itemImage.sprite = _bottomBorderGradient;
            itemImage.type = Image.Type.Simple;
            itemImage.preserveAspect = false;
            itemImage.color = Color.white;

            var labelObj = new GameObject("Label");
            labelObj.transform.SetParent(itemObj.transform, false);

            var labelRect = labelObj.AddComponent<RectTransform>();
            labelRect.anchorMin = new Vector2(0, 0);
            labelRect.anchorMax = new Vector2(0.6f, 1);
            labelRect.sizeDelta = Vector2.zero;
            labelRect.anchoredPosition = new Vector2(15, 0);

            var labelText = labelObj.AddComponent<Text>();
            labelText.text = GetText("ui_sound");
            labelText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            labelText.fontSize = 38;
            labelText.color = ColorTextPrimary;
            labelText.alignment = TextAnchor.MiddleLeft;
            labelObj.AddComponent<Framework.FontScaler>();

            var toggleObj = new GameObject("Toggle");
            toggleObj.transform.SetParent(itemObj.transform, false);

            var toggleRect = toggleObj.AddComponent<RectTransform>();
            toggleRect.anchorMin = new Vector2(1, 0.5f);
            toggleRect.anchorMax = new Vector2(1, 0.5f);
            toggleRect.sizeDelta = new Vector2(50, 50);
            toggleRect.anchoredPosition = new Vector2(-30, 0);

            var bgObj = new GameObject("Background");
            bgObj.transform.SetParent(toggleObj.transform, false);
            var bgRect = bgObj.AddComponent<RectTransform>();
            bgRect.anchorMin = Vector2.zero;
            bgRect.anchorMax = Vector2.one;
            bgRect.sizeDelta = Vector2.zero;
            var bgImage = bgObj.AddComponent<Image>();
            if (_rectangleSolid != null) bgImage.sprite = _rectangleSolid;
            bgImage.type = Image.Type.Sliced;
            bgImage.color = ColorToggleBackground;

            var checkObj = new GameObject("Checkmark");
            checkObj.transform.SetParent(bgObj.transform, false);
            var checkRect = checkObj.AddComponent<RectTransform>();
            checkRect.anchorMin = new Vector2(0.5f, 0.5f);
            checkRect.anchorMax = new Vector2(0.5f, 0.5f);
            checkRect.sizeDelta = new Vector2(30, 30);
            var checkImage = checkObj.AddComponent<Image>();
            if (_checkmark != null) checkImage.sprite = _checkmark;
            checkImage.type = Image.Type.Simple;
            checkImage.preserveAspect = true;
            checkImage.color = ColorButtonEdit;

            var toggle = toggleObj.AddComponent<Toggle>();
            toggle.targetGraphic = bgImage;
            toggle.graphic = checkImage;
            toggle.isOn = DataManager.Instance.User.UISoundEnabled;
            toggle.onValueChanged.AddListener(OnSoundToggle);

            var padding = itemObj.AddComponent<LayoutElement>();
            padding.preferredHeight = UnitHeight;
        }

        #endregion

        #region Settings Callbacks

        private string GetLanguageName(DataManager.Languages language)
        {
            return GetText($"lang_{language}");
        }

        private void OnLanguageItemClick()
        {
            var allLanguages = Enum.GetValues(typeof(DataManager.Languages)).Cast<DataManager.Languages>().ToList();
            int currentIndex = allLanguages.IndexOf(DataManager.Instance.Language);
            int nextIndex = (currentIndex + 1) % allLanguages.Count;
            var newLanguage = allLanguages[nextIndex];

            DataManager.Instance.Language = newLanguage;
            Local.Instance.Save(DataManager.Instance.User);
            BuildUI();
        }

        private void OnSoundToggle(bool enabled)
        {
            DataManager.Instance.User.UISoundEnabled = enabled;
            Local.Instance.Save(DataManager.Instance.User);
        }

        private void OnFontSizeDecrease()
        {
            int[] fontSizes = { 25, 30, 35, 40 };
            int currentFontSize = DataManager.Instance.FontSize;
            int currentIndex = System.Array.IndexOf(fontSizes, currentFontSize);
            if (currentIndex < 0) currentIndex = 1;

            if (currentIndex > 0)
            {
                currentIndex--;
                DataManager.Instance.FontSize = fontSizes[currentIndex];
                Local.Instance.Save(DataManager.Instance.User);
                BuildUI();
            }
        }

        private void OnFontSizeIncrease()
        {
            int[] fontSizes = { 25, 30, 35, 40 };
            int currentFontSize = DataManager.Instance.FontSize;
            int currentIndex = System.Array.IndexOf(fontSizes, currentFontSize);
            if (currentIndex < 0) currentIndex = 1;

            if (currentIndex < fontSizes.Length - 1)
            {
                currentIndex++;
                DataManager.Instance.FontSize = fontSizes[currentIndex];
                Local.Instance.Save(DataManager.Instance.User);
                BuildUI();
            }
        }

        #endregion
    }
}
