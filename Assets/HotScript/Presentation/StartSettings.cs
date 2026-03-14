using Framework;
using Game.Data;
using Game.Net;
using Game.Utils;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;

namespace Game.Presentation
{
    using Config = Game.Data.Config;

    public partial class StartSettings : UI.Core
    {
        #region Constants
        private const float UnitHeight = 83f;
        private const float PanelHeightUnits = 12f;
        private const float AnimationDuration = 0.3f;

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

        private string GetText(string key)
        {
            var texts = DataManager.Instance.Texts;
            if (texts != null && texts.TryGetValue(key, out var value) && !string.IsNullOrEmpty(value))
                return value;
            if (_loggedMissingKeys.Add(key))
                Utils.Debug.LogWarning("StartSettings", $"Texts missing or empty key: {key}");
            return key;
        }

        private void ApplyButtonColorBlock(Button button, bool transparent = false)
        {
            var colors = button.colors;
            if (transparent)
            {
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

        #region Fields
        private List<Account> _accounts;
        private int _selectedIndex;
        private RectTransform _panelRect;
        private Image _maskImage;
        private GameObject _scrollContent;
        private List<GameObject> _accountItemObjects = new List<GameObject>();
        private HashSet<string> _loggedMissingKeys = new HashSet<string>();
        private Sprite _rectangleSolid;
        private Sprite _bottomBorderGradient;
        private Sprite _checkmark;
        #endregion

        #region Lifecycle

        public override void OnCreate(params object[] args)
        {
            _rectangleSolid = AssetManager.Instance.LoadSprite("Textures", "RectangleSolid");
            _bottomBorderGradient = AssetManager.Instance.LoadSprite("Textures", "BottomBorder");
            _checkmark = AssetManager.Instance.LoadSprite("Textures", "True");

            _panelRect = transform.Find("Panel")?.GetComponent<RectTransform>();
            _maskImage = GetComponent<Image>();

            var panelImage = _panelRect?.GetComponent<Image>();
            if (panelImage == null)
            {
                panelImage = _panelRect.gameObject.AddComponent<Image>();
            }
            if (_rectangleSolid != null) panelImage.sprite = _rectangleSolid;
            panelImage.type = Image.Type.Sliced;
            panelImage.color = ColorPanelBackground;
            panelImage.raycastTarget = true;

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

            ApplyLayout();
            SetupButtons();
            LoadAccountData();
            BuildUI();
        }

        public override void OnEnter(params object[] args)
        {
            _loggedMissingKeys.Clear();

            DataManager.Instance.after.Register(DataManager.Type.Language, OnLanguageChanged);

            if (_panelRect == null || _maskImage == null) return;

            float panelHeight = UnitHeight * PanelHeightUnits;

            _panelRect.anchoredPosition = new Vector2(0, -panelHeight);
            _maskImage.color = new Color(ColorMaskBackground.r, ColorMaskBackground.g, ColorMaskBackground.b, 0f);

            var sequence = DOTween.Sequence();
            sequence.Append(_maskImage.DOColor(ColorMaskBackground, 0.25f));
            sequence.Join(_panelRect.DOAnchorPosY(0, AnimationDuration).SetEase(Ease.OutCubic));
        }

        public override void OnExit()
        {
            DataManager.Instance.after.Unregister(DataManager.Type.Language, OnLanguageChanged);

            if (_panelRect != null) _panelRect.DOKill();
            if (_maskImage != null) _maskImage.DOKill();
        }

        private void OnLanguageChanged(params object[] args)
        {
            Localization.Instance.Init(DataManager.Instance.Language.ToString());
            Net.Authentication.Gateway.Request(() => BuildUI());
        }

        #endregion

        #region Layout

        private void ApplyLayout()
        {
            if (_panelRect == null) return;

            float panelHeight = UnitHeight * PanelHeightUnits;

            _panelRect.anchorMin = new Vector2(0, 0);
            _panelRect.anchorMax = new Vector2(1, 0);
            _panelRect.pivot = new Vector2(0.5f, 0);
            _panelRect.sizeDelta = new Vector2(0, panelHeight);
            _panelRect.anchoredPosition = new Vector2(0, 0);
        }

        private void SetupButtons()
        {
            var closeButton = transform.Find("Panel/Header/Close")?.GetComponent<Button>();
            if (closeButton != null)
            {
                closeButton.onClick.AddListener(OnCloseClick);
            }

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
            _accounts = DataManager.Instance.User.Accounts.ToList();
            _selectedIndex = DataManager.Instance.User.SelectedAccountIndex;
        }

        #endregion

        #region UI Building

        private void BuildUI()
        {
            if (_scrollContent == null)
            {
                Utils.Debug.LogError("StartSettings", "Content container not found");
                return;
            }

            _scrollContent.ClearChildren();

            BuildAccountSection();
            BuildSettingsSection();
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

        #endregion

        #region Close

        private void OnCloseClick()
        {
            Close();
        }

        private void OnMaskClick()
        {
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
