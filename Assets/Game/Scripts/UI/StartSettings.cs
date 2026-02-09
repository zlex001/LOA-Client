using Framework;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq;
using System;
using DG.Tweening;

namespace Game
{
    public class StartSettings : UI.Core
    {
        #region Constants
        private const float UnitHeight = 83f;
        private const float PanelHeightUnits = 12f;
        private const float AnimationDuration = 0.3f;
        #endregion

        #region Tab System
        private enum TabType { Account, Settings }
        private TabType _currentTab = TabType.Account;
        #endregion

        #region Account Data
        private List<Account> _accounts;
        private int _selectedIndex;
        #endregion

        #region UI References
        private RectTransform _panelRect;
        private Image _maskImage;
        private GameObject _accountContent;
        private GameObject _settingsContent;
        private GameObject _accountTabButton;
        private GameObject _settingsTabButton;
        private GameObject _addAccountButton;
        #endregion

        #region Lifecycle
        public override void OnCreate(params object[] args)
        {
            
            Utils.Debug.Log("StartSettings", "OnCreate called");
            
            // Cache references
            _panelRect = transform.Find("Panel")?.GetComponent<RectTransform>();
            _maskImage = GetComponent<Image>();
            
            
            // Apply layout first
            ApplyLayout();
            
            // Build UI structure
            BuildTabBar();
            
            BuildAccountTab();
            
            BuildSettingsTab();
            
            // Setup close button
            var closeButton = transform.Find("Panel/Header/Close")?.GetComponent<Button>();
            if (closeButton != null)
            {
                closeButton.onClick.AddListener(OnCloseClick);
            }
            
            // Setup mask click to close
            var maskButton = GetComponent<Button>();
            if (maskButton != null)
            {
                maskButton.onClick.AddListener(OnMaskClick);
            }
            
            // Load account data
            _accounts = Data.Instance.User.Accounts.ToList();
            _selectedIndex = Data.Instance.User.SelectedAccountIndex;
            
            // Build account list
            BuildAccountList();
            
            // Show default tab
            UpdateContent();
            
            Utils.Debug.Log("StartSettings", $"Loaded {_accounts.Count} accounts, selected index: {_selectedIndex}");
        }

        public override void OnEnter(params object[] args)
        {
            Utils.Debug.Log("StartSettings", "OnEnter called - starting slide-up animation");
            
            if (_panelRect == null || _maskImage == null) return;
            
            float panelHeight = UnitHeight * PanelHeightUnits;
            
            // Initial state: panel below screen, mask transparent
            _panelRect.anchoredPosition = new Vector2(0, -panelHeight);
            _maskImage.color = new Color(0, 0, 0, 0);
            
            // Animate: slide up from bottom
            var sequence = DOTween.Sequence();
            sequence.Append(_maskImage.DOFade(0.7f, 0.25f));
            sequence.Join(_panelRect.DOAnchorPosY(0, AnimationDuration).SetEase(Ease.OutCubic));
            
            Utils.Debug.Log("StartSettings", "Slide-up animation started");
        }

        public override void OnClose()
        {
            Utils.Debug.Log("StartSettings", "OnClose called - starting slide-down animation");
            
            if (_panelRect == null || _maskImage == null)
            {
                base.OnClose();
                return;
            }
            
            float panelHeight = UnitHeight * PanelHeightUnits;
            
            // Animate: slide down
            var sequence = DOTween.Sequence();
            sequence.Append(_panelRect.DOAnchorPosY(-panelHeight, 0.25f).SetEase(Ease.InCubic));
            sequence.Join(_maskImage.DOFade(0f, 0.25f));
            sequence.OnComplete(() => base.OnClose());
            
            Utils.Debug.Log("StartSettings", "Slide-down animation started");
        }

        public override void OnScreenAdaptationChanged()
        {
            ApplyLayout();
        }
        #endregion

        #region Layout
        private void ApplyLayout()
        {
            float screenWidth = GetComponent<RectTransform>().rect.width;
            float screenHeight = GetComponent<RectTransform>().rect.height;
            float panelHeight = UnitHeight * PanelHeightUnits;
            
            // Bottom drawer layout: anchor to bottom center
            if (_panelRect != null)
            {
                _panelRect.anchorMin = new Vector2(0.5f, 0f);  // Bottom center
                _panelRect.anchorMax = new Vector2(0.5f, 0f);
                _panelRect.pivot = new Vector2(0.5f, 0f);       // Pivot at bottom
                _panelRect.sizeDelta = new Vector2(screenWidth, panelHeight);
                // Position will be set by animation
            }
            
            Utils.Debug.Log("StartSettings", $"Bottom drawer layout applied: size=({screenWidth}, {panelHeight})");
        }
        #endregion

        #region Tab System
        private void BuildTabBar()
        {
            Utils.Debug.Log("StartSettings", "Building tab bar");
            
            // Find or create TabBar
            var tabBar = transform.Find("Panel/TabBar");
            if (tabBar == null)
            {
                var tabBarObj = new GameObject("TabBar");
                tabBarObj.transform.SetParent(transform.Find("Panel"), false);
                tabBar = tabBarObj.transform;
                
                var tabBarRect = tabBarObj.AddComponent<RectTransform>();
                float screenWidth = GetComponent<RectTransform>().rect.width;
                tabBarRect.anchorMin = new Vector2(0, 1);
                tabBarRect.anchorMax = new Vector2(1, 1);
                tabBarRect.pivot = new Vector2(0.5f, 1);
                tabBarRect.sizeDelta = new Vector2(0, UnitHeight);
                tabBarRect.anchoredPosition = new Vector2(0, -UnitHeight); // Below header
                
                // Background
                var tabBarBg = tabBarObj.AddComponent<Image>();
                tabBarBg.color = new Color(0.15f, 0.15f, 0.15f, 1f);
            }
            
            // Create Account Tab Button
            _accountTabButton = CreateTabButton(tabBar, "AccountTab", Localization.Instance.Get("start_settings_tab_account"), 0, TabType.Account);
            
            // Create Settings Tab Button
            _settingsTabButton = CreateTabButton(tabBar, "SettingsTab", Localization.Instance.Get("start_settings_tab_settings"), 1, TabType.Settings);
            
            UpdateTabVisuals();
        }

        private GameObject CreateTabButton(Transform parent, string name, string label, int index, TabType tabType)
        {
            var buttonObj = new GameObject(name);
            buttonObj.transform.SetParent(parent, false);
            
            var rect = buttonObj.AddComponent<RectTransform>();
            float halfWidth = 0.5f;
            rect.anchorMin = new Vector2(index * halfWidth, 0);
            rect.anchorMax = new Vector2((index + 1) * halfWidth, 1);
            rect.sizeDelta = Vector2.zero;
            rect.anchoredPosition = Vector2.zero;
            
            // Background
            var bg = buttonObj.AddComponent<Image>();
            bg.color = new Color(0.2f, 0.2f, 0.2f, 0.5f);
            
            // Text (as child GameObject - correct pattern)
            var textObj = new GameObject("Text");
            textObj.transform.SetParent(buttonObj.transform, false);
            var textRect = textObj.AddComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.sizeDelta = Vector2.zero;
            textRect.anchoredPosition = Vector2.zero;
            
            var text = textObj.AddComponent<Text>();
            text.text = label;
            text.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            text.fontSize = 28;
            text.color = new Color(0.7f, 0.7f, 0.7f, 1f);
            text.alignment = TextAnchor.MiddleCenter;
            
            // Button
            var button = buttonObj.AddComponent<Button>();
            button.onClick.AddListener(() => OnTabClick(tabType));
            
            return buttonObj;
        }

        private void OnTabClick(TabType tab)
        {
            if (_currentTab == tab) return;
            
            Utils.Debug.Log("StartSettings", $"Tab clicked: {tab}");
            _currentTab = tab;
            UpdateTabVisuals();
            UpdateContent();
        }

        private void UpdateTabVisuals()
        {
            // Update Account Tab
            if (_accountTabButton != null)
            {
                var text = _accountTabButton.transform.Find("Text")?.GetComponent<Text>();
                if (text != null)
                {
                    text.color = (_currentTab == TabType.Account) ? Color.white : new Color(0.7f, 0.7f, 0.7f, 1f);
                }
                var bg = _accountTabButton.GetComponent<Image>();
                if (bg != null)
                {
                    bg.color = (_currentTab == TabType.Account) ? new Color(0.25f, 0.25f, 0.25f, 1f) : new Color(0.2f, 0.2f, 0.2f, 0.5f);
                }
            }
            
            // Update Settings Tab
            if (_settingsTabButton != null)
            {
                var text = _settingsTabButton.transform.Find("Text")?.GetComponent<Text>();
                if (text != null)
                {
                    text.color = (_currentTab == TabType.Settings) ? Color.white : new Color(0.7f, 0.7f, 0.7f, 1f);
                }
                var bg = _settingsTabButton.GetComponent<Image>();
                if (bg != null)
                {
                    bg.color = (_currentTab == TabType.Settings) ? new Color(0.25f, 0.25f, 0.25f, 1f) : new Color(0.2f, 0.2f, 0.2f, 0.5f);
                }
            }
        }

        private void UpdateContent()
        {
            if (_accountContent != null)
            {
                _accountContent.SetActive(_currentTab == TabType.Account);
                if (_currentTab == TabType.Account)
                {
                    BuildAccountList();
                }
            }
            
            if (_settingsContent != null)
            {
                _settingsContent.SetActive(_currentTab == TabType.Settings);
            }
            
            if (_addAccountButton != null)
            {
                _addAccountButton.SetActive(_currentTab == TabType.Account);
            }
            
            Utils.Debug.Log("StartSettings", $"Content updated: showing {_currentTab} tab");
        }
        #endregion

        #region Account Tab
        private void BuildAccountTab()
        {
            Utils.Debug.Log("StartSettings", "Building account tab");
            
            var panel = transform.Find("Panel");
            if (panel == null) return;
            
            float screenWidth = GetComponent<RectTransform>().rect.width;
            
            // Create AccountContent container
            var accountContentObj = new GameObject("AccountContent");
            accountContentObj.transform.SetParent(panel, false);
            _accountContent = accountContentObj;
            
            var contentRect = accountContentObj.AddComponent<RectTransform>();
            contentRect.anchorMin = Vector2.zero;
            contentRect.anchorMax = Vector2.one;
            contentRect.sizeDelta = Vector2.zero;
            contentRect.anchoredPosition = Vector2.zero;
            
            // Create ScrollView
            var scrollViewObj = new GameObject("ScrollView");
            scrollViewObj.transform.SetParent(accountContentObj.transform, false);
            
            var scrollRect = scrollViewObj.AddComponent<RectTransform>();
            scrollRect.anchorMin = new Vector2(0, 0);
            scrollRect.anchorMax = new Vector2(1, 1);
            scrollRect.sizeDelta = new Vector2(0, -UnitHeight * 3); // Reserve space for header, tab, and button
            scrollRect.anchoredPosition = new Vector2(0, -UnitHeight * 2); // Below header and tab
            
            // Add ScrollRect component
            var scrollView = scrollViewObj.AddComponent<ScrollRect>();
            scrollView.horizontal = false;
            scrollView.vertical = true;
            
            // Viewport
            var viewportObj = new GameObject("Viewport");
            viewportObj.transform.SetParent(scrollViewObj.transform, false);
            
            var viewportRect = viewportObj.AddComponent<RectTransform>();
            viewportRect.anchorMin = Vector2.zero;
            viewportRect.anchorMax = Vector2.one;
            viewportRect.sizeDelta = Vector2.zero;
            viewportRect.anchoredPosition = Vector2.zero;
            
            var viewportMask = viewportObj.AddComponent<Mask>();
            viewportMask.showMaskGraphic = false;
            var viewportImage = viewportObj.AddComponent<Image>();
            viewportImage.color = new Color(0.1f, 0.1f, 0.1f, 0.5f);
            
            scrollView.viewport = viewportRect;
            
            // Content
            var contentObj = new GameObject("Content");
            contentObj.transform.SetParent(viewportObj.transform, false);
            
            var contentRectTransform = contentObj.AddComponent<RectTransform>();
            contentRectTransform.anchorMin = new Vector2(0, 1);
            contentRectTransform.anchorMax = new Vector2(1, 1);
            contentRectTransform.pivot = new Vector2(0.5f, 1);
            contentRectTransform.sizeDelta = new Vector2(0, 0); // Will be set dynamically
            contentRectTransform.anchoredPosition = Vector2.zero;
            
            scrollView.content = contentRectTransform;
            
            // Add Account Button (bottom)
            _addAccountButton = CreateAddAccountButton(accountContentObj);
            
            Utils.Debug.Log("StartSettings", "Account tab structure created");
        }

        private GameObject CreateAddAccountButton(GameObject parent)
        {
            var buttonObj = new GameObject("AddAccountButton");
            buttonObj.transform.SetParent(parent.transform, false);
            
            var rect = buttonObj.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(0, 0);
            rect.anchorMax = new Vector2(1, 0);
            rect.pivot = new Vector2(0.5f, 0);
            rect.sizeDelta = new Vector2(-40, UnitHeight * 0.8f);
            rect.anchoredPosition = new Vector2(0, 10);
            
            // Background
            var bg = buttonObj.AddComponent<Image>();
            bg.color = new Color(0.2f, 0.5f, 0.8f, 0.8f);
            
            // Text
            var textObj = new GameObject("Text");
            textObj.transform.SetParent(buttonObj.transform, false);
            var textRect = textObj.AddComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.sizeDelta = Vector2.zero;
            textRect.anchoredPosition = Vector2.zero;
            
            var text = textObj.AddComponent<Text>();
            text.text = Localization.Instance.Get("start_settings_add_account");
            text.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            text.fontSize = 28;
            text.color = Color.white;
            text.alignment = TextAnchor.MiddleCenter;
            
            // Button
            var button = buttonObj.AddComponent<Button>();
            button.onClick.AddListener(OnAddAccountClick);
            
            return buttonObj;
        }

        private void BuildAccountList()
        {
            Utils.Debug.Log("StartSettings", $"BuildAccountList called, account count: {_accounts.Count}");
        }

        private void CreateAccountItem(Transform parent, Account account, int index)
        {
            // Main container
            var itemObj = new GameObject($"Account_{index}");
            itemObj.transform.SetParent(parent, false);
            
            var rect = itemObj.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(0, 1);
            rect.anchorMax = new Vector2(1, 1);
            rect.pivot = new Vector2(0.5f, 1);
            rect.sizeDelta = new Vector2(0, UnitHeight);
            rect.anchoredPosition = new Vector2(0, -index * UnitHeight);
            
            // Background
            var bg = itemObj.AddComponent<Image>();
            bg.color = (index == _selectedIndex) ?
                new Color(0.3f, 0.6f, 1f, 0.3f) :  // Selected: light blue
                new Color(0.2f, 0.2f, 0.2f, 0.5f); // Normal: dark gray
            
            // Check icon (left side)
            if (index == _selectedIndex)
            {
                CreateCheckIcon(itemObj, 40, UnitHeight / 2);
            }
            
            // Account text (center)
            CreateAccountText(itemObj, account);
            
            // Edit button (right side)
            CreateEditButton(itemObj, index);
            
            // Delete button (right side, next to edit)
            CreateDeleteButton(itemObj, index);
            
            // Click to switch account
            var button = itemObj.AddComponent<Button>();
            int capturedIndex = index;
            button.onClick.AddListener(() => OnAccountClick(capturedIndex));
            
            Utils.Debug.Log("StartSettings", $"Created account item: {account.Id} (index {index})");
        }

        private void CreateCheckIcon(GameObject parent, float xOffset, float yOffset)
        {
            var iconObj = new GameObject("CheckIcon");
            iconObj.transform.SetParent(parent.transform, false);
            
            var rect = iconObj.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(0, 0.5f);
            rect.anchorMax = new Vector2(0, 0.5f);
            rect.pivot = new Vector2(0, 0.5f);
            rect.sizeDelta = new Vector2(30, 30);
            rect.anchoredPosition = new Vector2(xOffset, 0);
            
            var text = iconObj.AddComponent<Text>();
            text.text = "✓";
            text.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            text.fontSize = 32;
            text.color = new Color(0.3f, 0.8f, 1f, 1f);
            text.alignment = TextAnchor.MiddleCenter;
        }

        private void CreateAccountText(GameObject parent, Account account)
        {
            var textObj = new GameObject("Text");
            textObj.transform.SetParent(parent.transform, false);
            
            var textRect = textObj.AddComponent<RectTransform>();
            textRect.anchorMin = new Vector2(0.15f, 0);
            textRect.anchorMax = new Vector2(0.7f, 1);
            textRect.sizeDelta = Vector2.zero;
            textRect.anchoredPosition = Vector2.zero;
            
            var text = textObj.AddComponent<Text>();
            text.text = $"{account.Id}\n{account.Note}";
            text.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            text.fontSize = 22;
            text.color = Color.white;
            text.alignment = TextAnchor.MiddleLeft;
        }

        private void CreateEditButton(GameObject parent, int index)
        {
            var buttonObj = new GameObject("EditButton");
            buttonObj.transform.SetParent(parent.transform, false);
            
            var rect = buttonObj.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(1, 0.5f);
            rect.anchorMax = new Vector2(1, 0.5f);
            rect.pivot = new Vector2(1, 0.5f);
            rect.sizeDelta = new Vector2(50, 50);
            rect.anchoredPosition = new Vector2(-60, 0);
            
            var bg = buttonObj.AddComponent<Image>();
            bg.color = new Color(0.4f, 0.4f, 0.4f, 0.5f);
            
            // Text (as child GameObject - correct pattern)
            var textObj = new GameObject("Text");
            textObj.transform.SetParent(buttonObj.transform, false);
            var textRect = textObj.AddComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.sizeDelta = Vector2.zero;
            textRect.anchoredPosition = Vector2.zero;
            
            var text = textObj.AddComponent<Text>();
            text.text = "✏";
            text.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            text.fontSize = 28;
            text.color = Color.white;
            text.alignment = TextAnchor.MiddleCenter;
            
            var button = buttonObj.AddComponent<Button>();
            int capturedIndex = index;
            button.onClick.AddListener(() => OnEditAccountNote(capturedIndex));
        }

        private void CreateDeleteButton(GameObject parent, int index)
        {
            var buttonObj = new GameObject("DeleteButton");
            buttonObj.transform.SetParent(parent.transform, false);
            
            var rect = buttonObj.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(1, 0.5f);
            rect.anchorMax = new Vector2(1, 0.5f);
            rect.pivot = new Vector2(1, 0.5f);
            rect.sizeDelta = new Vector2(50, 50);
            rect.anchoredPosition = new Vector2(-5, 0);
            
            var bg = buttonObj.AddComponent<Image>();
            bg.color = new Color(0.6f, 0.2f, 0.2f, 0.5f);
            
            // Text (as child GameObject - correct pattern)
            var textObj = new GameObject("Text");
            textObj.transform.SetParent(buttonObj.transform, false);
            var textRect = textObj.AddComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.sizeDelta = Vector2.zero;
            textRect.anchoredPosition = Vector2.zero;
            
            var text = textObj.AddComponent<Text>();
            text.text = "🗑";
            text.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            text.fontSize = 28;
            text.color = Color.white;
            text.alignment = TextAnchor.MiddleCenter;
            
            var button = buttonObj.AddComponent<Button>();
            int capturedIndex = index;
            button.onClick.AddListener(() => OnDeleteAccount(capturedIndex));
        }

        private void RefreshAccountList()
        {
            _accounts = Data.Instance.User.Accounts.ToList();
            _selectedIndex = Data.Instance.User.SelectedAccountIndex;
            BuildAccountList();
        }

        private void OnAccountClick(int index)
        {
            if (index == _selectedIndex)
            {
                Utils.Debug.Log("StartSettings", $"Account {index} already selected, ignoring");
                return;
            }
            
            Utils.Debug.Log("StartSettings", $"Account {index} clicked, switching from {_selectedIndex}");
            
            // Update selected index
            Data.Instance.User.SelectedAccountIndex = index;
            Local.Instance.Save(Data.Instance.User);
            
            // Show connecting UI
            UI.Instance.Open(Config.UI.Dark, Localization.Instance.Get("connecting"));
            
            // Disconnect current connection if any
            if (Data.Instance.Online)
            {
                Utils.Debug.Log("StartSettings", "Disconnecting current connection");
                Net.Instance.Disconnect();
            }
            
            // Set new login account (this triggers Net to send Login protocol)
            Utils.Debug.Log("StartSettings", $"Setting LoginAccount to: {_accounts[index].Id}");
            Data.Instance.LoginAccount = _accounts[index];
            
            // Close settings panel
            Close();
        }

        private void OnEditAccountNote(int index)
        {
            Utils.Debug.Log("StartSettings", $"Edit account note clicked for index {index}");
            var account = _accounts[index];
            ShowInputDialog(Localization.Instance.Get("start_settings_edit_note"), account.Note, (newNote) =>
            {
                account.Note = newNote;
                Data.Instance.User.Accounts[index] = account;
                Local.Instance.Save(Data.Instance.User);
                RefreshAccountList();
            });
        }

        private void OnDeleteAccount(int index)
        {
            if (index == _selectedIndex)
            {
                ShowErrorDialog(Localization.Instance.Get("start_settings_cannot_delete_current"));
                return;
            }
            
            var account = _accounts[index];
            ShowConfirmDialog(
                Localization.Instance.Get("start_settings_delete_confirm_title"),
                Localization.Instance.Get("start_settings_delete_confirm_message", account.Id, account.Note),
                () =>
                {
                    Data.Instance.User.Accounts.RemoveAt(index);
                    // Adjust selected index if necessary
                    if (Data.Instance.User.SelectedAccountIndex > index)
                    {
                        Data.Instance.User.SelectedAccountIndex--;
                    }
                    Local.Instance.Save(Data.Instance.User);
                    RefreshAccountList();
                }
            );
        }

        private void OnAddAccountClick()
        {
            Utils.Debug.Log("StartSettings", "Add account clicked");
            ShowAddAccountDialog((id, password, note) =>
            {
                var newAccount = new Account
                {
                    Id = id,
                    Password = password,
                    Note = note
                };
                Data.Instance.User.Accounts.Add(newAccount);
                Local.Instance.Save(Data.Instance.User);
                RefreshAccountList();
            });
        }
        #endregion

        #region Settings Tab
        private void BuildSettingsTab()
        {
            Utils.Debug.Log("StartSettings", "Building settings tab");
            
            var panel = transform.Find("Panel");
            if (panel == null) return;
            
            // Create SettingsContent container
            var settingsContentObj = new GameObject("SettingsContent");
            settingsContentObj.transform.SetParent(panel, false);
            _settingsContent = settingsContentObj;
            
            var contentRect = settingsContentObj.AddComponent<RectTransform>();
            contentRect.anchorMin = Vector2.zero;
            contentRect.anchorMax = Vector2.one;
            contentRect.sizeDelta = Vector2.zero;
            contentRect.anchoredPosition = Vector2.zero;
            
            // Create settings items
            float yOffset = -20;
            CreateLanguageSetting(settingsContentObj, ref yOffset);
            CreateSoundSetting(settingsContentObj, ref yOffset);
            
            Utils.Debug.Log("StartSettings", "Settings tab created");
        }

        private void CreateLanguageSetting(GameObject parent, ref float yOffset)
        {
            
            var settingObj = new GameObject("LanguageSetting");
            settingObj.transform.SetParent(parent.transform, false);
            
            var rect = settingObj.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(0, 1);
            rect.anchorMax = new Vector2(1, 1);
            rect.pivot = new Vector2(0.5f, 1);
            rect.sizeDelta = new Vector2(-40, UnitHeight * 1.5f);
            rect.anchoredPosition = new Vector2(0, yOffset);
            
            // Background
            var bg = settingObj.AddComponent<Image>();
            bg.color = new Color(0.2f, 0.2f, 0.2f, 0.5f);
            
            
            // Title
            var titleObj = new GameObject("Title");
            titleObj.transform.SetParent(settingObj.transform, false);
            var titleRect = titleObj.AddComponent<RectTransform>();
            titleRect.anchorMin = new Vector2(0, 0.6f);
            titleRect.anchorMax = new Vector2(1, 1);
            titleRect.sizeDelta = Vector2.zero;
            titleRect.anchoredPosition = Vector2.zero;
            
            
            var titleText = titleObj.AddComponent<Text>();
            
            
            titleText.text = Localization.Instance.Get("start_settings_language");
            
            
            titleText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            
            
            titleText.fontSize = 24;
            
            titleText.color = new Color(0.8f, 0.8f, 0.8f, 1f);
            
            titleText.alignment = TextAnchor.MiddleCenter;
            
            
            // Dropdown
            var dropdownObj = new GameObject("Dropdown");
            dropdownObj.transform.SetParent(settingObj.transform, false);
            var dropdownRect = dropdownObj.AddComponent<RectTransform>();
            dropdownRect.anchorMin = new Vector2(0.2f, 0.1f);
            dropdownRect.anchorMax = new Vector2(0.8f, 0.5f);
            dropdownRect.sizeDelta = Vector2.zero;
            dropdownRect.anchoredPosition = Vector2.zero;
            
            var dropdownBg = dropdownObj.AddComponent<Image>();
            dropdownBg.color = new Color(0.15f, 0.15f, 0.15f, 0.8f);
            
            var dropdown = dropdownObj.AddComponent<Dropdown>();
            
            // Label (displays current selection)
            var labelObj = new GameObject("Label");
            labelObj.transform.SetParent(dropdownObj.transform, false);
            var labelRect = labelObj.AddComponent<RectTransform>();
            labelRect.anchorMin = new Vector2(0.1f, 0);
            labelRect.anchorMax = new Vector2(0.9f, 1);
            labelRect.sizeDelta = Vector2.zero;
            labelRect.anchoredPosition = Vector2.zero;
            
            var labelText = labelObj.AddComponent<Text>();
            labelText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            labelText.fontSize = 24;
            labelText.color = Color.white;
            labelText.alignment = TextAnchor.MiddleLeft;
            
            dropdown.captionText = labelText;
            
            // Populate dropdown options from Data.Languages enum
            dropdown.ClearOptions();
            var languageOptions = new List<Dropdown.OptionData>();
            var languageNames = System.Enum.GetNames(typeof(Data.Languages));
            foreach (var langName in languageNames)
            {
                languageOptions.Add(new Dropdown.OptionData(langName));
            }
            dropdown.AddOptions(languageOptions);
            
            // Set current language as selected
            string currentLanguage = Data.Instance.User.Language;
            int selectedIndex = System.Array.IndexOf(languageNames, currentLanguage);
            if (selectedIndex >= 0)
            {
                dropdown.value = selectedIndex;
            }
            
            // Bind event: when language changes
            dropdown.onValueChanged.AddListener((int index) =>
            {
                string newLanguage = languageNames[index];
                Data.Instance.User.Language = newLanguage;
                Localization.Instance.Init(newLanguage);
                PlayerPrefs.SetString("LANGUAGE", newLanguage);
                PlayerPrefs.Save();
                Local.Instance.Save(Data.Instance.User);
                
                Utils.Debug.Log("StartSettings", $"Language changed to: {newLanguage}");
                
                // Refresh UI text (close and reopen panel)
                Close();
                UI.Instance.Open(Config.UI.StartSettings);
            });
            
            yOffset -= UnitHeight * 1.7f;
            
        }

        private void CreateSoundSetting(GameObject parent, ref float yOffset)
        {
            var settingObj = new GameObject("SoundSetting");
            settingObj.transform.SetParent(parent.transform, false);
            
            var rect = settingObj.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(0, 1);
            rect.anchorMax = new Vector2(1, 1);
            rect.pivot = new Vector2(0.5f, 1);
            rect.sizeDelta = new Vector2(-40, UnitHeight * 1.5f);
            rect.anchoredPosition = new Vector2(0, yOffset);
            
            // Background
            var bg = settingObj.AddComponent<Image>();
            bg.color = new Color(0.2f, 0.2f, 0.2f, 0.5f);
            
            // Title
            var titleObj = new GameObject("Title");
            titleObj.transform.SetParent(settingObj.transform, false);
            var titleRect = titleObj.AddComponent<RectTransform>();
            titleRect.anchorMin = new Vector2(0, 0.6f);
            titleRect.anchorMax = new Vector2(1, 1);
            titleRect.sizeDelta = Vector2.zero;
            titleRect.anchoredPosition = Vector2.zero;
            
            var titleText = titleObj.AddComponent<Text>();
            titleText.text = Localization.Instance.Get("start_settings_ui_sound");
            titleText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            titleText.fontSize = 24;
            titleText.color = new Color(0.8f, 0.8f, 0.8f, 1f);
            titleText.alignment = TextAnchor.MiddleCenter;
            
            // Toggle
            var toggleObj = new GameObject("Toggle");
            toggleObj.transform.SetParent(settingObj.transform, false);
            var toggleRect = toggleObj.AddComponent<RectTransform>();
            toggleRect.anchorMin = new Vector2(0.3f, 0.1f);
            toggleRect.anchorMax = new Vector2(0.7f, 0.5f);
            toggleRect.sizeDelta = Vector2.zero;
            toggleRect.anchoredPosition = Vector2.zero;
            
            var toggle = toggleObj.AddComponent<Toggle>();
            
            // Background
            var bgObj = new GameObject("Background");
            bgObj.transform.SetParent(toggleObj.transform, false);
            var bgRect = bgObj.AddComponent<RectTransform>();
            bgRect.anchorMin = new Vector2(0, 0.5f);
            bgRect.anchorMax = new Vector2(0, 0.5f);
            bgRect.pivot = new Vector2(0, 0.5f);
            bgRect.sizeDelta = new Vector2(50, 50);
            bgRect.anchoredPosition = new Vector2(10, 0);
            
            var bgImage = bgObj.AddComponent<Image>();
            bgImage.color = new Color(0.2f, 0.2f, 0.2f, 0.8f);
            
            // Checkmark
            var checkmarkObj = new GameObject("Checkmark");
            checkmarkObj.transform.SetParent(bgObj.transform, false);
            var checkmarkRect = checkmarkObj.AddComponent<RectTransform>();
            checkmarkRect.anchorMin = Vector2.zero;
            checkmarkRect.anchorMax = Vector2.one;
            checkmarkRect.sizeDelta = new Vector2(-10, -10);
            checkmarkRect.anchoredPosition = Vector2.zero;
            
            var checkmarkImage = checkmarkObj.AddComponent<Image>();
            checkmarkImage.color = new Color(0.2f, 0.8f, 0.2f, 1f);
            
            toggle.graphic = checkmarkImage;
            toggle.targetGraphic = bgImage;
            
            // Label
            var labelObj = new GameObject("Label");
            labelObj.transform.SetParent(toggleObj.transform, false);
            var labelRect = labelObj.AddComponent<RectTransform>();
            labelRect.anchorMin = new Vector2(0, 0);
            labelRect.anchorMax = new Vector2(1, 1);
            labelRect.sizeDelta = Vector2.zero;
            labelRect.anchoredPosition = new Vector2(30, 0);
            
            var labelText = labelObj.AddComponent<Text>();
            labelText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            labelText.fontSize = 24;
            labelText.color = Color.white;
            labelText.alignment = TextAnchor.MiddleLeft;
            
            // Set initial state
            bool currentSoundEnabled = Data.Instance.User.UISoundEnabled;
            toggle.isOn = currentSoundEnabled;
            labelText.text = currentSoundEnabled ? 
                Localization.Instance.Get("start_settings_sound_on") : 
                Localization.Instance.Get("start_settings_sound_off");
            
            // Bind event: when toggle changes
            toggle.onValueChanged.AddListener((bool isOn) =>
            {
                Data.Instance.User.UISoundEnabled = isOn;
                Local.Instance.Save(Data.Instance.User);
                labelText.text = isOn ? 
                    Localization.Instance.Get("start_settings_sound_on") : 
                    Localization.Instance.Get("start_settings_sound_off");
                
                Utils.Debug.Log("StartSettings", $"UI Sound toggled: {isOn}");
            });
            
            yOffset -= UnitHeight * 1.7f;
        }
        #endregion

        #region Dialog System
        private void ShowConfirmDialog(string title, string message, Action onConfirm)
        {
            Utils.Debug.Log("StartSettings", $"Showing confirm dialog: {title}");
            
            // Create dialog overlay
            var dialogObj = new GameObject("ConfirmDialog");
            dialogObj.transform.SetParent(transform, false);
            
            var dialogRect = dialogObj.AddComponent<RectTransform>();
            dialogRect.anchorMin = Vector2.zero;
            dialogRect.anchorMax = Vector2.one;
            dialogRect.sizeDelta = Vector2.zero;
            dialogRect.anchoredPosition = Vector2.zero;
            
            // Overlay background
            var overlayBg = dialogObj.AddComponent<Image>();
            overlayBg.color = new Color(0, 0, 0, 0.8f);
            
            // Dialog panel
            var panelObj = new GameObject("Panel");
            panelObj.transform.SetParent(dialogObj.transform, false);
            
            var panelRect = panelObj.AddComponent<RectTransform>();
            panelRect.anchorMin = new Vector2(0.5f, 0.5f);
            panelRect.anchorMax = new Vector2(0.5f, 0.5f);
            panelRect.pivot = new Vector2(0.5f, 0.5f);
            panelRect.sizeDelta = new Vector2(GetComponent<RectTransform>().rect.width * 0.7f, UnitHeight * 4);
            panelRect.anchoredPosition = Vector2.zero;
            
            var panelBg = panelObj.AddComponent<Image>();
            panelBg.color = new Color(0.15f, 0.15f, 0.15f, 1f);
            
            // Title
            var titleObj = new GameObject("Title");
            titleObj.transform.SetParent(panelObj.transform, false);
            var titleRect = titleObj.AddComponent<RectTransform>();
            titleRect.anchorMin = new Vector2(0, 0.75f);
            titleRect.anchorMax = new Vector2(1, 1);
            titleRect.sizeDelta = Vector2.zero;
            titleRect.anchoredPosition = Vector2.zero;
            
            var titleText = titleObj.AddComponent<Text>();
            titleText.text = title;
            titleText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            titleText.fontSize = 28;
            titleText.color = Color.white;
            titleText.alignment = TextAnchor.MiddleCenter;
            
            // Message
            var messageObj = new GameObject("Message");
            messageObj.transform.SetParent(panelObj.transform, false);
            var messageRect = messageObj.AddComponent<RectTransform>();
            messageRect.anchorMin = new Vector2(0.1f, 0.35f);
            messageRect.anchorMax = new Vector2(0.9f, 0.7f);
            messageRect.sizeDelta = Vector2.zero;
            messageRect.anchoredPosition = Vector2.zero;
            
            var messageText = messageObj.AddComponent<Text>();
            messageText.text = message;
            messageText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            messageText.fontSize = 22;
            messageText.color = new Color(0.9f, 0.9f, 0.9f, 1f);
            messageText.alignment = TextAnchor.UpperLeft;
            
            // Confirm button
            CreateDialogButton(panelObj, Localization.Instance.Get("start_settings_confirm"), 0.55f, 0.85f, true, () =>
            {
                onConfirm?.Invoke();
                Destroy(dialogObj);
            });
            
            // Cancel button
            CreateDialogButton(panelObj, Localization.Instance.Get("start_settings_cancel"), 0.15f, 0.45f, false, () =>
            {
                Destroy(dialogObj);
            });
        }

        private void ShowInputDialog(string title, string defaultValue, Action<string> onConfirm)
        {
            Utils.Debug.Log("StartSettings", $"Showing input dialog: {title}");
            
            // Create dialog overlay
            var dialogObj = new GameObject("InputDialog");
            dialogObj.transform.SetParent(transform, false);
            
            var dialogRect = dialogObj.AddComponent<RectTransform>();
            dialogRect.anchorMin = Vector2.zero;
            dialogRect.anchorMax = Vector2.one;
            dialogRect.sizeDelta = Vector2.zero;
            dialogRect.anchoredPosition = Vector2.zero;
            
            // Overlay background
            var overlayBg = dialogObj.AddComponent<Image>();
            overlayBg.color = new Color(0, 0, 0, 0.8f);
            
            // Dialog panel
            var panelObj = new GameObject("Panel");
            panelObj.transform.SetParent(dialogObj.transform, false);
            
            var panelRect = panelObj.AddComponent<RectTransform>();
            panelRect.anchorMin = new Vector2(0.5f, 0.5f);
            panelRect.anchorMax = new Vector2(0.5f, 0.5f);
            panelRect.pivot = new Vector2(0.5f, 0.5f);
            panelRect.sizeDelta = new Vector2(GetComponent<RectTransform>().rect.width * 0.7f, UnitHeight * 3.5f);
            panelRect.anchoredPosition = Vector2.zero;
            
            var panelBg = panelObj.AddComponent<Image>();
            panelBg.color = new Color(0.15f, 0.15f, 0.15f, 1f);
            
            // Title
            var titleObj = new GameObject("Title");
            titleObj.transform.SetParent(panelObj.transform, false);
            var titleRect = titleObj.AddComponent<RectTransform>();
            titleRect.anchorMin = new Vector2(0, 0.75f);
            titleRect.anchorMax = new Vector2(1, 1);
            titleRect.sizeDelta = Vector2.zero;
            titleRect.anchoredPosition = Vector2.zero;
            
            var titleText = titleObj.AddComponent<Text>();
            titleText.text = title;
            titleText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            titleText.fontSize = 28;
            titleText.color = Color.white;
            titleText.alignment = TextAnchor.MiddleCenter;
            
            // Input field
            var inputObj = new GameObject("InputField");
            inputObj.transform.SetParent(panelObj.transform, false);
            var inputRect = inputObj.AddComponent<RectTransform>();
            inputRect.anchorMin = new Vector2(0.1f, 0.45f);
            inputRect.anchorMax = new Vector2(0.9f, 0.7f);
            inputRect.sizeDelta = Vector2.zero;
            inputRect.anchoredPosition = Vector2.zero;
            
            var inputBg = inputObj.AddComponent<Image>();
            inputBg.color = new Color(0.1f, 0.1f, 0.1f, 1f);
            
            var inputField = inputObj.AddComponent<InputField>();
            inputField.text = defaultValue;
            
            var inputTextObj = new GameObject("Text");
            inputTextObj.transform.SetParent(inputObj.transform, false);
            var inputTextRect = inputTextObj.AddComponent<RectTransform>();
            inputTextRect.anchorMin = new Vector2(0.05f, 0);
            inputTextRect.anchorMax = new Vector2(0.95f, 1);
            inputTextRect.sizeDelta = Vector2.zero;
            inputTextRect.anchoredPosition = Vector2.zero;
            
            var inputText = inputTextObj.AddComponent<Text>();
            inputText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            inputText.fontSize = 24;
            inputText.color = Color.white;
            inputText.alignment = TextAnchor.MiddleLeft;
            inputText.supportRichText = false;
            
            inputField.textComponent = inputText;
            
            // Confirm button
            CreateDialogButton(panelObj, Localization.Instance.Get("start_settings_confirm"), 0.55f, 0.85f, true, () =>
            {
                onConfirm?.Invoke(inputField.text);
                Destroy(dialogObj);
            });
            
            // Cancel button
            CreateDialogButton(panelObj, Localization.Instance.Get("start_settings_cancel"), 0.15f, 0.45f, false, () =>
            {
                Destroy(dialogObj);
            });
        }

        private void ShowAddAccountDialog(Action<string, string, string> onConfirm)
        {
            Utils.Debug.Log("StartSettings", "Showing add account dialog");
            
            // Create dialog overlay
            var dialogObj = new GameObject("AddAccountDialog");
            dialogObj.transform.SetParent(transform, false);
            
            var dialogRect = dialogObj.AddComponent<RectTransform>();
            dialogRect.anchorMin = Vector2.zero;
            dialogRect.anchorMax = Vector2.one;
            dialogRect.sizeDelta = Vector2.zero;
            dialogRect.anchoredPosition = Vector2.zero;
            
            // Overlay background
            var overlayBg = dialogObj.AddComponent<Image>();
            overlayBg.color = new Color(0, 0, 0, 0.8f);
            
            // Dialog panel
            var panelObj = new GameObject("Panel");
            panelObj.transform.SetParent(dialogObj.transform, false);
            
            var panelRect = panelObj.AddComponent<RectTransform>();
            panelRect.anchorMin = new Vector2(0.5f, 0.5f);
            panelRect.anchorMax = new Vector2(0.5f, 0.5f);
            panelRect.pivot = new Vector2(0.5f, 0.5f);
            panelRect.sizeDelta = new Vector2(GetComponent<RectTransform>().rect.width * 0.7f, UnitHeight * 5.5f);
            panelRect.anchoredPosition = Vector2.zero;
            
            var panelBg = panelObj.AddComponent<Image>();
            panelBg.color = new Color(0.15f, 0.15f, 0.15f, 1f);
            
            // Title
            var titleObj = new GameObject("Title");
            titleObj.transform.SetParent(panelObj.transform, false);
            var titleRect = titleObj.AddComponent<RectTransform>();
            titleRect.anchorMin = new Vector2(0, 0.85f);
            titleRect.anchorMax = new Vector2(1, 1);
            titleRect.sizeDelta = Vector2.zero;
            titleRect.anchoredPosition = Vector2.zero;
            
            var titleText = titleObj.AddComponent<Text>();
            titleText.text = Localization.Instance.Get("start_settings_add_account");
            titleText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            titleText.fontSize = 28;
            titleText.color = Color.white;
            titleText.alignment = TextAnchor.MiddleCenter;
            
            // Account ID input
            var idField = CreateInputFieldWithLabel(panelObj, Localization.Instance.Get("start_settings_account_id"), "", 0.65f, 0.8f);
            
            // Password input
            var passwordField = CreateInputFieldWithLabel(panelObj, Localization.Instance.Get("start_settings_password"), "", 0.48f, 0.63f);
            
            // Note input
            var noteField = CreateInputFieldWithLabel(panelObj, Localization.Instance.Get("start_settings_note_optional"), "", 0.31f, 0.46f);
            
            // Confirm button
            CreateDialogButton(panelObj, Localization.Instance.Get("start_settings_confirm"), 0.55f, 0.85f, true, () =>
            {
                string id = idField.text;
                string password = passwordField.text;
                string note = noteField.text;
                
                if (string.IsNullOrEmpty(id) || string.IsNullOrEmpty(password))
                {
                    ShowErrorDialog(Localization.Instance.Get("start_settings_id_password_required"));
                    return;
                }
                
                onConfirm?.Invoke(id, password, note);
                Destroy(dialogObj);
            });
            
            // Cancel button
            CreateDialogButton(panelObj, Localization.Instance.Get("start_settings_cancel"), 0.15f, 0.45f, false, () =>
            {
                Destroy(dialogObj);
            });
        }

        private InputField CreateInputFieldWithLabel(GameObject parent, string label, string defaultValue, float yMin, float yMax)
        {
            var containerObj = new GameObject(label.Replace(" ", ""));
            containerObj.transform.SetParent(parent.transform, false);
            
            var containerRect = containerObj.AddComponent<RectTransform>();
            containerRect.anchorMin = new Vector2(0.1f, yMin);
            containerRect.anchorMax = new Vector2(0.9f, yMax);
            containerRect.sizeDelta = Vector2.zero;
            containerRect.anchoredPosition = Vector2.zero;
            
            // Label
            var labelObj = new GameObject("Label");
            labelObj.transform.SetParent(containerObj.transform, false);
            var labelRect = labelObj.AddComponent<RectTransform>();
            labelRect.anchorMin = new Vector2(0, 0.6f);
            labelRect.anchorMax = new Vector2(1, 1);
            labelRect.sizeDelta = Vector2.zero;
            labelRect.anchoredPosition = Vector2.zero;
            
            var labelText = labelObj.AddComponent<Text>();
            labelText.text = label;
            labelText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            labelText.fontSize = 20;
            labelText.color = new Color(0.8f, 0.8f, 0.8f, 1f);
            labelText.alignment = TextAnchor.LowerLeft;
            
            // Input field
            var inputObj = new GameObject("InputField");
            inputObj.transform.SetParent(containerObj.transform, false);
            var inputRect = inputObj.AddComponent<RectTransform>();
            inputRect.anchorMin = new Vector2(0, 0);
            inputRect.anchorMax = new Vector2(1, 0.5f);
            inputRect.sizeDelta = Vector2.zero;
            inputRect.anchoredPosition = Vector2.zero;
            
            var inputBg = inputObj.AddComponent<Image>();
            inputBg.color = new Color(0.1f, 0.1f, 0.1f, 1f);
            
            var inputField = inputObj.AddComponent<InputField>();
            inputField.text = defaultValue;
            
            var inputTextObj = new GameObject("Text");
            inputTextObj.transform.SetParent(inputObj.transform, false);
            var inputTextRect = inputTextObj.AddComponent<RectTransform>();
            inputTextRect.anchorMin = new Vector2(0.05f, 0);
            inputTextRect.anchorMax = new Vector2(0.95f, 1);
            inputTextRect.sizeDelta = Vector2.zero;
            inputTextRect.anchoredPosition = Vector2.zero;
            
            var inputText = inputTextObj.AddComponent<Text>();
            inputText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            inputText.fontSize = 22;
            inputText.color = Color.white;
            inputText.alignment = TextAnchor.MiddleLeft;
            inputText.supportRichText = false;
            
            inputField.textComponent = inputText;
            
            return inputField;
        }

        private void ShowErrorDialog(string message)
        {
            Utils.Debug.Log("StartSettings", $"Showing error dialog: {message}");
            ShowConfirmDialog(Localization.Instance.Get("start_settings_error"), message, null);
        }

        private void CreateDialogButton(GameObject parent, string label, float xMin, float xMax, bool isConfirm, Action onClick)
        {
            var buttonObj = new GameObject($"{label}Button");
            buttonObj.transform.SetParent(parent.transform, false);
            
            var rect = buttonObj.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(xMin, 0.05f);
            rect.anchorMax = new Vector2(xMax, 0.25f);
            rect.sizeDelta = Vector2.zero;
            rect.anchoredPosition = Vector2.zero;
            
            var bg = buttonObj.AddComponent<Image>();
            bg.color = isConfirm ? 
                new Color(0.2f, 0.5f, 0.8f, 0.9f) : 
                new Color(0.4f, 0.4f, 0.4f, 0.9f);
            
            var textObj = new GameObject("Text");
            textObj.transform.SetParent(buttonObj.transform, false);
            var textRect = textObj.AddComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.sizeDelta = Vector2.zero;
            textRect.anchoredPosition = Vector2.zero;
            
            var text = textObj.AddComponent<Text>();
            text.text = label;
            text.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            text.fontSize = 24;
            text.color = Color.white;
            text.alignment = TextAnchor.MiddleCenter;
            
            var button = buttonObj.AddComponent<Button>();
            button.onClick.AddListener(() => onClick?.Invoke());
        }
        #endregion

        #region Event Handlers
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
        #endregion
    }
}
