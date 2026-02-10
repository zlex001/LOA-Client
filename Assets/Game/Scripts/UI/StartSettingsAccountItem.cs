using UnityEngine;
using UnityEngine.UI;
using System;

namespace Game
{
    public class StartSettingsAccountItem : MonoBehaviour
    {
        private Button _mainButton;
        private Button _editButton;
        private Button _deleteButton;
        private Text _idText;
        private Text _noteText;
        private Image _background;
        
        private Account _account;
        private Action<Account> _onSwitchCallback;
        private Action<Account> _onEditCallback;
        private Action<Account> _onDeleteCallback;

        private void Awake()
        {
            _mainButton = GetComponent<Button>();
            _background = GetComponent<Image>();
            _idText = transform.Find("IdText").GetComponent<Text>();
            _noteText = transform.Find("NoteText").GetComponent<Text>();
            _editButton = transform.Find("EditButton").GetComponent<Button>();
            _deleteButton = transform.Find("DeleteButton").GetComponent<Button>();
        }

        public void Initialize(
            Account account, 
            bool isCurrentAccount,
            Action<Account> onSwitchCallback, 
            Action<Account> onEditCallback, 
            Action<Account> onDeleteCallback)
        {
            _account = account;
            _onSwitchCallback = onSwitchCallback;
            _onEditCallback = onEditCallback;
            _onDeleteCallback = onDeleteCallback;

            RefreshDisplay(isCurrentAccount);

            if (_mainButton != null)
            {
                _mainButton.onClick.AddListener(OnSwitchClick);
            }
            if (_editButton != null)
            {
                _editButton.onClick.AddListener(OnEditClick);
            }
            if (_deleteButton != null)
            {
                _deleteButton.onClick.AddListener(OnDeleteClick);
            }
        }

        public void RefreshDisplay(bool isCurrentAccount)
        {
            if (_idText != null)
            {
                _idText.text = _account.Id;
            }
            if (_noteText != null)
            {
                _noteText.text = string.IsNullOrEmpty(_account.Note) ? "" : $"({_account.Note})";
            }
            if (_background != null)
            {
                _background.color = isCurrentAccount 
                    ? new Color(0.78f, 0.78f, 0.78f, 1f)  // Selected
                    : Color.white;  // Normal
            }
        }

        private void OnSwitchClick()
        {
            _onSwitchCallback?.Invoke(_account);
        }

        private void OnEditClick()
        {
            _onEditCallback?.Invoke(_account);
        }

        private void OnDeleteClick()
        {
            _onDeleteCallback?.Invoke(_account);
        }

        private void OnDestroy()
        {
            if (_mainButton != null) _mainButton.onClick.RemoveListener(OnSwitchClick);
            if (_editButton != null) _editButton.onClick.RemoveListener(OnEditClick);
            if (_deleteButton != null) _deleteButton.onClick.RemoveListener(OnDeleteClick);
        }
    }
}
