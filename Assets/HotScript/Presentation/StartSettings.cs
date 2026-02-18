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
        
        // Color scheme - Dark theme (unified with game design)
        // RectangleSolid.png is a black rounded rectangle; Image.color is multiplicative tint
        // White tint preserves sprite's natural dark appearance
        private static readonly Color ColorPanelBackground = Color.white;
        private static readonly Color ColorMaskBackground = new Color(0f, 0f, 0f, 0.5f);
        private static readonly Color ColorItemBackground = Color.white;
        private static readonly Color ColorButtonEdit = new Color(0.2f, 0.6f, 1f, 1f);
        private static readonly Color ColorButtonDelete = new Color(1f, 0.4f, 0.4f, 1f);
        private static readonly Color ColorTextPrimary = Color.white;
        private static readonly Color ColorTextSecondary = new Color(1f, 1f, 1f, 0.6f);
        private static readonly Color ColorTextAccent = new Color(0.2f, 0.5f, 0.9f, 1f);
        private static readonly Color ColorSectionHeader = new Color(1f, 1f, 1f, 0.5f);
        private static readonly Color ColorPlaceholder = new Color(1f, 1f, 1f, 0.5f);
        private static readonly Color ColorDialogMask = new Color(0f, 0f, 0f, 0.8f);
        private static readonly Color ColorDialogPanel = Color.white;
        private static readonly Color ColorInputFieldBackground = Color.white;
        private static readonly Color ColorToggleBackground = Color.white;
        private static readonly Color ColorTransparent = new Color(1f, 1f, 1f, 0f);
        #endregion
        
        #region Helper Methods
        /// <summary>
        /// Get text from SDUI data layer
        /// </summary>
        private string GetText(string key)
        {
            var texts = Data.Instance.StartSettingsTexts;
            if (texts != null && texts.ContainsKey(key))
            {
                return texts[key];
            }
            return key;
        }
        
        /// <summary>
        /// Apply interactive color feedback to button
        /// </summary>
        private void ApplyButtonColorBlock(Button button, bool transparent = false)
        {
            var colors = button.colors;
            if (transparent)
            {
                // FIX: For text-only buttons, use transparent colors
                colors.normalColor = new Color(1f, 1f, 1f, 0f);
                colors.highlightedColor = new Color(0.9607843f, 0.9607843f, 0.9607843f, 0.3f);
                colors.pressedColor = new Color(0.78431374f, 0.78431374f, 0.78431374f, 0.5f);
                colors.selectedColor = new Color(1f, 1f, 1f, 0f);
                colors.disabledColor = new Color(0.78431374f, 0.78431374f, 0.78431374f, 0.2f);
            }
            else
            {
                colors.normalColor = Color.white;
                colors.highlightedColor = new Color(0.9607843f, 0.9607843f, 0.9607843f, 1f);
                colors.pressedColor = new Color(0.78431374f, 0.78431374f, 0.78431374f, 1f);
                colors.selectedColor = new Color(0.9607843f, 0.9607843f, 0.9607843f, 1f);
                colors.disabledColor = new Color(0.78431374f, 0.78431374f, 0.78431374f, 0.5019608f);
            }
            button.colors = colors;
            button.transition = Selectable.Transition.ColorTint;
        }
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

        #region Sprite Cache
        private Sprite _rectangleSolid;
        private Sprite _bottomBorderGradient;
        private Sprite _checkmark;
        #endregion

        #region Lifecycle
        public override void OnCreate(params object[] args)
        {
            Utils.Debug.Log("StartSettings", "OnCreate - Simplified version with dynamic UI");
            
            // Load sprites
            _rectangleSolid = AssetManager.Instance.LoadSprite("Textures", "RectangleSolid");
            _bottomBorderGradient = AssetManager.Instance.LoadSprite("Textures", "BottomBorder");
            _checkmark = AssetManager.Instance.LoadSprite("Textures", "True");
            
            // Cache references
            _panelRect = transform.Find("Panel")?.GetComponent<RectTransform>();
            _maskImage = GetComponent<Image>();
            
            // Ensure Panel has Image component to block clicks from reaching mask
            var panelImage = _panelRect?.GetComponent<Image>();
            if (panelImage == null)
            {
                panelImage = _panelRect.gameObject.AddComponent<Image>();
            }
            if (_rectangleSolid != null) panelImage.sprite = _rectangleSolid;
            panelImage.type = Image.Type.Sliced;
            panelImage.color = ColorPanelBackground;
            panelImage.raycastTarget = true; // Intercept clicks on panel
            
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
            
            // Receive and store UI texts from server
            if (args.Length > 0 && args[0] is Dictionary<string, string> texts)
            {
                Data.Instance.StartSettingsTexts = texts;
            }
            
            // Register language change observer
            Data.Instance.after.Register(Data.Type.Language, OnLanguageChanged);
            
            if (_panelRect == null || _maskImage == null) return;
            
            float panelHeight = UnitHeight * PanelHeightUnits;
            
            // Initial state
            _panelRect.anchoredPosition = new Vector2(0, -panelHeight);
            _maskImage.color = new Color(ColorMaskBackground.r, ColorMaskBackground.g, ColorMaskBackground.b, 0f);
            
            // Animate
            var sequence = DOTween.Sequence();
            sequence.Append(_maskImage.DOColor(ColorMaskBackground, 0.25f));
            sequence.Join(_panelRect.DOAnchorPosY(0, AnimationDuration).SetEase(Ease.OutCubic));
            
            Utils.Debug.Log("StartSettings", "Animation sequence started");
        }

        public override void OnExit()
        {
            Utils.Debug.Log("StartSettings", "OnExit - closing panel");
            
            // Unregister language change observer
            Data.Instance.after.Unregister(Data.Type.Language, OnLanguageChanged);
            
            if (_panelRect != null) _panelRect.DOKill();
            if (_maskImage != null) _maskImage.DOKill();
        }
        
        private void OnLanguageChanged(params object[] args)
        {
            Utils.Debug.Log("StartSettings", $"Language changed, rebuilding UI");
            
            // FIX: Reload localization for new language
            Localization.Instance.Init(Data.Instance.Language.ToString());
            
            BuildUI();
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
            CreateSectionHeader(GetText("accounts"));
            
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
            CreateSectionHeader(GetText("general"));
            
            // Language item
            CreateLanguageItem();
            
            // Font size item
            CreateFontSizeItem();
            
            // Sound item
            CreateSoundItem();
            
            Utils.Debug.Log("StartSettings", "Settings section built");
        }

        private void CreateSectionHeader(string title)
        {
            var headerObj = new GameObject("SectionHeader");
            headerObj.transform.SetParent(_scrollContent.transform, false);
            
            float sectionHeaderHeight = UnitHeight * 0.618f;
            var headerRect = headerObj.AddComponent<RectTransform>();
            headerRect.sizeDelta = new Vector2(0, sectionHeaderHeight);
            
            var headerText = headerObj.AddComponent<Text>();
            headerText.text = title;
            headerText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            headerText.fontSize = 38;
            headerText.color = ColorSectionHeader;
            headerText.alignment = TextAnchor.MiddleLeft;
            headerObj.AddComponent<Framework.FontScaler>();
            
            var padding = headerObj.AddComponent<LayoutElement>();
            padding.preferredHeight = sectionHeaderHeight;
        }

        private GameObject CreateAccountItem(Account account, bool isSelected, int index)
        {
            var itemObj = new GameObject($"AccountItem_{index}");
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
            itemButton.onClick.AddListener(() => OnAccountSwitch(account));
            ApplyButtonColorBlock(itemButton);
            
            // Account text (left aligned)
            var textObj = new GameObject("Text");
            textObj.transform.SetParent(itemObj.transform, false);
            
            var textRect = textObj.AddComponent<RectTransform>();
            textRect.anchorMin = new Vector2(0, 0);
            textRect.anchorMax = new Vector2(0.6f, 1);
            textRect.sizeDelta = Vector2.zero;
            textRect.anchoredPosition = Vector2.zero;
            
            var text = textObj.AddComponent<Text>();
            // Display format: "AccountID (Note)" or just "AccountID" if no note
            text.text = account.Id + (string.IsNullOrEmpty(account.Note) ? "" : $" ({account.Note})");
            text.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            text.fontSize = 38;
            text.color = isSelected ? ColorTextAccent : ColorTextPrimary;
            text.alignment = TextAnchor.MiddleLeft;
            textObj.AddComponent<Framework.FontScaler>();
            
            // Edit button (right side) - unified anchor positioning
            var editButtonObj = new GameObject("EditButton");
            editButtonObj.transform.SetParent(itemObj.transform, false);
            
            var editButtonRect = editButtonObj.AddComponent<RectTransform>();
            editButtonRect.anchorMin = new Vector2(0.7f, 0.25f);
            editButtonRect.anchorMax = new Vector2(0.84f, 0.75f);
            editButtonRect.sizeDelta = Vector2.zero;
            
            var editButtonImage = editButtonObj.AddComponent<Image>();
            editButtonImage.color = ColorTransparent;
            
            var editButton = editButtonObj.AddComponent<Button>();
            editButton.targetGraphic = editButtonImage;
            editButton.onClick.AddListener(() => OnAccountEdit(account));
            ApplyButtonColorBlock(editButton);
            
            var editTextObj = new GameObject("Text");
            editTextObj.transform.SetParent(editButtonObj.transform, false);
            
            var editTextRect = editTextObj.AddComponent<RectTransform>();
            editTextRect.anchorMin = Vector2.zero;
            editTextRect.anchorMax = Vector2.one;
            editTextRect.sizeDelta = Vector2.zero;
            
            var editText = editTextObj.AddComponent<Text>();
            editText.text = GetText("edit");
            editText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            editText.fontSize = 38;
            editText.color = ColorButtonEdit;
            editText.alignment = TextAnchor.MiddleCenter;
            editTextObj.AddComponent<Framework.FontScaler>();
            
            // Delete button (right side) - unified anchor positioning
            var deleteButtonObj = new GameObject("DeleteButton");
            deleteButtonObj.transform.SetParent(itemObj.transform, false);
            
            var deleteButtonRect = deleteButtonObj.AddComponent<RectTransform>();
            deleteButtonRect.anchorMin = new Vector2(0.85f, 0.25f);
            deleteButtonRect.anchorMax = new Vector2(0.97f, 0.75f);
            deleteButtonRect.sizeDelta = Vector2.zero;
            
            var deleteButtonImage = deleteButtonObj.AddComponent<Image>();
            deleteButtonImage.color = ColorTransparent;
            
            var deleteButton = deleteButtonObj.AddComponent<Button>();
            deleteButton.targetGraphic = deleteButtonImage;
            deleteButton.onClick.AddListener(() => OnAccountDelete(account));
            ApplyButtonColorBlock(deleteButton);
            
            var deleteTextObj = new GameObject("Text");
            deleteTextObj.transform.SetParent(deleteButtonObj.transform, false);
            
            var deleteTextRect = deleteTextObj.AddComponent<RectTransform>();
            deleteTextRect.anchorMin = Vector2.zero;
            deleteTextRect.anchorMax = Vector2.one;
            deleteTextRect.sizeDelta = Vector2.zero;
            
            var deleteText = deleteTextObj.AddComponent<Text>();
            deleteText.text = GetText("delete");
            deleteText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            deleteText.fontSize = 38;
            deleteText.color = ColorButtonDelete;
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
            buttonImage.color = ColorTransparent;
            
            var button = buttonObj.AddComponent<Button>();
            button.targetGraphic = buttonImage;
            button.onClick.AddListener(OnAddAccountClick);
            ApplyButtonColorBlock(button);
            
            var textObj = new GameObject("Text");
            textObj.transform.SetParent(buttonObj.transform, false);
            
            var textRect = textObj.AddComponent<RectTransform>();
            textRect.anchorMin = new Vector2(0, 0);
            textRect.anchorMax = new Vector2(1, 1);
            textRect.sizeDelta = Vector2.zero;
            textRect.anchoredPosition = Vector2.zero;
            
            var text = textObj.AddComponent<Text>();
            text.text = GetText("add_account");
            text.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            text.fontSize = 38;
            text.color = ColorTextAccent;
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
            valueText.text = GetLanguageName(Data.Instance.Language);
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
            
            // Label (left side)
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
            
            // Font size control container (right side)
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
            
            // Minus button (text only)
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
            
            // Value text
            var valueTextObj = new GameObject("ValueText");
            valueTextObj.transform.SetParent(controlObj.transform, false);
            
            var valueTextRect = valueTextObj.AddComponent<RectTransform>();
            valueTextRect.sizeDelta = new Vector2(200, 40);  // FIX: Increased from 120 to 200 to accommodate long text
            
            var valueText = valueTextObj.AddComponent<Text>();
            valueText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            valueText.fontSize = 36;
            valueText.color = ColorTextSecondary;
            valueText.alignment = TextAnchor.MiddleCenter;
            valueTextObj.AddComponent<Framework.FontScaler>();
            
            // Set current font size label
            int[] fontSizes = { 25, 30, 35, 40 };
            string[] fontLabels = {
                GetText("font_size_small"),
                GetText("font_size_medium"),
                GetText("font_size_large"),
                GetText("font_size_extra_large")
            };
            int currentFontSize = Data.Instance.FontSize;
            int currentIndex = System.Array.IndexOf(fontSizes, currentFontSize);
            if (currentIndex < 0) currentIndex = 1; // Default to medium
            valueText.text = fontLabels[currentIndex];
            
            // Plus button (text only)
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
            
            // Create Toggle with proper structure
            var toggleObj = new GameObject("Toggle");
            toggleObj.transform.SetParent(itemObj.transform, false);
            
            var toggleRect = toggleObj.AddComponent<RectTransform>();
            toggleRect.anchorMin = new Vector2(1, 0.5f);
            toggleRect.anchorMax = new Vector2(1, 0.5f);
            toggleRect.sizeDelta = new Vector2(50, 50);
            toggleRect.anchoredPosition = new Vector2(-30, 0);
            
            // Background
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
            
            // Checkmark
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
            
            ShowEditAccountDialog(account.Id ?? "", account.Password ?? "", account.Note ?? "", (newId, newPassword, newNote) =>
            {
                if (string.IsNullOrEmpty(newId) || string.IsNullOrEmpty(newPassword))
                {
                    Utils.Debug.LogWarning("StartSettings", "Account ID and Password cannot be empty");
                    return;
                }
                
                account.Id = newId;
                account.Password = newPassword;
                account.Note = newNote;
                Local.Instance.Save(Data.Instance.User);
                BuildUI();
            });
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
                GetText("delete_confirm"),
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
            
            ShowAddAccountDialog((accountId, password, note) =>
            {
                if (string.IsNullOrEmpty(accountId) || string.IsNullOrEmpty(password))
                {
                    Utils.Debug.LogWarning("StartSettings", "Account ID and Password cannot be empty");
                    return;
                }
                
                var newAccount = new Account
                {
                    Id = accountId,
                    Password = password,
                    Note = note
                };
                
                _accounts.Add(newAccount);
                Data.Instance.User.Accounts.Add(newAccount);
                Local.Instance.Save(Data.Instance.User);
                
                Utils.Debug.Log("StartSettings", $"Added new account: {newAccount.Id}");
                BuildUI();
            });
        }
        #endregion

        #region Settings Callbacks
        private string GetLanguageName(Data.Languages language)
        {
            return GetText($"lang_{language}");
        }
        
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
        
        private void OnFontSizeDecrease()
        {
            int[] fontSizes = { 25, 30, 35, 40 };
            int currentFontSize = Data.Instance.FontSize;
            int currentIndex = System.Array.IndexOf(fontSizes, currentFontSize);
            if (currentIndex < 0) currentIndex = 1;
            
            if (currentIndex > 0)
            {
                currentIndex--;
                Data.Instance.FontSize = fontSizes[currentIndex];
                Local.Instance.Save(Data.Instance.User);
                BuildUI();
                Utils.Debug.Log("StartSettings", $"Font size decreased to {fontSizes[currentIndex]}");
            }
        }
        
        private void OnFontSizeIncrease()
        {
            int[] fontSizes = { 25, 30, 35, 40 };
            int currentFontSize = Data.Instance.FontSize;
            int currentIndex = System.Array.IndexOf(fontSizes, currentFontSize);
            if (currentIndex < 0) currentIndex = 1;
            
            if (currentIndex < fontSizes.Length - 1)
            {
                currentIndex++;
                Data.Instance.FontSize = fontSizes[currentIndex];
                Local.Instance.Save(Data.Instance.User);
                BuildUI();
                Utils.Debug.Log("StartSettings", $"Font size increased to {fontSizes[currentIndex]}");
            }
        }
        #endregion

        #region Dialog Helpers
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
            
            // Semi-transparent mask
            var mask = dialogObj.AddComponent<Image>();
            mask.color = ColorDialogMask;
            
            // Add button to mask to intercept clicks and prevent penetration
            var maskButton = dialogObj.AddComponent<Button>();
            maskButton.targetGraphic = mask;
            maskButton.onClick.AddListener(() => GameObject.Destroy(dialogObj));
            
            // Dialog panel
            var panelObj = new GameObject("Panel");
            panelObj.transform.SetParent(dialogObj.transform, false);
            
            var panelRect = panelObj.AddComponent<RectTransform>();
            panelRect.anchorMin = new Vector2(0.5f, 0.5f);
            panelRect.anchorMax = new Vector2(0.5f, 0.5f);
            panelRect.sizeDelta = new Vector2(600, 500);
            panelRect.anchoredPosition = Vector2.zero;
            
            var panelImage = panelObj.AddComponent<Image>();
            if (_rectangleSolid != null) panelImage.sprite = _rectangleSolid;
            panelImage.type = Image.Type.Sliced;
            panelImage.color = ColorDialogPanel;
            
            // Title
            var titleObj = new GameObject("Title");
            titleObj.transform.SetParent(panelObj.transform, false);
            
            var titleRect = titleObj.AddComponent<RectTransform>();
            titleRect.anchorMin = new Vector2(0.1f, 0.85f);
            titleRect.anchorMax = new Vector2(0.9f, 0.95f);
            titleRect.sizeDelta = Vector2.zero;
            
            var titleText = titleObj.AddComponent<Text>();
            titleText.text = GetText("edit_account");
            titleText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            titleText.fontSize = 38;
            titleText.color = ColorTextPrimary;
            titleText.alignment = TextAnchor.MiddleCenter;
            titleObj.AddComponent<Framework.FontScaler>();
            
            // Account ID input
            var accountField = CreateInputField(panelObj.transform, "AccountField", 
                new Vector2(0.1f, 0.65f), new Vector2(0.9f, 0.8f),
                GetText("account_id"), "");
            
            // Password input
            var passwordField = CreateInputField(panelObj.transform, "PasswordField",
                new Vector2(0.1f, 0.48f), new Vector2(0.9f, 0.63f),
                GetText("password"), "");
            passwordField.inputType = InputField.InputType.Password;
            
            // Note input
            var noteField = CreateInputField(panelObj.transform, "NoteField",
                new Vector2(0.1f, 0.31f), new Vector2(0.9f, 0.46f),
                GetText("note_optional"), "");
            
            // Cancel button
            CreateDialogButton(panelObj.transform, "Cancel", new Vector2(0.15f, 0.1f), new Vector2(0.45f, 0.25f),
                GetText("cancel"),
                () => GameObject.Destroy(dialogObj));
            
            // Confirm button
            CreateDialogButton(panelObj.transform, "Confirm", new Vector2(0.55f, 0.1f), new Vector2(0.85f, 0.25f),
                GetText("confirm"),
                () => {
                    onConfirm?.Invoke(accountField.text, passwordField.text, noteField.text);
                    GameObject.Destroy(dialogObj);
                });
        }

        private void ShowEditAccountDialog(string defaultId, string defaultPassword, string defaultNote, Action<string, string, string> onConfirm)
        {
            Utils.Debug.Log("StartSettings", "Showing edit account dialog");
            
            // Create dialog overlay
            var dialogObj = new GameObject("EditAccountDialog");
            dialogObj.transform.SetParent(transform, false);
            
            var dialogRect = dialogObj.AddComponent<RectTransform>();
            dialogRect.anchorMin = Vector2.zero;
            dialogRect.anchorMax = Vector2.one;
            dialogRect.sizeDelta = Vector2.zero;
            
            // Semi-transparent mask
            var mask = dialogObj.AddComponent<Image>();
            mask.color = ColorDialogMask;
            
            // Add button to mask to intercept clicks and prevent penetration
            var maskButton = dialogObj.AddComponent<Button>();
            maskButton.targetGraphic = mask;
            maskButton.onClick.AddListener(() => GameObject.Destroy(dialogObj));
            
            // Dialog panel
            var panelObj = new GameObject("Panel");
            panelObj.transform.SetParent(dialogObj.transform, false);
            
            var panelRect = panelObj.AddComponent<RectTransform>();
            panelRect.anchorMin = new Vector2(0.5f, 0.5f);
            panelRect.anchorMax = new Vector2(0.5f, 0.5f);
            panelRect.sizeDelta = new Vector2(600, 500);
            panelRect.anchoredPosition = Vector2.zero;
            
            var panelImage = panelObj.AddComponent<Image>();
            if (_rectangleSolid != null) panelImage.sprite = _rectangleSolid;
            panelImage.type = Image.Type.Sliced;
            panelImage.color = ColorDialogPanel;
            
            // Title
            var titleObj = new GameObject("Title");
            titleObj.transform.SetParent(panelObj.transform, false);
            
            var titleRect = titleObj.AddComponent<RectTransform>();
            titleRect.anchorMin = new Vector2(0.1f, 0.8f);
            titleRect.anchorMax = new Vector2(0.9f, 0.95f);
            titleRect.sizeDelta = Vector2.zero;
            
            var titleText = titleObj.AddComponent<Text>();
            titleText.text = GetText("edit_account");
            titleText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            titleText.fontSize = 38;
            titleText.color = ColorTextPrimary;
            titleText.alignment = TextAnchor.MiddleCenter;
            titleObj.AddComponent<Framework.FontScaler>();
            
            // Account ID input
            var idField = CreateInputField(panelObj.transform, "AccountIdField",
                new Vector2(0.1f, 0.63f), new Vector2(0.9f, 0.78f),
                GetText("account_id"), defaultId);
            
            // Password input
            var passwordField = CreateInputField(panelObj.transform, "PasswordField",
                new Vector2(0.1f, 0.45f), new Vector2(0.9f, 0.6f),
                GetText("password"), defaultPassword);
            passwordField.inputType = InputField.InputType.Password;
            
            // Note input
            var noteField = CreateInputField(panelObj.transform, "NoteField",
                new Vector2(0.1f, 0.27f), new Vector2(0.9f, 0.42f),
                GetText("note_optional"), defaultNote);
            
            // Cancel button
            CreateDialogButton(panelObj.transform, "Cancel", new Vector2(0.15f, 0.1f), new Vector2(0.45f, 0.25f),
                GetText("cancel"),
                () => GameObject.Destroy(dialogObj));
            
            // Confirm button
            CreateDialogButton(panelObj.transform, "Confirm", new Vector2(0.55f, 0.1f), new Vector2(0.85f, 0.25f),
                GetText("confirm"),
                () => {
                    onConfirm?.Invoke(idField.text, passwordField.text, noteField.text);
                    GameObject.Destroy(dialogObj);
                });
        }

        private InputField CreateInputField(Transform parent, string name, Vector2 anchorMin, Vector2 anchorMax, 
            string placeholder, string defaultValue)
        {
            var inputObj = new GameObject(name);
            inputObj.transform.SetParent(parent, false);
            
            var inputRect = inputObj.AddComponent<RectTransform>();
            inputRect.anchorMin = anchorMin;
            inputRect.anchorMax = anchorMax;
            inputRect.sizeDelta = Vector2.zero;
            
            var inputImage = inputObj.AddComponent<Image>();
            if (_rectangleSolid != null) inputImage.sprite = _rectangleSolid;
            inputImage.type = Image.Type.Sliced;
            inputImage.color = ColorInputFieldBackground;
            
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
            text.fontSize = 38;
            text.color = ColorTextPrimary;
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
            placeholderText.text = placeholder;
            placeholderText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            placeholderText.fontSize = 38;
            placeholderText.color = ColorPlaceholder;
            placeholderText.alignment = TextAnchor.MiddleLeft;
            placeholderText.supportRichText = false;
            placeholderObj.AddComponent<Framework.FontScaler>();
            
            inputField.placeholder = placeholderText;
            
            return inputField;
        }

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
            mask.color = ColorDialogMask;
            
            // Add button to mask to intercept clicks and prevent penetration
            var maskButton = dialogObj.AddComponent<Button>();
            maskButton.targetGraphic = mask;
            maskButton.onClick.AddListener(() => GameObject.Destroy(dialogObj));
            
            // Dialog panel
            var panelObj = new GameObject("Panel");
            panelObj.transform.SetParent(dialogObj.transform, false);
            
            var panelRect = panelObj.AddComponent<RectTransform>();
            panelRect.anchorMin = new Vector2(0.5f, 0.5f);
            panelRect.anchorMax = new Vector2(0.5f, 0.5f);
            panelRect.sizeDelta = new Vector2(600, 300);
            panelRect.anchoredPosition = Vector2.zero;
            
            var panelImage = panelObj.AddComponent<Image>();
            if (_rectangleSolid != null) panelImage.sprite = _rectangleSolid;
            panelImage.type = Image.Type.Sliced;
            panelImage.color = ColorDialogPanel;
            
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
            messageText.fontSize = 38;
            messageText.color = ColorTextPrimary;
            messageText.alignment = TextAnchor.MiddleCenter;
            messageObj.AddComponent<Framework.FontScaler>();
            
            // Cancel button
            CreateDialogButton(panelObj.transform, "Cancel", new Vector2(0.15f, 0.15f), new Vector2(0.45f, 0.35f),
                GetText("cancel"), 
                () => GameObject.Destroy(dialogObj));
            
            // Confirm button
            CreateDialogButton(panelObj.transform, "Confirm", new Vector2(0.55f, 0.15f), new Vector2(0.85f, 0.35f),
                GetText("confirm"),
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
            mask.color = ColorDialogMask;
            
            // Add button to mask to intercept clicks and prevent penetration
            var maskButton = dialogObj.AddComponent<Button>();
            maskButton.targetGraphic = mask;
            maskButton.onClick.AddListener(() => GameObject.Destroy(dialogObj));
            
            // Dialog panel
            var panelObj = new GameObject("Panel");
            panelObj.transform.SetParent(dialogObj.transform, false);
            
            var panelRect = panelObj.AddComponent<RectTransform>();
            panelRect.anchorMin = new Vector2(0.5f, 0.5f);
            panelRect.anchorMax = new Vector2(0.5f, 0.5f);
            panelRect.sizeDelta = new Vector2(600, 350);
            panelRect.anchoredPosition = Vector2.zero;
            
            var panelImage = panelObj.AddComponent<Image>();
            if (_rectangleSolid != null) panelImage.sprite = _rectangleSolid;
            panelImage.type = Image.Type.Sliced;
            panelImage.color = ColorDialogPanel;
            
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
            titleText.fontSize = 38;
            titleText.color = ColorTextPrimary;
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
            if (_rectangleSolid != null) inputImage.sprite = _rectangleSolid;
            inputImage.type = Image.Type.Sliced;
            inputImage.color = ColorInputFieldBackground;
            
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
            text.fontSize = 38;
            text.color = ColorTextPrimary;
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
            placeholderText.fontSize = 38;
            placeholderText.color = ColorPlaceholder;
            placeholderText.alignment = TextAnchor.MiddleLeft;
            placeholderText.supportRichText = false;
            placeholderObj.AddComponent<Framework.FontScaler>();
            
            inputField.placeholder = placeholderText;
            
            // Cancel button
            CreateDialogButton(panelObj.transform, "Cancel", new Vector2(0.15f, 0.1f), new Vector2(0.45f, 0.3f),
                GetText("cancel"),
                () => GameObject.Destroy(dialogObj));
            
            // Confirm button
            CreateDialogButton(panelObj.transform, "Confirm", new Vector2(0.55f, 0.1f), new Vector2(0.85f, 0.3f),
                GetText("confirm"),
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
            if (_rectangleSolid != null) buttonImage.sprite = _rectangleSolid;
            buttonImage.type = Image.Type.Sliced;
            buttonImage.color = ColorDialogPanel;
            
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
            text.fontSize = 38;
            text.color = name == "Confirm" ? ColorTextAccent : ColorTextPrimary;
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
