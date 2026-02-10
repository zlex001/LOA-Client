using UnityEngine;
using UnityEngine.UI;
using System;

namespace Game
{
    public class StartSettingsSoundItem : MonoBehaviour
    {
        private Text _labelText;
        private Toggle _toggle;
        private Text _statusText;

        private void Awake()
        {
            _labelText = transform.Find("Label").GetComponent<Text>();
            _toggle = transform.Find("Toggle").GetComponent<Toggle>();
            _statusText = transform.Find("StatusText").GetComponent<Text>();
        }

        public void Initialize(bool isEnabled, Action<bool> onChangeCallback)
        {
            if (_labelText != null)
            {
                _labelText.text = Localization.Instance.Get("start_settings_ui_sound");
            }

            if (_toggle != null)
            {
                _toggle.isOn = isEnabled;
                _toggle.onValueChanged.AddListener((value) =>
                {
                    UpdateStatusText(value);
                    onChangeCallback?.Invoke(value);
                });
            }

            UpdateStatusText(isEnabled);
        }

        private void UpdateStatusText(bool isOn)
        {
            if (_statusText != null)
            {
                _statusText.text = isOn 
                    ? Localization.Instance.Get("start_settings_sound_on")
                    : Localization.Instance.Get("start_settings_sound_off");
            }
        }

        private void OnDestroy()
        {
            if (_toggle != null)
            {
                _toggle.onValueChanged.RemoveAllListeners();
            }
        }
    }
}
