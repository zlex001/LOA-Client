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
            
            // Account text (left aligned)
            var textObj = new GameObject("Text");
            textObj.transform.SetParent(itemObj.transform, false);
            
            var textRect = textObj.AddComponent<RectTransform>();
            textRect.anchorMin = new Vector2(0, 0);
            textRect.anchorMax = new Vector2(0.6f, 1);
            textRect.sizeDelta = Vector2.zero;
            textRect.anchoredPosition = Vector2.zero;
            
            var text = textObj.AddComponent<Text>();
            text.text = account.Id + (string.IsNullOrEmpty(account.Note) ? "" : $" ({account.Note})");
            text.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            text.fontSize = 28;
            text.color = Color.black;
            text.alignment = TextAnchor.MiddleLeft;
            textObj.AddComponent<Framework.FontScaler>();
            
            // Edit button (right side)
            var editButtonObj = new GameObject("EditButton");
            editButtonObj.transform.SetParent(itemObj.transform, false);
            
            var editButtonRect = editButtonObj.AddComponent<RectTransform>();
            editButtonRect.anchorMin = new Vector2(0.65f, 0.25f);
            editButtonRect.anchorMax = new Vector2(0.8f, 0.75f);
            editButtonRect.sizeDelta = Vector2.zero;
            
            var editButtonImage = editButtonObj.AddComponent<Image>();
            editButtonImage.color = new Color(0, 0.5f, 1f, 1f);
            
            var editButton = editButtonObj.AddComponent<Button>();
            editButton.targetGraphic = editButtonImage;
            editButton.onClick.AddListener(() => OnAccountEdit(account));
            
            var editTextObj = new GameObject("Text");
            editTextObj.transform.SetParent(editButtonObj.transform, false);
            
            var editTextRect = editTextObj.AddComponent<RectTransform>();
            editTextRect.anchorMin = Vector2.zero;
            editTextRect.anchorMax = Vector2.one;
            editTextRect.sizeDelta = Vector2.zero;
            
            var editText = editTextObj.AddComponent<Text>();
            editText.text = Localization.Instance.Get("start_settings_edit");
            editText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            editText.fontSize = 24;
            editText.color = Color.white;
            editText.alignment = TextAnchor.MiddleCenter;
            editTextObj.AddComponent<Framework.FontScaler>();
            
            // Delete button (right side)
            var deleteButtonObj = new GameObject("DeleteButton");
            deleteButtonObj.transform.SetParent(itemObj.transform, false);
            
            var deleteButtonRect = deleteButtonObj.AddComponent<RectTransform>();
            deleteButtonRect.anchorMin = new Vector2(0.82f, 0.25f);
            deleteButtonRect.anchorMax = new Vector2(0.97f, 0.75f);
            deleteButtonRect.sizeDelta = Vector2.zero;
            
            var deleteButtonImage = deleteButtonObj.AddComponent<Image>();
            deleteButtonImage.color = new Color(1f, 0.3f, 0.3f, 1f);
            
            var deleteButton = deleteButtonObj.AddComponent<Button>();
            deleteButton.targetGraphic = deleteButtonImage;
            deleteButton.onClick.AddListener(() => OnAccountDelete(account));
            
            var deleteTextObj = new GameObject("Text");
            deleteTextObj.transform.SetParent(deleteButtonObj.transform, false);
            
            var deleteTextRect = deleteTextObj.AddComponent<RectTransform>();
            deleteTextRect.anchorMin = Vector2.zero;
            deleteTextRect.anchorMax = Vector2.one;
            deleteTextRect.sizeDelta = Vector2.zero;
            
            var deleteText = deleteTextObj.AddComponent<Text>();
            deleteText.text = Localization.Instance.Get("start_settings_delete");
            deleteText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            deleteText.fontSize = 24;
            deleteText.color = Color.white;
            deleteText.alignment = TextAnchor.MiddleCenter;
            deleteTextObj.AddComponent<Framework.FontScaler>();
            
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

        private void OnAccountEdit(Account account)
        {
            Utils.Debug.Log("StartSettings", $"Editing account: {account.Id}");
            
            ShowInputDialog(
                Localization.Instance.Get("start_settings_edit_note"),
                account.Note ?? "",
                (newNote) =>
                {
                    account.Note = newNote;
                    Local.Instance.Save(Data.Instance.User);
                    BuildUI();
                }
            );
        }

        private void OnAccountDelete(Account account)
        {
            Utils.Debug.Log("StartSettings", $"Deleting account: {account.Id}");
            
            if (_accounts.Count <= 1)
            {
                Utils.Debug.LogWarning("StartSettings", "Cannot delete last account");
                return;
            }
            
            string message = string.Format(
                Localization.Instance.Get("start_settings_delete_confirm"),
                account.Id
            );
            
            ShowConfirmDialog(message, () =>
            {
                int index = _accounts.FindIndex(a => a.Id == account.Id);
                if (index < 0) return;
                
                _accounts.RemoveAt(index);
                Data.Instance.User.Accounts.RemoveAt(index);
                
                if (_selectedIndex == index)
                {
                    _selectedIndex = 0;
                    Data.Instance.User.SelectedAccountIndex = 0;
                }
                else if (_selectedIndex > index)
                {
                    _selectedIndex--;
                    Data.Instance.User.SelectedAccountIndex = _selectedIndex;
                }
                
                Local.Instance.Save(Data.Instance.User);
                BuildUI();
            });
        }

        private void OnAddAccountClick()
        {
            Utils.Debug.Log("StartSettings", "Add account clicked");
            
            ShowInputDialog(
                Localization.Instance.Get("start_settings_add_account"),
                "",
                (note) =>
                {
                    if (string.IsNullOrEmpty(note))
                    {
                        note = Localization.Instance.Get("start_settings_add_account");
                    }
                    
                    var newAccount = new Account
                    {
                        Id = $"Account_{System.DateTime.Now.Ticks}",
                        Note = note
                    };
                    
                    _accounts.Add(newAccount);
                    Data.Instance.User.Accounts.Add(newAccount);
                    Local.Instance.Save(Data.Instance.User);
                    
                    Utils.Debug.Log("StartSettings", $"Added new account: {newAccount.Id}");
                    BuildUI();
                }
            );
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

        #region Dialog Helpers
        private void ShowConfirmDialog(string message, Action onConfirm)
        {
            Utils.Debug.Log("StartSettings", $"Showing confirm dialog: {message}");
            
            // Create dialog overlay
            var dialogObj = new GameObject("ConfirmDialog");
            dialogObj.transform.SetParent(transform, false);
            
            var dialogRect = dialogObj.AddComponent<RectTransform>();
            dialogRect.anchorMin = Vector2.zero;
            dialogRect.anchorMax = Vector2.one;
            dialogRect.sizeDelta = Vector2.zero;
            
            // Semi-transparent mask
            var mask = dialogObj.AddComponent<Image>();
            mask.color = new Color(0, 0, 0, 0.8f);
            
            // Dialog panel
            var panelObj = new GameObject("Panel");
            panelObj.transform.SetParent(dialogObj.transform, false);
            
            var panelRect = panelObj.AddComponent<RectTransform>();
            panelRect.anchorMin = new Vector2(0.5f, 0.5f);
            panelRect.anchorMax = new Vector2(0.5f, 0.5f);
            panelRect.sizeDelta = new Vector2(600, 300);
            panelRect.anchoredPosition = Vector2.zero;
            
            var panelImage = panelObj.AddComponent<Image>();
            panelImage.color = Color.white;
            
            // Message text
            var messageObj = new GameObject("Message");
            messageObj.transform.SetParent(panelObj.transform, false);
            
            var messageRect = messageObj.AddComponent<RectTransform>();
            messageRect.anchorMin = new Vector2(0.1f, 0.4f);
            messageRect.anchorMax = new Vector2(0.9f, 0.8f);
            messageRect.sizeDelta = Vector2.zero;
            
            var messageText = messageObj.AddComponent<Text>();
            messageText.text = message;
            messageText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            messageText.fontSize = 28;
            messageText.color = Color.black;
            messageText.alignment = TextAnchor.MiddleCenter;
            messageObj.AddComponent<Framework.FontScaler>();
            
            // Cancel button
            CreateDialogButton(panelObj.transform, "Cancel", new Vector2(0.15f, 0.15f), new Vector2(0.45f, 0.35f),
                Localization.Instance.Get("start_settings_cancel"), 
                () => GameObject.Destroy(dialogObj));
            
            // Confirm button
            CreateDialogButton(panelObj.transform, "Confirm", new Vector2(0.55f, 0.15f), new Vector2(0.85f, 0.35f),
                Localization.Instance.Get("start_settings_confirm"),
                () => {
                    onConfirm?.Invoke();
                    GameObject.Destroy(dialogObj);
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
            
            // Semi-transparent mask
            var mask = dialogObj.AddComponent<Image>();
            mask.color = new Color(0, 0, 0, 0.8f);
            
            // Dialog panel
            var panelObj = new GameObject("Panel");
            panelObj.transform.SetParent(dialogObj.transform, false);
            
            var panelRect = panelObj.AddComponent<RectTransform>();
            panelRect.anchorMin = new Vector2(0.5f, 0.5f);
            panelRect.anchorMax = new Vector2(0.5f, 0.5f);
            panelRect.sizeDelta = new Vector2(600, 350);
            panelRect.anchoredPosition = Vector2.zero;
            
            var panelImage = panelObj.AddComponent<Image>();
            panelImage.color = Color.white;
            
            // Title text
            var titleObj = new GameObject("Title");
            titleObj.transform.SetParent(panelObj.transform, false);
            
            var titleRect = titleObj.AddComponent<RectTransform>();
            titleRect.anchorMin = new Vector2(0.1f, 0.7f);
            titleRect.anchorMax = new Vector2(0.9f, 0.9f);
            titleRect.sizeDelta = Vector2.zero;
            
            var titleText = titleObj.AddComponent<Text>();
            titleText.text = title;
            titleText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            titleText.fontSize = 32;
            titleText.color = Color.black;
            titleText.alignment = TextAnchor.MiddleCenter;
            titleObj.AddComponent<Framework.FontScaler>();
            
            // Input field
            var inputObj = new GameObject("InputField");
            inputObj.transform.SetParent(panelObj.transform, false);
            
            var inputRect = inputObj.AddComponent<RectTransform>();
            inputRect.anchorMin = new Vector2(0.1f, 0.45f);
            inputRect.anchorMax = new Vector2(0.9f, 0.65f);
            inputRect.sizeDelta = Vector2.zero;
            
            var inputImage = inputObj.AddComponent<Image>();
            inputImage.color = new Color(0.95f, 0.95f, 0.95f, 1f);
            
            var inputField = inputObj.AddComponent<InputField>();
            inputField.text = defaultValue;
            
            // Input field text
            var textObj = new GameObject("Text");
            textObj.transform.SetParent(inputObj.transform, false);
            
            var textRect = textObj.AddComponent<RectTransform>();
            textRect.anchorMin = new Vector2(0.05f, 0);
            textRect.anchorMax = new Vector2(0.95f, 1);
            textRect.sizeDelta = Vector2.zero;
            
            var text = textObj.AddComponent<Text>();
            text.text = defaultValue;
            text.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            text.fontSize = 28;
            text.color = Color.black;
            text.alignment = TextAnchor.MiddleLeft;
            text.supportRichText = false;
            textObj.AddComponent<Framework.FontScaler>();
            
            inputField.textComponent = text;
            
            // Placeholder
            var placeholderObj = new GameObject("Placeholder");
            placeholderObj.transform.SetParent(inputObj.transform, false);
            
            var placeholderRect = placeholderObj.AddComponent<RectTransform>();
            placeholderRect.anchorMin = new Vector2(0.05f, 0);
            placeholderRect.anchorMax = new Vector2(0.95f, 1);
            placeholderRect.sizeDelta = Vector2.zero;
            
            var placeholderText = placeholderObj.AddComponent<Text>();
            placeholderText.text = title;
            placeholderText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            placeholderText.fontSize = 28;
            placeholderText.color = new Color(0.5f, 0.5f, 0.5f, 0.5f);
            placeholderText.alignment = TextAnchor.MiddleLeft;
            placeholderText.supportRichText = false;
            placeholderObj.AddComponent<Framework.FontScaler>();
            
            inputField.placeholder = placeholderText;
            
            // Cancel button
            CreateDialogButton(panelObj.transform, "Cancel", new Vector2(0.15f, 0.1f), new Vector2(0.45f, 0.3f),
                Localization.Instance.Get("start_settings_cancel"),
                () => GameObject.Destroy(dialogObj));
            
            // Confirm button
            CreateDialogButton(panelObj.transform, "Confirm", new Vector2(0.55f, 0.1f), new Vector2(0.85f, 0.3f),
                Localization.Instance.Get("start_settings_confirm"),
                () => {
                    onConfirm?.Invoke(inputField.text);
                    GameObject.Destroy(dialogObj);
                });
        }

        private void CreateDialogButton(Transform parent, string name, Vector2 anchorMin, Vector2 anchorMax, 
            string buttonText, Action onClick)
        {
            var buttonObj = new GameObject(name);
            buttonObj.transform.SetParent(parent, false);
            
            var buttonRect = buttonObj.AddComponent<RectTransform>();
            buttonRect.anchorMin = anchorMin;
            buttonRect.anchorMax = anchorMax;
            buttonRect.sizeDelta = Vector2.zero;
            
            var buttonImage = buttonObj.AddComponent<Image>();
            buttonImage.color = name == "Confirm" 
                ? new Color(0, 0.5f, 1f, 1f)  // Blue for confirm
                : new Color(0.8f, 0.8f, 0.8f, 1f);  // Gray for cancel
            
            var button = buttonObj.AddComponent<Button>();
            button.targetGraphic = buttonImage;
            button.onClick.AddListener(() => onClick?.Invoke());
            
            var textObj = new GameObject("Text");
            textObj.transform.SetParent(buttonObj.transform, false);
            
            var textRect = textObj.AddComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.sizeDelta = Vector2.zero;
            
            var text = textObj.AddComponent<Text>();
            text.text = buttonText;
            text.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            text.fontSize = 32;
            text.color = name == "Confirm" ? Color.white : Color.black;
            text.alignment = TextAnchor.MiddleCenter;
            textObj.AddComponent<Framework.FontScaler>();
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
