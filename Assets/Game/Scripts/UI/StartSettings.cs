using Framework;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq;
using System;
using DG.Tweening;

namespace Game
{
    /// <summary>
    /// iOS/WeChat style grouped list settings panel
    /// Temporary simplified version with dynamic UI creation
    /// </summary>
    public class StartSettings : UI.Core
    {
        #region Constants
        private const float UnitHeight = 83f;
        private const float PanelHeightUnits = 12f;
        private const float AnimationDuration = 0.3f;
        #endregion

        #region Account Data
        private List<Account> _accounts;
        private int _selectedIndex;
        #endregion

        #region UI References
        private RectTransform _panelRect;
        private Image _maskImage;
        private GameObject _scrollContent;
        private List<GameObject> _accountItemObjects = new List<GameObject>();
        #endregion

        #region Lifecycle
        public override void OnCreate(params object[] args)
        {
            Utils.Debug.Log("StartSettings", "OnCreate - Simplified version with dynamic UI");
            
            // Cache references
            _panelRect = transform.Find("Panel")?.GetComponent<RectTransform>();
            _maskImage = GetComponent<Image>();
            
            // Find or create Content container
            var contentTransform = transform.Find("Panel/Content");
            if (contentTransform == null)
            {
                var contentObj = new GameObject("Content");
                contentObj.transform.SetParent(_panelRect, false);
                
                var contentRect = contentObj.AddComponent<RectTransform>();
                contentRect.anchorMin = new Vector2(0, 0);
                contentRect.anchorMax = new Vector2(1, 1);
                contentRect.sizeDelta = Vector2.zero;
                contentRect.anchoredPosition = Vector2.zero;
                
                // Add VerticalLayoutGroup for auto-layout
                var layoutGroup = contentObj.AddComponent<VerticalLayoutGroup>();
                layoutGroup.childAlignment = TextAnchor.UpperCenter;
                layoutGroup.childControlWidth = true;
                layoutGroup.childControlHeight = false;
                layoutGroup.childForceExpandWidth = true;
                layoutGroup.childForceExpandHeight = false;
                layoutGroup.spacing = 5;
                layoutGroup.padding = new RectOffset(20, 20, 40, 20);
                
                _scrollContent = contentObj;
            }
            else
            {
                _scrollContent = contentTransform.gameObject;
            }
            
            // Apply layout
            ApplyLayout();
            
            // Setup buttons
            SetupButtons();
            
            // Load data
            LoadAccountData();
            
            // Build UI structure
            BuildUI();
        }

        public override void OnEnter(params object[] args)
        {
            Utils.Debug.Log("StartSettings", "OnEnter - starting slide-up animation");
            
            if (_panelRect == null || _maskImage == null) return;
            
            float panelHeight = UnitHeight * PanelHeightUnits;
            
            // Initial state
            _panelRect.anchoredPosition = new Vector2(0, -panelHeight);
            _maskImage.color = new Color(0, 0, 0, 0);
            
            // Animate
            var sequence = DOTween.Sequence();
            sequence.Append(_maskImage.DOFade(0.7f, 0.25f));
            sequence.Join(_panelRect.DOAnchorPosY(0, AnimationDuration).SetEase(Ease.OutCubic));
            
            Utils.Debug.Log("StartSettings", "Animation sequence started");
        }

        public override void OnExit()
        {
            Utils.Debug.Log("StartSettings", "OnExit - closing panel");
            DOTween.KillAll();
        }
        #endregion

        #region Layout
        private void ApplyLayout()
        {
            if (_panelRect == null) return;
            
            float panelHeight = UnitHeight * PanelHeightUnits;
            
            // Set panel to full screen width, fixed height
            _panelRect.anchorMin = new Vector2(0, 0);
            _panelRect.anchorMax = new Vector2(1, 0);
            _panelRect.pivot = new Vector2(0.5f, 0);
            _panelRect.sizeDelta = new Vector2(0, panelHeight); // x=0 means stretch to parent width
            _panelRect.anchoredPosition = new Vector2(0, 0);
            
            Utils.Debug.Log("StartSettings", $"Panel layout applied: width=full screen, height={panelHeight}");
        }

        private void SetupButtons()
        {
            // Close button
            var closeButton = transform.Find("Panel/Header/Close")?.GetComponent<Button>();
            if (closeButton != null)
            {
                closeButton.onClick.AddListener(OnCloseClick);
            }
            
            // Mask click to close
            var maskButton = GetComponent<Button>();
            if (maskButton != null)
            {
                maskButton.onClick.AddListener(OnMaskClick);
            }
        }
        #endregion

        #region Data Loading
        private void LoadAccountData()
        {
            _accounts = Data.Instance.User.Accounts.ToList();
            _selectedIndex = Data.Instance.User.SelectedAccountIndex;
            
            Utils.Debug.Log("StartSettings", $"Loaded {_accounts.Count} accounts, selected: {_selectedIndex}");
        }
        #endregion

        #region UI Building - Dynamic Creation
        private void BuildUI()
        {
            if (_scrollContent == null)
            {
                Utils.Debug.LogError("StartSettings", "Content container not found");
                return;
            }
            
            // Clear existing content
            _scrollContent.ClearChildren();
            
            // Build sections
            BuildAccountSection();
            BuildSettingsSection();
            
            Utils.Debug.Log("StartSettings", "UI built successfully (dynamic version)");
        }

        private void BuildAccountSection()
        {
            // Section header
            CreateSectionHeader(Localization.Instance.Get("start_settings_accounts"));
            
            // Account items
            _accountItemObjects.Clear();
            for (int i = 0; i < _accounts.Count; i++)
            {
                var account = _accounts[i];
                var itemObj = CreateAccountItem(account, i == _selectedIndex, i);
                _accountItemObjects.Add(itemObj);
            }
            
            // Add account button
            CreateAddAccountButton();
            
            Utils.Debug.Log("StartSettings", $"Account section built with {_accounts.Count} items");
        }

        private void BuildSettingsSection()
        {
            // Section header
            CreateSectionHeader(Localization.Instance.Get("start_settings_general"));
            
            // Language item
            CreateLanguageItem();
            
            // Sound item
            CreateSoundItem();
            
            Utils.Debug.Log("StartSettings", "Settings section built");
        }

        private void CreateSectionHeader(string title)
        {
            var headerObj = new GameObject("SectionHeader");
            headerObj.transform.SetParent(_scrollContent.transform, false);
            
            var headerRect = headerObj.AddComponent<RectTransform>();
            headerRect.sizeDelta = new Vector2(0, 40);
            
            var headerText = headerObj.AddComponent<Text>();
            headerText.text = title;
            headerText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            headerText.fontSize = 24;
            headerText.color = new Color(0.557f, 0.557f, 0.576f, 1f);
            headerText.alignment = TextAnchor.MiddleLeft;
            headerObj.AddComponent<Framework.FontScaler>();
            
            var padding = headerObj.AddComponent<LayoutElement>();
            padding.preferredHeight = 40;
        }

        private GameObject CreateAccountItem(Account account, bool isSelected, int index)
        {
            var itemObj = new GameObject($"AccountItem_{index}");
            itemObj.transform.SetParent(_scrollContent.transform, false);
            
            var itemRect = itemObj.AddComponent<RectTransform>();
            itemRect.sizeDelta = new Vector2(0, UnitHeight);
            
            var itemImage = itemObj.AddComponent<Image>();
            itemImage.color = isSelected ? new Color(0.9f, 0.9f, 0.9f, 1f) : Color.white;
            
            var itemButton = itemObj.AddComponent<Button>();
            itemButton.targetGraphic = itemImage;
            itemButton.onClick.AddListener(() => OnAccountSwitch(account));
            
            var textObj = new GameObject("Text");
            textObj.transform.SetParent(itemObj.transform, false);
            
            var textRect = textObj.AddComponent<RectTransform>();
            textRect.anchorMin = new Vector2(0, 0);
            textRect.anchorMax = new Vector2(1, 1);
            textRect.sizeDelta = Vector2.zero;
            textRect.anchoredPosition = Vector2.zero;
            
            var text = textObj.AddComponent<Text>();
            text.text = account.Id + (string.IsNullOrEmpty(account.Note) ? "" : $" ({account.Note})");
            text.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            text.fontSize = 32;
            text.color = Color.black;
            text.alignment = TextAnchor.MiddleLeft;
            textObj.AddComponent<Framework.FontScaler>();
            
            var padding = itemObj.AddComponent<LayoutElement>();
            padding.preferredHeight = UnitHeight;
            
            return itemObj;
        }

        private void CreateAddAccountButton()
        {
            var buttonObj = new GameObject("AddAccountButton");
            buttonObj.transform.SetParent(_scrollContent.transform, false);
            
            var buttonRect = buttonObj.AddComponent<RectTransform>();
            buttonRect.sizeDelta = new Vector2(0, UnitHeight);
            
            var buttonImage = buttonObj.AddComponent<Image>();
            buttonImage.color = Color.white;
            
            var button = buttonObj.AddComponent<Button>();
            button.targetGraphic = buttonImage;
            button.onClick.AddListener(OnAddAccountClick);
            
            var textObj = new GameObject("Text");
            textObj.transform.SetParent(buttonObj.transform, false);
            
            var textRect = textObj.AddComponent<RectTransform>();
            textRect.anchorMin = new Vector2(0, 0);
            textRect.anchorMax = new Vector2(1, 1);
            textRect.sizeDelta = Vector2.zero;
            textRect.anchoredPosition = Vector2.zero;
            
            var text = textObj.AddComponent<Text>();
            text.text = Localization.Instance.Get("start_settings_add_account");
            text.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            text.fontSize = 32;
            text.color = new Color(0, 0.5f, 1, 1);
            text.alignment = TextAnchor.MiddleCenter;
            textObj.AddComponent<Framework.FontScaler>();
            
            var padding = buttonObj.AddComponent<LayoutElement>();
            padding.preferredHeight = UnitHeight;
        }

        private void CreateLanguageItem()
        {
            var itemObj = new GameObject("LanguageItem");
            itemObj.transform.SetParent(_scrollContent.transform, false);
            
            var itemRect = itemObj.AddComponent<RectTransform>();
            itemRect.sizeDelta = new Vector2(0, UnitHeight);
            
            var itemImage = itemObj.AddComponent<Image>();
            itemImage.color = Color.white;
            
            var itemButton = itemObj.AddComponent<Button>();
            itemButton.targetGraphic = itemImage;
            itemButton.onClick.AddListener(OnLanguageItemClick);
            
            var labelObj = new GameObject("Label");
            labelObj.transform.SetParent(itemObj.transform, false);
            
            var labelRect = labelObj.AddComponent<RectTransform>();
            labelRect.anchorMin = new Vector2(0, 0);
            labelRect.anchorMax = new Vector2(0.5f, 1);
            labelRect.sizeDelta = Vector2.zero;
            labelRect.anchoredPosition = new Vector2(15, 0);
            
            var labelText = labelObj.AddComponent<Text>();
            labelText.text = Localization.Instance.Get("start_settings_language");
            labelText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            labelText.fontSize = 32;
            labelText.color = Color.black;
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
            valueText.text = Data.Instance.Language.ToString();
            valueText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            valueText.fontSize = 28;
            valueText.color = new Color(0.557f, 0.557f, 0.576f, 1f);
            valueText.alignment = TextAnchor.MiddleRight;
            valueObj.AddComponent<Framework.FontScaler>();
            
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
            itemImage.color = Color.white;
            
            var labelObj = new GameObject("Label");
            labelObj.transform.SetParent(itemObj.transform, false);
            
            var labelRect = labelObj.AddComponent<RectTransform>();
            labelRect.anchorMin = new Vector2(0, 0);
            labelRect.anchorMax = new Vector2(0.6f, 1);
            labelRect.sizeDelta = Vector2.zero;
            labelRect.anchoredPosition = new Vector2(15, 0);
            
            var labelText = labelObj.AddComponent<Text>();
            labelText.text = Localization.Instance.Get("start_settings_ui_sound");
            labelText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            labelText.fontSize = 32;
            labelText.color = Color.black;
            labelText.alignment = TextAnchor.MiddleLeft;
            labelObj.AddComponent<Framework.FontScaler>();
            
            // Create Toggle with proper structure
            var toggleObj = new GameObject("Toggle");
            toggleObj.transform.SetParent(itemObj.transform, false);
            
            var toggleRect = toggleObj.AddComponent<RectTransform>();
            toggleRect.anchorMin = new Vector2(1, 0.5f);
            toggleRect.anchorMax = new Vector2(1, 0.5f);
            toggleRect.sizeDelta = new Vector2(60, 40);
            toggleRect.anchoredPosition = new Vector2(-30, 0);
            
            // Background
            var bgObj = new GameObject("Background");
            bgObj.transform.SetParent(toggleObj.transform, false);
            var bgRect = bgObj.AddComponent<RectTransform>();
            bgRect.anchorMin = Vector2.zero;
            bgRect.anchorMax = Vector2.one;
            bgRect.sizeDelta = Vector2.zero;
            var bgImage = bgObj.AddComponent<Image>();
            bgImage.color = new Color(0.8f, 0.8f, 0.8f, 1f);
            
            // Checkmark
            var checkObj = new GameObject("Checkmark");
            checkObj.transform.SetParent(bgObj.transform, false);
            var checkRect = checkObj.AddComponent<RectTransform>();
            checkRect.anchorMin = new Vector2(0.5f, 0.5f);
            checkRect.anchorMax = new Vector2(0.5f, 0.5f);
            checkRect.sizeDelta = new Vector2(30, 30);
            var checkImage = checkObj.AddComponent<Image>();
            checkImage.color = new Color(0, 0.5f, 1f, 1f);
            
            // Toggle component
            var toggle = toggleObj.AddComponent<Toggle>();
            toggle.targetGraphic = bgImage;
            toggle.graphic = checkImage;
            toggle.isOn = Data.Instance.User.UISoundEnabled;
            toggle.onValueChanged.AddListener(OnSoundToggle);
            
            var padding = itemObj.AddComponent<LayoutElement>();
            padding.preferredHeight = UnitHeight;
        }
        #endregion

        #region Account Callbacks
        private void OnAccountSwitch(Account account)
        {
            Utils.Debug.Log("StartSettings", $"Switching to account: {account.Id}");
            
            int newIndex = _accounts.FindIndex(a => a.Id == account.Id);
            if (newIndex < 0) return;
            
            _selectedIndex = newIndex;
            Data.Instance.User.SelectedAccountIndex = _selectedIndex;
            Local.Instance.Save(Data.Instance.User);
            
            // Close panel and reconnect
            Close();
            Net.Instance.Disconnect();
            Net.Instance.Connect(Data.Instance.SelectedServer.Ip, Data.Instance.SelectedServer.Port);
        }

        private void OnAddAccountClick()
        {
            Utils.Debug.Log("StartSettings", "Add account clicked - feature coming soon");
            // TODO: Implement add account dialog
        }
        #endregion

        #region Settings Callbacks
        private void OnLanguageItemClick()
        {
            Utils.Debug.Log("StartSettings", "Language item clicked");
            
            // Cycle through languages
            var allLanguages = Enum.GetValues(typeof(Data.Languages)).Cast<Data.Languages>().ToList();
            int currentIndex = allLanguages.IndexOf(Data.Instance.Language);
            int nextIndex = (currentIndex + 1) % allLanguages.Count;
            var newLanguage = allLanguages[nextIndex];
            
            // Update language
            Data.Instance.Language = newLanguage;
            Local.Instance.Save(Data.Instance.User);
            
            // Rebuild UI to reflect new language
            BuildUI();
        }

        private void OnSoundToggle(bool enabled)
        {
            Utils.Debug.Log("StartSettings", $"Sound toggled: {enabled}");
            
            Data.Instance.User.UISoundEnabled = enabled;
            Local.Instance.Save(Data.Instance.User);
        }
        #endregion

        #region Close
        private void OnCloseClick()
        {
            Utils.Debug.Log("StartSettings", "Close button clicked");
            Close();
        }

        private void OnMaskClick()
        {
            Utils.Debug.Log("StartSettings", "Mask clicked");
            Close();
        }

        private void Close()
        {
            if (_panelRect == null || _maskImage == null)
            {
                UI.Instance.Close(Config.UI.StartSettings);
                return;
            }
            
            float panelHeight = UnitHeight * PanelHeightUnits;
            
            var sequence = DOTween.Sequence();
            sequence.Append(_maskImage.DOFade(0, 0.2f));
            sequence.Join(_panelRect.DOAnchorPosY(-panelHeight, 0.25f).SetEase(Ease.InCubic));
            sequence.OnComplete(() => {
                UI.Instance.Close(Config.UI.StartSettings);
            });
        }
        #endregion
    }
}
