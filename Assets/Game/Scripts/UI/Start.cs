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

            // Initialize Components
            transform.Find("Servers").GetComponent<InfiniWheel>().Init(Data.Instance.Servers.Select(s => s.Name).ToArray());
            transform.Find("Servers").GetComponent<InfiniWheel>().Select(Data.Instance.User.SelectedServerIndex);
            transform.Find("Servers").GetComponent<InfiniWheel>().ValueChange += OnServersValueChange;
            transform.Find("Accounts").GetComponent<LoopListView2>().InitListView(Data.Instance.User.Accounts.Count, OnGetAccountByIndex);
            transform.Find("Accounts").GetComponent<LoopListView2>().MovePanelToItemIndex(Data.Instance.User.SelectedAccountIndex, transform.Find("Accounts").gameObject.GetComponent<RectTransform>().rect.height / 3f);

            transform.Find("Block/Text").GetComponent<Text>().DOFade(0, 1f).SetLoops(-1, LoopType.Yoyo);
            transform.Find("Block").GetComponent<Button>().onClick.AddListener(OnBlockClick);

            transform.Find("Accounts/Delete").GetComponent<Button>().onClick.AddListener(OnAccountsDeleteClick);
            transform.Find("Accounts/Add").GetComponent<Button>().onClick.AddListener(OnAccountsAddClick);

            transform.Find("Login").GetComponent<Button>().onClick.AddListener(OnLoginClick);
            
            // Setup QuickStart button if exists
            var quickStartButton = transform.Find("QuickStart")?.GetComponent<Button>();
            if (quickStartButton != null)
            {
                quickStartButton.onClick.AddListener(OnQuickStartClick);
                Utils.Debug.Log("Start", "QuickStart button listener registered");
            }
            
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
            var accountsListView = transform.Find("Accounts")?.GetComponent<LoopListView2>();
            if (accountsListView != null)
            {
                accountsListView.RefreshAllShownItem();
            }
        }

        private void ApplyAbsoluteLayout()
        {
            float screenWidth = GetComponent<RectTransform>().rect.width;
            float screenHeight = GetComponent<RectTransform>().rect.height;

            var footerRect = transform.Find("Footer")?.GetComponent<RectTransform>();
            if (footerRect != null)
            {
                footerRect.anchorMin = Vector2.zero;
                footerRect.anchorMax = Vector2.zero;
                footerRect.pivot = new Vector2(0.5f, 0f);
                footerRect.sizeDelta = new Vector2(screenWidth, UnitHeight);
                footerRect.anchoredPosition = new Vector2(screenWidth / 2, 0);
            }

            float listWidth = screenWidth * (1 - GoldenRatio);
            float serversHeight = UnitHeight*1.25f;
            float accountsHeight = UnitHeight * 8;
            float loginHeight = UnitHeight;
            float contentHeight = serversHeight + accountsHeight;
            float remainingSpace = screenHeight - contentHeight;
            float bottomMargin = remainingSpace * (1 - GoldenRatio);
            float topMargin = remainingSpace * GoldenRatio;

            float accountsBottomY = bottomMargin;
            float accountsTopY = accountsBottomY + accountsHeight;
            float serversBottomY = accountsTopY;
            float serversTopY = serversBottomY + serversHeight;

            var accountsRect = transform.Find("Accounts")?.GetComponent<RectTransform>();
            if (accountsRect != null)
            {
                accountsRect.anchorMin = Vector2.zero;
                accountsRect.anchorMax = Vector2.zero;
                accountsRect.pivot = new Vector2(0.5f, 0f);
                accountsRect.sizeDelta = new Vector2(listWidth, accountsHeight);
                accountsRect.anchoredPosition = new Vector2(screenWidth / 2, accountsBottomY);
            }

            var serversRect = transform.Find("Servers")?.GetComponent<RectTransform>();
            if (serversRect != null)
            {
                serversRect.anchorMin = Vector2.zero;
                serversRect.anchorMax = Vector2.zero;
                serversRect.pivot = new Vector2(0.5f, 0.5f);
                serversRect.sizeDelta = new Vector2(listWidth, serversHeight);
                serversRect.anchoredPosition = new Vector2(screenWidth / 2, serversBottomY + serversHeight / 2);
            }

            var serversWheel = transform.Find("Servers")?.GetComponent<InfiniWheel>();
            if (serversWheel != null)
            {
                serversWheel.Select(serversWheel.SelectedItem);
            }

            // Handle Login and QuickStart button layout
            var loginRect = transform.Find("Login")?.GetComponent<RectTransform>();
            var quickStartRect = transform.Find("QuickStart")?.GetComponent<RectTransform>();
            
            if (quickStartRect != null && loginRect != null)
            {
                // Both buttons exist: vertical stack layout
                float buttonHeight = UnitHeight;
                float spacing = UnitHeight * 0.2f;
                float totalButtonHeight = buttonHeight * 2 + spacing;
                float buttonCenterY = accountsBottomY * GoldenRatio;
                float topButtonY = buttonCenterY + totalButtonHeight / 2 - buttonHeight / 2;
                float bottomButtonY = buttonCenterY - totalButtonHeight / 2 + buttonHeight / 2;
                
                // QuickStart on top (primary action)
                quickStartRect.anchorMin = Vector2.zero;
                quickStartRect.anchorMax = Vector2.zero;
                quickStartRect.pivot = new Vector2(0.5f, 0.5f);
                quickStartRect.sizeDelta = new Vector2(listWidth, buttonHeight);
                quickStartRect.anchoredPosition = new Vector2(screenWidth / 2, topButtonY);
                
                // Login on bottom (secondary action)
                loginRect.anchorMin = Vector2.zero;
                loginRect.anchorMax = Vector2.zero;
                loginRect.pivot = new Vector2(0.5f, 0.5f);
                loginRect.sizeDelta = new Vector2(listWidth, buttonHeight);
                loginRect.anchoredPosition = new Vector2(screenWidth / 2, bottomButtonY);
            }
            else if (loginRect != null)
            {
                // Only Login button: centered
                loginRect.anchorMin = Vector2.zero;
                loginRect.anchorMax = Vector2.zero;
                loginRect.pivot = new Vector2(0.5f, 0.5f);
                loginRect.sizeDelta = new Vector2(listWidth, loginHeight);
                loginRect.anchoredPosition = new Vector2(screenWidth / 2, accountsBottomY * GoldenRatio);
            }

            var titleRect = transform.Find("Title")?.GetComponent<RectTransform>();
            if (titleRect != null)
            {
                titleRect.anchorMin = Vector2.zero;
                titleRect.anchorMax = Vector2.zero;
                titleRect.pivot = new Vector2(0.5f, 0.5f);
                titleRect.anchoredPosition = new Vector2(screenWidth / 2, (serversTopY + screenHeight) / 2);
            }

            var authorRect = transform.Find("Author")?.GetComponent<RectTransform>();
            if (authorRect != null)
            {
                authorRect.anchorMin = Vector2.zero;
                authorRect.anchorMax = Vector2.zero;
                authorRect.pivot = new Vector2(0.5f, 0.5f);
                authorRect.sizeDelta = new Vector2(UnitHeight * 0.5f, UnitHeight);
                authorRect.anchoredPosition = new Vector2(screenWidth * GoldenRatio, (serversTopY + screenHeight) / 2);
            }
        }
        #endregion

        #region Animation
        private void OnBlockClick()
        {
            if (Data.Instance.User.Accounts.Count == 0)
            {
                Close();
                UI.Instance.Open(Config.UI.Account, _uiTexts, false);
            }
            else
            {
                transform.Find("Block").gameObject.SetActive(false);
                transform.Find("Login").gameObject.SetActive(true);
                transform.Find("Servers").gameObject.SetActive(true);
                transform.Find("Accounts").gameObject.SetActive(true);
            }
        }

        private System.Collections.IEnumerator Animate()
        {
            float titleTime = 1.2f;
            
            var authorImage = transform.Find("Author").GetComponent<Image>();
            var titleText = transform.Find("Title").GetComponent<Text>();
            var titleTransform = transform.Find("Title").GetComponent<RectTransform>();
            
            authorImage.color = new Color(authorImage.color.r, authorImage.color.g, authorImage.color.b, 0);
            titleText.color = new Color(titleText.color.r, titleText.color.g, titleText.color.b, 0);
            
            Vector3 originalPos = titleTransform.anchoredPosition;
            titleTransform.anchoredPosition = originalPos + new Vector3(0, 100, 0);
            
            authorImage.DOFade(1, titleTime).SetEase(Ease.OutQuad);
            titleText.DOFade(1, titleTime).SetEase(Ease.OutQuad);
            titleTransform.DOAnchorPos(originalPos, titleTime).SetEase(Ease.OutCubic);
            
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
        private void OnAccountsDeleteClick()
        {
            Data.Instance.User.Accounts.RemoveAt(Data.Instance.User.SelectedAccountIndex);
            if (Data.Instance.User.SelectedAccountIndex >= Data.Instance.User.Accounts.Count)
            {
                Data.Instance.User.SelectedAccountIndex = Data.Instance.User.Accounts.Count - 1;
            }
            Local.Instance.Save(Data.Instance.User);
            transform.Find("Accounts").GetComponent<LoopListView2>().SetListItemCount(Data.Instance.User.Accounts.Count);
            transform.Find("Accounts").GetComponent<LoopListView2>().RefreshAllShownItem();
            transform.Find("Accounts").GetComponent<LoopListView2>().MovePanelToItemIndex(Data.Instance.User.SelectedAccountIndex, transform.Find("Accounts").gameObject.GetComponent<RectTransform>().rect.height / 3f);

            if (Data.Instance.User.Accounts.Count == 0)
            {
                Close();
                UI.Instance.Open(Config.UI.Account, _uiTexts, false);
            }
        }

        private void OnAccountsAddClick()
        {
            Close();
            UI.Instance.Open(Config.UI.Account, _uiTexts, true);
        }


        private LoopListViewItem2 OnGetAccountByIndex(LoopListView2 view, int index)
        {
            if (index < 0 || index >= Data.Instance.User.Accounts.Count) { return null; }
            LoopListViewItem2 item = view.NewListViewItem("Item");
            item.GetComponent<StartAccount>().Refresh(index);
            item.gameObject.GetComponent<RectTransform>().sizeDelta = new Vector2(0, UnitHeight);
            return item;
        }

        #endregion

        #region Login
        private void OnLoginClick()
        {
            Utils.Debug.Log("Start", $"Login button clicked. Selected server: {Data.Instance.SelectedServer.Name} ({Data.Instance.SelectedServer.Ip}:{Data.Instance.SelectedServer.Port})");
            Utils.Debug.Log("Start", $"Selected account: {Data.Instance.SelectedAccount.Id}");
            Data.Instance.LoginAccount = Data.Instance.SelectedAccount;
        }

        private void OnQuickStartClick()
        {
            Utils.Debug.Log("Start", "QuickStart button clicked");
            Utils.Debug.Log("Start", $"Selected server: {Data.Instance.SelectedServer.Name} ({Data.Instance.SelectedServer.Ip}:{Data.Instance.SelectedServer.Port})");
            
            // Show loading screen
            UI.Instance.Open(Config.UI.Dark, Localization.Instance.Get("connecting"));
            
            // Set QuickStart login mode (special identifier to differentiate from traditional login)
            Data.Instance.LoginAccount = new User.Account 
            { 
                Id = "__QuickStart__",
                Password = ""
            };
        }

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