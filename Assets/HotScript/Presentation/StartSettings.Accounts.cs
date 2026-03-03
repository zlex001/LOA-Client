using Game.Data;
using Game.Net;
using UnityEngine;
using UnityEngine.UI;

namespace Game.Presentation
{
    public partial class StartSettings
    {
        #region Account UI Building

        private void BuildAccountSection()
        {
            CreateSectionHeader(GetText("accounts"));

            _accountItemObjects.Clear();
            for (int i = 0; i < _accounts.Count; i++)
            {
                var account = _accounts[i];
                var itemObj = CreateAccountItem(account, i == _selectedIndex, i);
                _accountItemObjects.Add(itemObj);
            }

            CreateAddAccountButton();
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
            text.fontSize = 38;
            text.color = isSelected ? ColorTextAccent : ColorTextPrimary;
            text.alignment = TextAnchor.MiddleLeft;
            textObj.AddComponent<Framework.FontScaler>();

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

        #endregion

        #region Account Callbacks

        private void OnAccountSwitch(Account account)
        {
            int newIndex = _accounts.FindIndex(a => a.Id == account.Id);
            if (newIndex < 0) return;

            _selectedIndex = newIndex;
            Close();
            NetManager.Instance.SwitchAccount(account, newIndex);
        }

        private void OnAccountEdit(Account account)
        {
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
                Local.Instance.Save(DataManager.Instance.User);
                BuildUI();
            });
        }

        private void OnAccountDelete(Account account)
        {
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
                DataManager.Instance.User.Accounts.RemoveAt(index);

                if (_selectedIndex == index)
                {
                    _selectedIndex = 0;
                    DataManager.Instance.User.SelectedAccountIndex = 0;
                }
                else if (_selectedIndex > index)
                {
                    _selectedIndex--;
                    DataManager.Instance.User.SelectedAccountIndex = _selectedIndex;
                }

                Local.Instance.Save(DataManager.Instance.User);
                BuildUI();
            });
        }

        private void OnAddAccountClick()
        {
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
                DataManager.Instance.User.Accounts.Add(newAccount);
                Local.Instance.Save(DataManager.Instance.User);
                BuildUI();
            });
        }

        #endregion
    }
}
