using Game.Data;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

namespace Game.Presentation
{
    public class AccountUI : UI.Core
    {
        private const float UnitHeight = 83f;
        private const float GoldenRatio = 0.618f;

        public override void OnCreate(params object[] args)
        {
            transform.Find("Id").GetComponent<InputField>().onEndEdit.AddListener(OnIdEndEdit);
            transform.Find("Password").GetComponent<InputField>().onEndEdit.AddListener(OnPasswordEndEdit);
            transform.Find("Buttons/Confirm").GetComponent<Button>().onClick.AddListener(OnConfirmClick);
            transform.Find("Buttons/Cancel").GetComponent<Button>().onClick.AddListener(OnCancelClick);
        }

        public override void OnEnter(params object[] args)
        {
            if (args.Length > 0)
            {
                var texts = args[0] as Dictionary<string, string>;
                if (texts != null)
                {
                    DataManager.Instance.AccountTexts = texts;
                }
            }

            RefreshUI();

            if (args.Length > 1)
            {
                bool showCancel = (bool)args[1];
                transform.Find("Buttons/Cancel").gameObject.SetActive(showCancel);
            }

            transform.Find("Id").GetComponent<InputField>().text = "";
            transform.Find("Password").GetComponent<InputField>().text = "";
            transform.Find("Note").GetComponent<InputField>().text = "";
            transform.Find("Check").GetComponent<Text>().text = "";

            ApplyAbsoluteLayout();
        }
        
        private void RefreshUI()
        {
            var texts = DataManager.Instance.AccountTexts;
            if (texts == null) return;
            
            var idPlaceholder = transform.Find("Id/Placeholder")?.GetComponent<Text>();
            if (idPlaceholder != null && texts.ContainsKey("accountIdPlaceholder"))
                idPlaceholder.text = texts["accountIdPlaceholder"];
            
            var passwordPlaceholder = transform.Find("Password/Placeholder")?.GetComponent<Text>();
            if (passwordPlaceholder != null && texts.ContainsKey("accountPasswordPlaceholder"))
                passwordPlaceholder.text = texts["accountPasswordPlaceholder"];
            
            var notePlaceholder = transform.Find("Note/Placeholder")?.GetComponent<Text>();
            if (notePlaceholder != null && texts.ContainsKey("accountNotePlaceholder"))
                notePlaceholder.text = texts["accountNotePlaceholder"];
        }

        public override void OnScreenAdaptationChanged()
        {
            ApplyAbsoluteLayout();
        }

        private void ApplyAbsoluteLayout()
        {
            float screenWidth = GetComponent<RectTransform>().rect.width;
            float screenHeight = GetComponent<RectTransform>().rect.height;

            float panelWidth = screenWidth * (1 - GoldenRatio);
            float inputHeight = UnitHeight;
            float buttonHeight = UnitHeight;
            float spacing = UnitHeight * 0.25f;
            float totalHeight = inputHeight * 4 + buttonHeight + spacing * 4;
            float baseY = (screenHeight - totalHeight) / 2;

            var idRect = transform.Find("Id")?.GetComponent<RectTransform>();
            if (idRect != null)
            {
                idRect.anchorMin = Vector2.zero;
                idRect.anchorMax = Vector2.zero;
                idRect.pivot = new Vector2(0.5f, 0f);
                idRect.sizeDelta = new Vector2(panelWidth, inputHeight);
                idRect.anchoredPosition = new Vector2(screenWidth / 2, baseY + buttonHeight + spacing + inputHeight * 3 + spacing * 3);
            }

            var passwordRect = transform.Find("Password")?.GetComponent<RectTransform>();
            if (passwordRect != null)
            {
                passwordRect.anchorMin = Vector2.zero;
                passwordRect.anchorMax = Vector2.zero;
                passwordRect.pivot = new Vector2(0.5f, 0f);
                passwordRect.sizeDelta = new Vector2(panelWidth, inputHeight);
                passwordRect.anchoredPosition = new Vector2(screenWidth / 2, baseY + buttonHeight + spacing + inputHeight * 2 + spacing * 2);
            }

            var noteRect = transform.Find("Note")?.GetComponent<RectTransform>();
            if (noteRect != null)
            {
                noteRect.anchorMin = Vector2.zero;
                noteRect.anchorMax = Vector2.zero;
                noteRect.pivot = new Vector2(0.5f, 0f);
                noteRect.sizeDelta = new Vector2(panelWidth, inputHeight);
                noteRect.anchoredPosition = new Vector2(screenWidth / 2, baseY + buttonHeight + spacing + inputHeight + spacing);
            }

            var checkRect = transform.Find("Check")?.GetComponent<RectTransform>();
            if (checkRect != null)
            {
                checkRect.anchorMin = Vector2.zero;
                checkRect.anchorMax = Vector2.zero;
                checkRect.pivot = new Vector2(0.5f, 0f);
                checkRect.sizeDelta = new Vector2(panelWidth, inputHeight);
                checkRect.anchoredPosition = new Vector2(screenWidth / 2, baseY + buttonHeight + spacing);
            }

            var buttonsRect = transform.Find("Buttons")?.GetComponent<RectTransform>();
            if (buttonsRect != null)
            {
                buttonsRect.anchorMin = Vector2.zero;
                buttonsRect.anchorMax = Vector2.zero;
                buttonsRect.pivot = new Vector2(0.5f, 0f);
                buttonsRect.sizeDelta = new Vector2(panelWidth, buttonHeight);
                buttonsRect.anchoredPosition = new Vector2(screenWidth / 2, baseY);
            }
        }

        private string InputCheck
        {
            get
            {
                string id = transform.Find("Id").GetComponent<InputField>().text;
                string password = transform.Find("Password").GetComponent<InputField>().text;
                var texts = DataManager.Instance.AccountTexts;
                
                if (string.IsNullOrEmpty(id))
                    return texts != null && texts.ContainsKey("errorAccountEmpty") ? texts["errorAccountEmpty"] : "";
                
                if (!System.Text.RegularExpressions.Regex.IsMatch(id, @"^[a-zA-Z0-9]{6,20}$"))
                    return texts != null && texts.ContainsKey("errorAccountFormat") ? texts["errorAccountFormat"] : "";
                
                if (string.IsNullOrEmpty(password))
                    return texts != null && texts.ContainsKey("errorPasswordEmpty") ? texts["errorPasswordEmpty"] : "";
                
                if (!System.Text.RegularExpressions.Regex.IsMatch(password, @"^[a-zA-Z0-9]{6,20}$"))
                    return texts != null && texts.ContainsKey("errorPasswordFormat") ? texts["errorPasswordFormat"] : "";
                
                return "";
            }
        }

        private void OnIdEndEdit(string value)
        {
            transform.Find("Check").GetComponent<Text>().text = InputCheck;
        }

        private void OnPasswordEndEdit(string value)
        {
            transform.Find("Check").GetComponent<Text>().text = InputCheck;
        }

        private void OnConfirmClick()
        {
            if (InputCheck == "")
            {
                string id = transform.Find("Id").GetComponent<InputField>().text;
                string password = transform.Find("Password").GetComponent<InputField>().text;
                string note = transform.Find("Note").GetComponent<InputField>().text;
                Authentication.Login(new Account { Id = id, Password = password, Note = note });
                Close();
            }
            else
            {
                transform.Find("Check").GetComponent<Text>().text = InputCheck;
            }
        }

        private void OnCancelClick()
        {
            Close();
            UI.Instance.Open(Config.UI.Start, DataManager.Instance.StartTexts);
        }
    }
}

