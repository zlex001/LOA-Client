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

        public enum Event
        {
            ServerSelect,
        }
        
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
        #endregion

        #region Lifecycle Methods

        public override void OnEnter(params object[] args)
        {
            var texts = args[0] as Dictionary<string, string>;
            if (texts != null)
            {
                _uiTexts = texts;
                
                transform.Find("Title").GetComponent<Text>().text = texts["title"];
                transform.Find("Block/Text").GetComponent<Text>().text = texts["tip"];
                
                if (texts.ContainsKey("loginButton"))
                {
                    transform.Find("Login/Text").GetComponent<Text>().text = texts["loginButton"];
                }
                
                if (texts.ContainsKey("footer"))
                {
                    string footerTemplate = texts["footer"].Replace("\\n", "\n");
                    transform.Find("Footer").GetComponent<Text>().text = string.Format(
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
            // Hide Block initially for title animation
            transform.Find("Block").gameObject.SetActive(false);
            
            // Settings button - create first so ApplyAbsoluteLayout can position it
            InitializeSettingsButton();
            
            ApplyAbsoluteLayout();

            // Initialize server selector
            transform.Find("Servers").GetComponent<InfiniWheel>().Init(Data.Instance.Servers.Select(s => s.Name).ToArray());
            transform.Find("Servers").GetComponent<InfiniWheel>().Select(Data.Instance.User.SelectedServerIndex);
            transform.Find("Servers").GetComponent<InfiniWheel>().ValueChange += OnServersValueChange;

            // Block animation
            var block = transform.Find("Block");
            var blockText = transform.Find("Block/Text");
            blockText.GetComponent<Text>().DOFade(0, 1f).SetLoops(-1, LoopType.Yoyo);
            
            // Remove Button from Block (prevent full-screen click)
            var blockButton = block.GetComponent<Button>();
            if (blockButton != null)
            {
                GameObject.Destroy(blockButton);
                Utils.Debug.Log("Start", "Removed Button from Block to prevent full-screen click");
            }
            
            // Disable Block's Image raycast to prevent full-screen click
            var blockImage = block.GetComponent<Image>();
            if (blockImage != null)
            {
                blockImage.raycastTarget = false;
                Utils.Debug.Log("Start", "Disabled Block Image raycast");
            }
            
            // Add Button to Block/Text only
            var blockTextButton = blockText.gameObject.AddComponent<Button>();
            blockTextButton.targetGraphic = blockText.GetComponent<Text>();
            blockTextButton.onClick.AddListener(OnBlockClick);
            Utils.Debug.Log("Start", "Added Button to Block/Text for precise click area");
            
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
            
            // Calculate heights
            float titleHeight = UnitHeight * 6;
            float serverHeight = UnitHeight * 3;
            float footerHeight = UnitHeight;
            
            // Visual center: title center at 61.8% from bottom (golden ratio)
            float titleCenterY = screenHeight * GoldenRatio;
            float titleBottomY = titleCenterY - titleHeight / 2;
            float titleTopY = titleCenterY + titleHeight / 2;
            
            // Footer (bottom, shortened to avoid Settings button)
            var footerRect = transform.Find("Footer")?.GetComponent<RectTransform>();
            if (footerRect != null)
            {
                float padding = UnitHeight * 0.25f;
                float buttonSize = UnitHeight;
                float footerWidth = screenWidth - (padding + buttonSize + padding * 2);  // Leave space for Settings button
                
                footerRect.anchorMin = Vector2.zero;
                footerRect.anchorMax = Vector2.zero;
                footerRect.pivot = new Vector2(0f, 0f);  // Left-bottom aligned
                footerRect.sizeDelta = new Vector2(footerWidth, footerHeight);
                footerRect.anchoredPosition = new Vector2(padding, 0);
            }
            
            // Servers selector (below title, with spacing)
            float serversSpacing = UnitHeight * 0.5f;
            float serversY = titleBottomY - serversSpacing - serverHeight / 2;
            var serversRect = transform.Find("Servers")?.GetComponent<RectTransform>();
            if (serversRect != null)
            {
                serversRect.anchorMin = Vector2.zero;
                serversRect.anchorMax = Vector2.zero;
                serversRect.pivot = new Vector2(0.5f, 0.5f);
                serversRect.sizeDelta = new Vector2(screenWidth * 0.6f, serverHeight);
                serversRect.anchoredPosition = new Vector2(screenWidth / 2, serversY);
            }
            
            var serversWheel = transform.Find("Servers")?.GetComponent<InfiniWheel>();
            if (serversWheel != null)
            {
                serversWheel.Select(serversWheel.SelectedItem);
            }
            
            // Title (visual center)
            var titleRect = transform.Find("Title")?.GetComponent<RectTransform>();
            if (titleRect != null)
            {
                titleRect.anchorMin = Vector2.zero;
                titleRect.anchorMax = Vector2.zero;
                titleRect.pivot = new Vector2(0.5f, 0.5f);
                titleRect.sizeDelta = new Vector2(screenWidth, titleHeight);
                titleRect.anchoredPosition = new Vector2(screenWidth / 2, titleCenterY);
            }
            
            // Set title font size to 76 (38 × 2)
            var titleText = transform.Find("Title")?.GetComponent<Text>();
            if (titleText != null)
            {
                titleText.fontSize = 76;
            }
            
            // Author (if exists, positioned relative to title)
            var authorRect = transform.Find("Author")?.GetComponent<RectTransform>();
            if (authorRect != null)
            {
                authorRect.anchorMin = Vector2.zero;
                authorRect.anchorMax = Vector2.zero;
                authorRect.pivot = new Vector2(0.5f, 0.5f);
                authorRect.sizeDelta = new Vector2(UnitHeight * 0.5f, UnitHeight);
                authorRect.anchoredPosition = new Vector2(screenWidth * GoldenRatio, titleCenterY);
            }
            
            // QuickStart/Block (fullscreen, for click detection)
            var quickStartRect = transform.Find("QuickStart")?.GetComponent<RectTransform>();
            if (quickStartRect != null)
            {
                quickStartRect.anchorMin = Vector2.zero;
                quickStartRect.anchorMax = Vector2.one;
                quickStartRect.sizeDelta = Vector2.zero;
                quickStartRect.anchoredPosition = Vector2.zero;
            }
            
            // Settings button (bottom-right corner, aligned with Footer)
            var settingsRect = transform.Find("Settings")?.GetComponent<RectTransform>();
            if (settingsRect != null)
            {
                float buttonSize = UnitHeight;  // 83px (1 unit height, square)
                float padding = UnitHeight * 0.25f;  // 21px (small fixed padding, industry standard)
                
                // Align vertically with Footer center (footerHeight / 2)
                float buttonCenterY = footerHeight / 2;
                float buttonY = buttonCenterY + buttonSize / 2;  // Convert center to top edge (pivot is top-right)
                
                settingsRect.anchorMin = Vector2.zero;
                settingsRect.anchorMax = Vector2.zero;
                settingsRect.pivot = new Vector2(1f, 1f);  // Top-right pivot
                settingsRect.sizeDelta = new Vector2(buttonSize, buttonSize);
                settingsRect.anchoredPosition = new Vector2(screenWidth - padding, buttonY);
                
                Utils.Debug.Log("Start", $"Settings button layout: size={buttonSize}, padding={padding}, footerHeight={footerHeight}, buttonY={buttonY}");
            }
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
            
            var authorImage = transform.Find("Author").GetComponent<Image>();
            var titleText = transform.Find("Title").GetComponent<Text>();
            var titleRect = transform.Find("Title").GetComponent<RectTransform>();
            
            // Set author initial state
            authorImage.color = new Color(authorImage.color.r, authorImage.color.g, authorImage.color.b, 0);
            authorImage.DOFade(1, ANIMATION_TIME * 0.6f).SetEase(Ease.OutQuad);
            
            // Store original values
            Color originalTitleColor = titleText.color;
            Vector3 originalPos = titleRect.anchoredPosition;
            Vector3 originalScale = titleRect.localScale;
            
            switch (TITLE_ANIMATION)
            {
                case AnimationType.SimpleFade:
                    Utils.Debug.Log("Start", "Animation: Simple Fade");
                    titleText.color = new Color(originalTitleColor.r, originalTitleColor.g, originalTitleColor.b, 0f);
                    titleText.DOFade(1, ANIMATION_TIME).SetEase(Ease.InOutQuad);
                    break;
                    
                case AnimationType.GoldGlow:
                    Utils.Debug.Log("Start", "Animation: Gold Glow");
                    titleText.color = new Color(1f, 0.84f, 0f, 0f);  // Gold color, alpha 0
                    DOTween.To(
                        () => titleText.color,
                        x => titleText.color = x,
                        new Color(originalTitleColor.r, originalTitleColor.g, originalTitleColor.b, 1f),
                        ANIMATION_TIME
                    ).SetEase(Ease.OutQuad);
                    break;
                    
                case AnimationType.ScaleFade:
                    Utils.Debug.Log("Start", "Animation: Scale + Fade");
                    titleText.color = new Color(originalTitleColor.r, originalTitleColor.g, originalTitleColor.b, 0f);
                    titleRect.localScale = originalScale * 0.5f;
                    titleText.DOFade(1, ANIMATION_TIME).SetEase(Ease.OutQuad);
                    titleRect.DOScale(originalScale, ANIMATION_TIME).SetEase(Ease.OutBack);
                    break;
                    
                case AnimationType.SlideFade:
                    Utils.Debug.Log("Start", "Animation: Slide from top + Fade");
                    titleText.color = new Color(originalTitleColor.r, originalTitleColor.g, originalTitleColor.b, 0f);
                    titleRect.anchoredPosition = originalPos + new Vector3(0, 200, 0);
                    titleText.DOFade(1, ANIMATION_TIME).SetEase(Ease.InOutQuad);
                    titleRect.DOAnchorPos(originalPos, ANIMATION_TIME).SetEase(Ease.OutCubic);
                    break;
                    
                case AnimationType.OverbrightGlow:
                    Utils.Debug.Log("Start", "Animation: Overbright Glow");
                    titleText.color = new Color(2f, 2f, 2f, 0f);
                    DOTween.To(
                        () => titleText.color,
                        x => titleText.color = x,
                        new Color(originalTitleColor.r, originalTitleColor.g, originalTitleColor.b, 1f),
                        ANIMATION_TIME
                    ).SetEase(Ease.OutQuad);
                    
                    var outline = titleText.GetComponent<Outline>();
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
            transform.Find("Block").gameObject.SetActive(true);
            Utils.Debug.Log("Start", "=== Title Animation Finished ===");
        }
        #endregion

        #region Server 
        private void OnServersValueChange(int index, Text text)
        {
            Game.Event.Instance.Fire(Event.ServerSelect, index);
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
                    
                    if (code == LoginResponse.Code.PasswordError)
                    {
                        Utils.Debug.Log("Start", "Marking account as error");
                        transform.Find("Accounts").GetComponent<LoopListView2>()
                            .GetShownItemByIndex(Data.Instance.User.SelectedAccountIndex)
                            .GetComponent<StartAccount>().Error();
                    }
                }
            }
        }
        #endregion
    }
}