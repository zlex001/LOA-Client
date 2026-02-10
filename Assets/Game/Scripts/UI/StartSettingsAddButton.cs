using UnityEngine;
using UnityEngine.UI;
using System;

namespace Game
{
    public class StartSettingsAddButton : MonoBehaviour
    {
        private Button _button;
        private Text _text;

        private void Awake()
        {
            _button = GetComponent<Button>();
            _text = transform.Find("Text").GetComponent<Text>();
        }

        public void Initialize(Action onClickCallback)
        {
            if (_text != null)
            {
                _text.text = Localization.Instance.Get("start_settings_add_account");
            }
            
            if (_button != null)
            {
                _button.onClick.AddListener(() => onClickCallback?.Invoke());
            }
        }

        private void OnDestroy()
        {
            if (_button != null)
            {
                _button.onClick.RemoveAllListeners();
            }
        }
    }
}
