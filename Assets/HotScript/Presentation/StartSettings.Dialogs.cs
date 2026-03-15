using System;
using UnityEngine;
using UnityEngine.UI;

namespace Game.Presentation
{
    public partial class StartSettings
    {
        #region Dialog Helpers

        private void ShowAddAccountDialog(Action<string, string, string> onConfirm)
        {
            var dialogObj = new GameObject("AddAccountDialog");
            dialogObj.transform.SetParent(transform, false);

            var dialogRect = dialogObj.AddComponent<RectTransform>();
            dialogRect.anchorMin = Vector2.zero;
            dialogRect.anchorMax = Vector2.one;
            dialogRect.sizeDelta = Vector2.zero;

            var mask = dialogObj.AddComponent<Image>();
            mask.color = ColorDialogMask;

            var maskButton = dialogObj.AddComponent<Button>();
            maskButton.targetGraphic = mask;
            maskButton.onClick.AddListener(() => GameObject.Destroy(dialogObj));

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

            var accountField = CreateInputField(panelObj.transform, "AccountField",
                new Vector2(0.1f, 0.65f), new Vector2(0.9f, 0.8f),
                GetText("account_id"), "");

            var passwordField = CreateInputField(panelObj.transform, "PasswordField",
                new Vector2(0.1f, 0.48f), new Vector2(0.9f, 0.63f),
                GetText("password"), "");
            passwordField.inputType = InputField.InputType.Password;

            var noteField = CreateInputField(panelObj.transform, "NoteField",
                new Vector2(0.1f, 0.31f), new Vector2(0.9f, 0.46f),
                GetText("note_optional"), "");

            CreateDialogButton(panelObj.transform, "Cancel", new Vector2(0.15f, 0.1f), new Vector2(0.45f, 0.25f),
                GetText("cancel"),
                () => GameObject.Destroy(dialogObj));

            CreateDialogButton(panelObj.transform, "Confirm", new Vector2(0.55f, 0.1f), new Vector2(0.85f, 0.25f),
                GetText("confirm"),
                () => {
                    onConfirm?.Invoke(accountField.text, passwordField.text, noteField.text);
                    GameObject.Destroy(dialogObj);
                });
        }

        private void ShowEditAccountDialog(string defaultId, string defaultPassword, string defaultNote, Action<string, string, string> onConfirm)
        {
            var dialogObj = new GameObject("EditAccountDialog");
            dialogObj.transform.SetParent(transform, false);

            var dialogRect = dialogObj.AddComponent<RectTransform>();
            dialogRect.anchorMin = Vector2.zero;
            dialogRect.anchorMax = Vector2.one;
            dialogRect.sizeDelta = Vector2.zero;

            var mask = dialogObj.AddComponent<Image>();
            mask.color = ColorDialogMask;

            var maskButton = dialogObj.AddComponent<Button>();
            maskButton.targetGraphic = mask;
            maskButton.onClick.AddListener(() => GameObject.Destroy(dialogObj));

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

            var idField = CreateInputField(panelObj.transform, "AccountIdField",
                new Vector2(0.1f, 0.63f), new Vector2(0.9f, 0.78f),
                GetText("account_id"), defaultId);

            var passwordField = CreateInputField(panelObj.transform, "PasswordField",
                new Vector2(0.1f, 0.45f), new Vector2(0.9f, 0.6f),
                GetText("password"), defaultPassword);
            passwordField.inputType = InputField.InputType.Password;

            var noteField = CreateInputField(panelObj.transform, "NoteField",
                new Vector2(0.1f, 0.27f), new Vector2(0.9f, 0.42f),
                GetText("note_optional"), defaultNote);

            CreateDialogButton(panelObj.transform, "Cancel", new Vector2(0.15f, 0.1f), new Vector2(0.45f, 0.25f),
                GetText("cancel"),
                () => GameObject.Destroy(dialogObj));

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
            var dialogObj = new GameObject("ConfirmDialog");
            dialogObj.transform.SetParent(transform, false);
            dialogObj.transform.SetAsLastSibling();

            var dialogRect = dialogObj.AddComponent<RectTransform>();
            dialogRect.anchorMin = Vector2.zero;
            dialogRect.anchorMax = Vector2.one;
            dialogRect.sizeDelta = Vector2.zero;

            var mask = dialogObj.AddComponent<Image>();
            mask.color = ColorDialogMask;

            var maskButton = dialogObj.AddComponent<Button>();
            maskButton.targetGraphic = mask;
            maskButton.onClick.AddListener(() => GameObject.Destroy(dialogObj));

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

            CreateDialogButton(panelObj.transform, "Cancel", new Vector2(0.15f, 0.15f), new Vector2(0.45f, 0.35f),
                GetText("cancel"),
                () => GameObject.Destroy(dialogObj));

            CreateDialogButton(panelObj.transform, "Confirm", new Vector2(0.55f, 0.15f), new Vector2(0.85f, 0.35f),
                GetText("confirm"),
                () => {
                    onConfirm?.Invoke();
                    GameObject.Destroy(dialogObj);
                });
        }

        private void ShowInputDialog(string title, string defaultValue, Action<string> onConfirm)
        {
            var dialogObj = new GameObject("InputDialog");
            dialogObj.transform.SetParent(transform, false);

            var dialogRect = dialogObj.AddComponent<RectTransform>();
            dialogRect.anchorMin = Vector2.zero;
            dialogRect.anchorMax = Vector2.one;
            dialogRect.sizeDelta = Vector2.zero;

            var mask = dialogObj.AddComponent<Image>();
            mask.color = ColorDialogMask;

            var maskButton = dialogObj.AddComponent<Button>();
            maskButton.targetGraphic = mask;
            maskButton.onClick.AddListener(() => GameObject.Destroy(dialogObj));

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

            var inputField = CreateInputField(panelObj.transform, "InputField",
                new Vector2(0.1f, 0.45f), new Vector2(0.9f, 0.65f),
                title, defaultValue);

            CreateDialogButton(panelObj.transform, "Cancel", new Vector2(0.15f, 0.1f), new Vector2(0.45f, 0.3f),
                GetText("cancel"),
                () => GameObject.Destroy(dialogObj));

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
    }
}
