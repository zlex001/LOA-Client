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
            ApplyAbsoluteLayout();

            // Initialize server selector
            transform.Find("Servers").GetComponent<InfiniWheel>().Init(Data.Instance.Servers.Select(s => s.Name).ToArray());
            transform.Find("Servers").GetComponent<InfiniWheel>().Select(Data.Instance.User.SelectedServerIndex);
            transform.Find("Servers").GetComponent<InfiniWheel>().ValueChange += OnServersValueChange;

            // Block animation
            transform.Find("Block/Text").GetComponent<Text>().DOFade(0, 1f).SetLoops(-1, LoopType.Yoyo);
            transform.Find("Block").GetComponent<Button>().onClick.AddListener(OnBlockClick);
            
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
            float titleHeight = UnitHeight * 5;
            float serverHeight = UnitHeight * 3;
            float buttonHeight = UnitHeight * 1.5f;
            float footerHeight = UnitHeight;
            
            float totalFixedHeight = titleHeight + serverHeight + buttonHeight + footerHeight;
            float remainingHeight = screenHeight - totalFixedHeight;
            float topPadding = remainingHeight * GoldenRatio;
            float bottomPadding = remainingHeight * (1 - GoldenRatio);
            
            float currentY = 0;
            
            // Footer (bottom)
            var footerRect = transform.Find("Footer")?.GetComponent<RectTransform>();
            if (footerRect != null)
            {
                footerRect.anchorMin = Vector2.zero;
                footerRect.anchorMax = Vector2.zero;
                footerRect.pivot = new Vector2(0.5f, 0f);
                footerRect.sizeDelta = new Vector2(screenWidth, footerHeight);
                footerRect.anchoredPosition = new Vector2(screenWidth / 2, currentY);
            }
            currentY += footerHeight + bottomPadding;
            
            // QuickStart button (centered, 80% width)
            var quickStartRect = transform.Find("QuickStart")?.GetComponent<RectTransform>();
            if (quickStartRect != null)
            {
                quickStartRect.anchorMin = Vector2.zero;
                quickStartRect.anchorMax = Vector2.zero;
                quickStartRect.pivot = new Vector2(0.5f, 0f);
                quickStartRect.sizeDelta = new Vector2(screenWidth * 0.8f, buttonHeight);
                quickStartRect.anchoredPosition = new Vector2(screenWidth / 2, currentY);
            }
            currentY += buttonHeight + UnitHeight;
            
            // Servers selector (centered, 60% width)
            var serversRect = transform.Find("Servers")?.GetComponent<RectTransform>();
            if (serversRect != null)
            {
                serversRect.anchorMin = Vector2.zero;
                serversRect.anchorMax = Vector2.zero;
                serversRect.pivot = new Vector2(0.5f, 0.5f);
                serversRect.sizeDelta = new Vector2(screenWidth * 0.6f, serverHeight);
                serversRect.anchoredPosition = new Vector2(screenWidth / 2, currentY + serverHeight / 2);
            }
            
            var serversWheel = transform.Find("Servers")?.GetComponent<InfiniWheel>();
            if (serversWheel != null)
            {
                serversWheel.Select(serversWheel.SelectedItem);
            }
            currentY += serverHeight + topPadding;
            
            // Title (top)
            var titleRect = transform.Find("Title")?.GetComponent<RectTransform>();
            if (titleRect != null)
            {
                titleRect.anchorMin = Vector2.zero;
                titleRect.anchorMax = Vector2.zero;
                titleRect.pivot = new Vector2(0.5f, 0f);
                titleRect.sizeDelta = new Vector2(screenWidth, titleHeight);
                titleRect.anchoredPosition = new Vector2(screenWidth / 2, currentY);
            }
            
            // Author (if exists)
            var authorRect = transform.Find("Author")?.GetComponent<RectTransform>();
            if (authorRect != null)
            {
                authorRect.anchorMin = Vector2.zero;
                authorRect.anchorMax = Vector2.zero;
                authorRect.pivot = new Vector2(0.5f, 0.5f);
                authorRect.sizeDelta = new Vector2(UnitHeight * 0.5f, UnitHeight);
                authorRect.anchoredPosition = new Vector2(screenWidth * GoldenRatio, currentY + titleHeight / 2);
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
            float titleTime = 1.5f;
            
            var authorImage = transform.Find("Author").GetComponent<Image>();
            var titleText = transform.Find("Title").GetComponent<Text>();
            var titleTransform = transform.Find("Title").GetComponent<RectTransform>();
            
            authorImage.color = new Color(authorImage.color.r, authorImage.color.g, authorImage.color.b, 0);
            titleText.color = new Color(titleText.color.r, titleText.color.g, titleText.color.b, 0);
            
            Vector3 originalPos = titleTransform.anchoredPosition;
            Vector3 originalScale = titleTransform.localScale;
            
            titleTransform.anchoredPosition = originalPos + new Vector3(0, 200, 0);
            titleTransform.localScale = originalScale * 0.7f;
            
            authorImage.DOFade(1, titleTime * 0.6f).SetEase(Ease.OutQuad);
            titleText.DOFade(1, titleTime * 0.8f).SetEase(Ease.OutQuad);
            titleTransform.DOAnchorPos(originalPos, titleTime).SetEase(Ease.OutBack);
            titleTransform.DOScale(originalScale, titleTime).SetEase(Ease.OutBack);
            
            yield return new WaitForSeconds(titleTime);
            transform.Find("Block").gameObject.SetActive(true);
        }
        #endregion

        #region Server 
        private void OnServersValueChange(int index, Text text)
        {
            Game.Event.Instance.Fire(Event.ServerSelect, index);
        }
        #endregion

        #region Accounts
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