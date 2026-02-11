using DG.Tweening;
using Framework;
using UnityEngine.UI;
using SuperScrollView;
using UnityEngine;
using System.Linq;
using Game.Protocol;
using UnityTimer;
using System.Collections.Generic;
using System;
using Game.Flow;



namespace Game
{
    public class Start : UI.Core
    {
        #region Enums and Constants
        private const float UnitHeight = 83f;
        private const float GoldenRatio = 0.618f;
        
        public enum AnimationType
        {
            SimpleFade,        // 1. Pure fade in
            GoldGlow,          // 2. Gold to white transition
            ScaleFade,         // 3. Scale + fade
            SlideFade,         // 4. Slide from top + fade
            OverbrightGlow     // 5. Overbright white glow (current)
        }
        
        // Change this to test different animations!
        private const AnimationType TITLE_ANIMATION = AnimationType.SimpleFade;
        private const float ANIMATION_TIME = 1.618f;  // Golden Ratio reciprocal (φ) seconds
        #endregion

        #region Fields
        private Dictionary<string, string> _uiTexts;
        private Text _titleText;
        private Text _footerText;
        private GameObject _tipContainer;
        private Text _tipText;
        #endregion

        #region Lifecycle Methods

        public override void OnEnter(params object[] args)
        {
            var texts = args[0] as Dictionary<string, string>;
            if (texts != null)
            {
                _uiTexts = texts;
                
                // Set title text
                if (_titleText != null && texts.ContainsKey("title"))
                {
                    _titleText.text = texts["title"];
                }
                
                // Set tip text
                if (_tipText != null && texts.ContainsKey("tip"))
                {
                    _tipText.text = texts["tip"];
                    
                    // Update text rect size to fit content
                    var textRect = _tipText.GetComponent<RectTransform>();
                    textRect.sizeDelta = new Vector2(_tipText.preferredWidth, _tipText.preferredHeight);
                }
                
                // Set footer text
                if (_footerText != null && texts.ContainsKey("footer"))
                {
                    string footerTemplate = texts["footer"].Replace("\\n", "\n");
                    _footerText.text = string.Format(
                        footerTemplate,
                        Data.Instance.Device.Split("-").Last(),
                        Data.Instance.AppVersion,
                        Data.Instance.HotVersion
                    );
                }
                
            }
        }

        public override void OnCreate(params object[] args)
        {
            // Set default server (single server mode)
            Data.Instance.User.SelectedServerIndex = 0;
            
            // Create all UI elements dynamically
            CreateTitleUI();
            CreateFooterUI();
            InitializeSettingsButton();
            CreateTipUI();
            
            // Setup UI Listeners
            Data.Instance.after.Register(Data.Type.LoginResponse, OnAfterLoginResponseChanged);
            Utils.Debug.Log("Start", "LoginResponse event listener registered");

            StartCoroutine(Animate());
        }

        public override void OnClose()
        {
            Utils.Debug.Log("Start", "OnClose called, unregistering LoginResponse event listener");
            Data.Instance.after.Unregister(Data.Type.LoginResponse, OnAfterLoginResponseChanged);
        }

        public override void OnScreenAdaptationChanged()
        {
            ApplyAbsoluteLayout();
        }

        private void ApplyAbsoluteLayout()
        {
            float screenWidth = GetComponent<RectTransform>().rect.width;
            float screenHeight = GetComponent<RectTransform>().rect.height;
            
            float titleHeight = UnitHeight * 6;
            float footerHeight = UnitHeight;
            float titleCenterY = screenHeight * GoldenRatio;
            float padding = UnitHeight * 0.25f;
            float buttonSize = UnitHeight;
            
            // Reposition Title
            if (_titleText != null)
            {
                var titleRect = _titleText.GetComponent<RectTransform>();
                titleRect.sizeDelta = new Vector2(screenWidth, titleHeight);
                titleRect.anchoredPosition = new Vector2(screenWidth / 2, titleCenterY);
            }
            
            // Reposition Footer
            if (_footerText != null)
            {
                var footerRect = _footerText.GetComponent<RectTransform>();
                float footerWidth = screenWidth - (padding + buttonSize + padding * 2);
                footerRect.sizeDelta = new Vector2(footerWidth, footerHeight);
                footerRect.anchoredPosition = new Vector2(padding, 0);
            }
            
            // Reposition Settings button
            var settingsRect = transform.Find("Settings")?.GetComponent<RectTransform>();
            if (settingsRect != null)
            {
                float buttonCenterY = footerHeight / 2;
                float buttonY = buttonCenterY + buttonSize / 2;
                settingsRect.anchoredPosition = new Vector2(screenWidth - padding, buttonY);
            }
        }
        #endregion

        #region UI Creation
        private void CreateTitleUI()
        {
            float screenHeight = GetComponent<RectTransform>().rect.height;
            float screenWidth = GetComponent<RectTransform>().rect.width;
            
            // Calculate title center at golden ratio position
            float titleHeight = UnitHeight * 6;
            float titleCenterY = screenHeight * GoldenRatio;
            
            // Create Title GameObject
            var titleObj = new GameObject("Title");
            titleObj.transform.SetParent(transform, false);
            
            var titleRect = titleObj.AddComponent<RectTransform>();
            titleRect.anchorMin = Vector2.zero;
            titleRect.anchorMax = Vector2.zero;
            titleRect.pivot = new Vector2(0.5f, 0.5f);
            titleRect.sizeDelta = new Vector2(screenWidth, titleHeight);
            titleRect.anchoredPosition = new Vector2(screenWidth / 2, titleCenterY);
            
            _titleText = titleObj.AddComponent<Text>();
            _titleText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            _titleText.fontSize = 76;
            _titleText.alignment = TextAnchor.MiddleCenter;
            _titleText.color = new Color(1f, 1f, 1f, 0f); // Start with alpha=0 for animation
            _titleText.raycastTarget = false;
            
            Utils.Debug.Log("Start", $"Created Title UI at golden ratio position: {titleCenterY}px ({GoldenRatio * 100}%)");
        }
        
        private void CreateFooterUI()
        {
            float screenWidth = GetComponent<RectTransform>().rect.width;
            float footerHeight = UnitHeight;
            
            // Calculate footer width (leave space for Settings button)
            float padding = UnitHeight * 0.25f;
            float buttonSize = UnitHeight;
            float footerWidth = screenWidth - (padding + buttonSize + padding * 2);
            
            // Create Footer GameObject
            var footerObj = new GameObject("Footer");
            footerObj.transform.SetParent(transform, false);
            
            var footerRect = footerObj.AddComponent<RectTransform>();
            footerRect.anchorMin = Vector2.zero;
            footerRect.anchorMax = Vector2.zero;
            footerRect.pivot = new Vector2(0f, 0f);  // Left-bottom aligned
            footerRect.sizeDelta = new Vector2(footerWidth, footerHeight);
            footerRect.anchoredPosition = new Vector2(padding, 0);
            
            _footerText = footerObj.AddComponent<Text>();
            _footerText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            _footerText.fontSize = 38;
            _footerText.alignment = TextAnchor.MiddleRight;
            _footerText.color = Color.white;
            _footerText.raycastTarget = false;
            
            // Add FontScaler
            footerObj.AddComponent<Framework.FontScaler>();
            
            Utils.Debug.Log("Start", $"Created Footer UI: width={footerWidth}px, height={footerHeight}px");
        }
        
        private void CreateTipUI()
        {
            float screenHeight = GetComponent<RectTransform>().rect.height;
            float titleHeight = UnitHeight * 6;
            
            // Calculate position: middle between Footer top and Title bottom
            float footerTop = UnitHeight;
            float titleBottom = screenHeight * GoldenRatio - titleHeight / 2;
            float middleY = (footerTop + titleBottom) / 2;
            
            // Create container
            _tipContainer = new GameObject("Tip");
            _tipContainer.transform.SetParent(transform, false);
            
            var containerRect = _tipContainer.AddComponent<RectTransform>();
            containerRect.anchorMin = new Vector2(0.5f, 0f);
            containerRect.anchorMax = new Vector2(0.5f, 0f);
            containerRect.pivot = new Vector2(0.5f, 0.5f);
            containerRect.sizeDelta = new Vector2(0f, UnitHeight);
            containerRect.anchoredPosition = new Vector2(0f, middleY);
            
            // Create text
            var textObj = new GameObject("Text");
            textObj.transform.SetParent(_tipContainer.transform, false);
            
            var textRect = textObj.AddComponent<RectTransform>();
            textRect.anchorMin = new Vector2(0.5f, 0.5f);
            textRect.anchorMax = new Vector2(0.5f, 0.5f);
            textRect.pivot = new Vector2(0.5f, 0.5f);
            textRect.anchoredPosition = Vector2.zero;
            
            _tipText = textObj.AddComponent<Text>();
            _tipText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            _tipText.fontSize = 38;
            _tipText.alignment = TextAnchor.MiddleCenter;
            _tipText.color = new Color(1f, 1f, 1f, 0f); // Start with alpha=0
            _tipText.raycastTarget = false;
            
            // Note: Animation will be started in Animate() after fade-in
            
            // Create click area (sized to text content with padding)
            var clickAreaObj = new GameObject("ClickArea");
            clickAreaObj.transform.SetParent(textObj.transform, false);
            
            var clickAreaRect = clickAreaObj.AddComponent<RectTransform>();
            clickAreaRect.anchorMin = Vector2.zero;
            clickAreaRect.anchorMax = Vector2.one;
            clickAreaRect.offsetMin = new Vector2(-20f, -10f);
            clickAreaRect.offsetMax = new Vector2(20f, 10f);
            
            // Add transparent image for raycast target
            var clickAreaImage = clickAreaObj.AddComponent<Image>();
            clickAreaImage.color = new Color(0, 0, 0, 0);
            clickAreaImage.raycastTarget = true;
            
            // Add button
            var clickButton = clickAreaObj.AddComponent<Button>();
            clickButton.targetGraphic = clickAreaImage;
            clickButton.onClick.AddListener(OnBlockClick);
            
            // Hide initially (will show after title animation)
            _tipContainer.SetActive(false);
            
            Utils.Debug.Log("Start", $"Created Tip UI at middle position: {middleY}px");
        }
        #endregion

        #region Animation
        private void OnBlockClick()
        {
            Utils.Debug.Log("Start", "Screen clicked, initiating login process");
            
            // Show connecting UI
            UI.Instance.Open(Config.UI.Dark, Localization.Instance.Get("connecting"));
            
            // Check if local accounts exist
            if (Data.Instance.User.Accounts.Count > 0)
            {
                // Has accounts: use local account to login
                Utils.Debug.Log("Start", $"Using local account: {Data.Instance.SelectedAccount.Id}");
                Data.Instance.LoginAccount = Data.Instance.SelectedAccount;
            }
            else
            {
                // No accounts: send QuickStart request
                Utils.Debug.Log("Start", "No local accounts, sending QuickStartRequest");
                Net.Instance.Send(new QuickStartRequest
                {
                    device = Data.Instance.Device,
                    version = Data.Instance.AppVersion,
                    platform = Application.platform.ToString(),
                    language = Data.Instance.Language.ToString()
                });
            }
        }

        private System.Collections.IEnumerator Animate()
        {
            Utils.Debug.Log("Start", $"=== Title Animation Started: {TITLE_ANIMATION} ===");
            Utils.Debug.Log("Start", $"Animation Time: {ANIMATION_TIME} seconds");
            
            if (_titleText == null)
            {
                Utils.Debug.LogError("Start", "Title text is null, cannot animate");
                yield break;
            }
            
            var titleRect = _titleText.GetComponent<RectTransform>();
            
            // Store original values
            Color originalTitleColor = _titleText.color;
            Vector3 originalPos = titleRect.anchoredPosition;
            Vector3 originalScale = titleRect.localScale;
            
            switch (TITLE_ANIMATION)
            {
                case AnimationType.SimpleFade:
                    Utils.Debug.Log("Start", "Animation: Simple Fade");
                    _titleText.color = new Color(originalTitleColor.r, originalTitleColor.g, originalTitleColor.b, 0f);
                    _titleText.DOFade(1, ANIMATION_TIME).SetEase(Ease.InOutQuad);
                    break;
                    
                case AnimationType.GoldGlow:
                    Utils.Debug.Log("Start", "Animation: Gold Glow");
                    _titleText.color = new Color(1f, 0.84f, 0f, 0f);  // Gold color, alpha 0
                    DOTween.To(
                        () => _titleText.color,
                        x => _titleText.color = x,
                        new Color(originalTitleColor.r, originalTitleColor.g, originalTitleColor.b, 1f),
                        ANIMATION_TIME
                    ).SetEase(Ease.OutQuad);
                    break;
                    
                case AnimationType.ScaleFade:
                    Utils.Debug.Log("Start", "Animation: Scale + Fade");
                    _titleText.color = new Color(originalTitleColor.r, originalTitleColor.g, originalTitleColor.b, 0f);
                    titleRect.localScale = originalScale * 0.5f;
                    _titleText.DOFade(1, ANIMATION_TIME).SetEase(Ease.OutQuad);
                    titleRect.DOScale(originalScale, ANIMATION_TIME).SetEase(Ease.OutBack);
                    break;
                    
                case AnimationType.SlideFade:
                    Utils.Debug.Log("Start", "Animation: Slide from top + Fade");
                    _titleText.color = new Color(originalTitleColor.r, originalTitleColor.g, originalTitleColor.b, 0f);
                    titleRect.anchoredPosition = originalPos + new Vector3(0, 200, 0);
                    _titleText.DOFade(1, ANIMATION_TIME).SetEase(Ease.InOutQuad);
                    titleRect.DOAnchorPos(originalPos, ANIMATION_TIME).SetEase(Ease.OutCubic);
                    break;
                    
                case AnimationType.OverbrightGlow:
                    Utils.Debug.Log("Start", "Animation: Overbright Glow");
                    _titleText.color = new Color(2f, 2f, 2f, 0f);
                    DOTween.To(
                        () => _titleText.color,
                        x => _titleText.color = x,
                        new Color(originalTitleColor.r, originalTitleColor.g, originalTitleColor.b, 1f),
                        ANIMATION_TIME
                    ).SetEase(Ease.OutQuad);
                    
                    var outline = _titleText.GetComponent<Outline>();
                    if (outline != null)
                    {
                        outline.effectColor = new Color(1f, 1f, 1f, 0f);
                        outline.effectDistance = new Vector2(8f, 8f);
                        DOTween.To(
                            () => outline.effectColor.a,
                            x => outline.effectColor = new Color(1f, 1f, 1f, x),
                            0.5f,
                            ANIMATION_TIME * 0.5f
                        ).SetEase(Ease.OutQuad).OnComplete(() => {
                            DOTween.To(
                                () => outline.effectColor.a,
                                x => outline.effectColor = new Color(1f, 1f, 1f, x),
                                0f,
                                ANIMATION_TIME * 0.5f
                            ).SetEase(Ease.InQuad);
                            DOTween.To(
                                () => outline.effectDistance,
                                x => outline.effectDistance = x,
                                new Vector2(2f, 2f),
                                ANIMATION_TIME * 0.5f
                            ).SetEase(Ease.InQuad);
                        });
                    }
                    break;
            }
            
            yield return new WaitForSeconds(ANIMATION_TIME);
            
            // Fade in tip UI
            if (_tipContainer != null && _tipText != null)
            {
                _tipContainer.SetActive(true);
                Utils.Debug.Log("Start", "Starting Tip fade-in animation");
                
                // Fade in from alpha=0 to alpha=1
                yield return _tipText.DOFade(1f, 0.8f).SetEase(Ease.InOutQuad).WaitForCompletion();
                
                Utils.Debug.Log("Start", "Tip fade-in completed, starting pulse animation");
                
                // Start pulsing animation (1.0 -> 0.3 -> 1.0, loop)
                _tipText.DOFade(0.3f, 1f).SetLoops(-1, LoopType.Yoyo).SetEase(Ease.InOutQuad);
            }
            
            Utils.Debug.Log("Start", "=== Title Animation Finished ===");
        }
        #endregion

        #region Settings
        private void InitializeSettingsButton()
        {
            var settingsButton = transform.Find("Settings");
            
            // If Settings button doesn't exist, create it dynamically
            if (settingsButton == null)
            {
                Utils.Debug.Log("Start", "Settings button not found, creating dynamically");
                settingsButton = CreateSettingsButton();
            }

            if (settingsButton == null)
            {
                Utils.Debug.LogError("Start", "Failed to create Settings button");
                return;
            }

            // Always show settings button (for language, audio, and account management)
            settingsButton.gameObject.SetActive(true);
            var button = settingsButton.GetComponent<Button>();
            if (button == null)
            {
                button = settingsButton.gameObject.AddComponent<Button>();
            }
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(OnSettingsClick);
            Utils.Debug.Log("Start", "Settings button enabled (always visible)");
        }

        private Transform CreateSettingsButton()
        {
            // Create Settings button GameObject
            GameObject settingsObj = new GameObject("Settings");
            settingsObj.transform.SetParent(transform, false);

            // Add RectTransform
            var rect = settingsObj.AddComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.zero;
            rect.pivot = new Vector2(1f, 1f);
            
            // Set button size and position
            float buttonSize = UnitHeight;
            float padding = UnitHeight * 0.25f;
            float screenWidth = GetComponent<RectTransform>().rect.width;
            float footerHeight = UnitHeight;
            float buttonCenterY = footerHeight / 2;
            float buttonY = buttonCenterY + buttonSize / 2;
            
            rect.sizeDelta = new Vector2(buttonSize, buttonSize);
            rect.anchoredPosition = new Vector2(screenWidth - padding, buttonY);

            // Add Image component for button background
            var image = settingsObj.AddComponent<Image>();
            
            // Load Settings.png using AssetManager (same pattern as other UI assets)
            try
            {
                var settingsSprite = AssetManager.Instance.LoadSprite("RawAssets/Texture", "Settings");
                if (settingsSprite != null)
                {
                    image.sprite = settingsSprite;
                    image.type = Image.Type.Simple;
                    image.preserveAspect = true;
                    image.color = Color.white;
                    Utils.Debug.Log("Start", "Settings sprite loaded successfully from RawAssets/Texture/Settings");
                }
                else
                {
                    // Fallback: use a simple colored circle
                    image.color = new Color(0.8f, 0.8f, 0.8f, 0.8f);
                    Utils.Debug.LogWarning("Start", "Settings.png not found in RawAssets/Texture/, using fallback color");
                }
            }
            catch (System.Exception e)
            {
                Utils.Debug.LogWarning("Start", $"Failed to load Settings sprite: {e.Message}, using fallback color");
                image.color = new Color(0.8f, 0.8f, 0.8f, 0.8f);
            }

            // Add Button component
            var button = settingsObj.AddComponent<Button>();
            button.targetGraphic = image;

            Utils.Debug.Log("Start", "Settings button created dynamically");
            return settingsObj.transform;
        }

        private void OnSettingsClick()
        {
            Utils.Debug.Log("Start", "Settings button clicked");
            UI.Instance.Open(Config.UI.StartSettings);
        }
        #endregion

        #region Login


        private void OnAfterLoginResponseChanged(params object[] args)
        {
            Utils.Debug.Log("Start", $"OnAfterLoginResponseChanged triggered, args count: {args.Length}");
            int v = (int)args[0];
            LoginResponse.Code code = (LoginResponse.Code)v;
            Utils.Debug.Log("Start", $"LoginResponse code: {code} ({v})");
            
            if (code == LoginResponse.Code.Success)
            {
                Utils.Debug.Log("Start", "Login success, closing Start UI");
                Close();
            }
            else
            {
                Utils.Debug.Log("Start", $"Login failed, closing Dark and showing error");
                Data.Instance.Dark = null;
                
                string message = Data.Instance.LoginResponseMessage;
                Utils.Debug.Log("Start", $"Error message: {message}");
                if (!string.IsNullOrEmpty(message))
                {
                    Utils.Debug.Log("Start", "Setting FlyTip");
                    Data.Instance.Tip = (UI.Tips.Fly, message);
                }
            }
        }
        #endregion
    }
}