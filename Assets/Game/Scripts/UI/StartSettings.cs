using Framework;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq;

namespace Game
{
    public class StartSettings : UI.Core
    {
        private const float UnitHeight = 83f;
        private const float GoldenRatio = 0.618f;
        private const float PanelHeightUnits = 10f;

        private List<Account> _accounts;
        private int _selectedIndex;

        #region Lifecycle Methods
        public override void OnCreate(params object[] args)
        {
            ApplyLayout();
            
            // Close button
            var closeButton = transform.Find("Panel/Close")?.GetComponent<Button>();
            if (closeButton != null)
            {
                closeButton.onClick.AddListener(OnCloseClick);
            }

            // Load account data
            _accounts = Data.Instance.User.Accounts.ToList();
            _selectedIndex = Data.Instance.User.SelectedAccountIndex;

            // Build account list
            BuildAccountList();

            Utils.Debug.Log("StartSettings", $"Loaded {_accounts.Count} accounts, selected index: {_selectedIndex}");
        }

        public override void OnScreenAdaptationChanged()
        {
            ApplyLayout();
        }

        private void ApplyLayout()
        {
            float screenWidth = GetComponent<RectTransform>().rect.width;
            float screenHeight = GetComponent<RectTransform>().rect.height;
            
            // Panel size and position
            float panelWidth = screenWidth * GoldenRatio;
            float panelHeight = UnitHeight * PanelHeightUnits;
            
            var panelRect = transform.Find("Panel")?.GetComponent<RectTransform>();
            if (panelRect != null)
            {
                panelRect.anchorMin = new Vector2(0.5f, 0.5f);
                panelRect.anchorMax = new Vector2(0.5f, 0.5f);
                panelRect.pivot = new Vector2(0.5f, 0.5f);
                panelRect.sizeDelta = new Vector2(panelWidth, panelHeight);
                panelRect.anchoredPosition = Vector2.zero;
            }

            Utils.Debug.Log("StartSettings", $"Panel layout: size=({panelWidth}, {panelHeight})");
        }
        #endregion

        #region Account List
        private void BuildAccountList()
        {
            var accountContainer = transform.Find("Panel/AccountList/Viewport/Content");
            if (accountContainer == null)
            {
                Utils.Debug.LogWarning("StartSettings", "Account container not found");
                return;
            }

            // Clear existing items
            foreach (Transform child in accountContainer)
            {
                Destroy(child.gameObject);
            }

            // Create account items
            for (int i = 0; i < _accounts.Count; i++)
            {
                CreateAccountItem(accountContainer, _accounts[i], i);
            }

            Utils.Debug.Log("StartSettings", $"Built account list with {_accounts.Count} items");
        }

        private void CreateAccountItem(Transform parent, Account account, int index)
        {
            // Create item GameObject
            GameObject itemObj = new GameObject($"Account_{index}");
            itemObj.transform.SetParent(parent, false);

            // Add RectTransform
            var rect = itemObj.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(0, 1);
            rect.anchorMax = new Vector2(1, 1);
            rect.pivot = new Vector2(0.5f, 1);
            rect.sizeDelta = new Vector2(0, UnitHeight);
            rect.anchoredPosition = new Vector2(0, -index * UnitHeight);

            // Add background Image
            var bg = itemObj.AddComponent<Image>();
            bg.color = (index == _selectedIndex) ? 
                new Color(0.3f, 0.6f, 1f, 0.3f) :  // Selected: light blue
                new Color(0.2f, 0.2f, 0.2f, 0.5f);  // Normal: dark gray

            // Add text for account info
            GameObject textObj = new GameObject("Text");
            textObj.transform.SetParent(itemObj.transform, false);
            
            var textRect = textObj.AddComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.sizeDelta = Vector2.zero;
            textRect.anchoredPosition = Vector2.zero;

            var text = textObj.AddComponent<Text>();
            text.text = $"{account.Id}\n{account.Note}";
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.fontSize = 24;
            text.color = Color.white;
            text.alignment = TextAnchor.MiddleCenter;

            // Add button for click interaction
            var button = itemObj.AddComponent<Button>();
            int capturedIndex = index;  // Capture for lambda
            button.onClick.AddListener(() => OnAccountClick(capturedIndex));

            Utils.Debug.Log("StartSettings", $"Created account item: {account.Id} (index {index})");
        }
        #endregion

        #region Event Handlers
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

        private void OnCloseClick()
        {
            Utils.Debug.Log("StartSettings", "Close button clicked");
            Close();
        }
        #endregion
    }
}
