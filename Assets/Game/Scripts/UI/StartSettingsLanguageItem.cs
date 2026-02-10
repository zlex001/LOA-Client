using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Game
{
    public class StartSettingsLanguageItem : MonoBehaviour
    {
        private Text _labelText;
        private Dropdown _dropdown;

        private void Awake()
        {
            _labelText = transform.Find("Label").GetComponent<Text>();
            _dropdown = transform.Find("Dropdown").GetComponent<Dropdown>();
        }

        public void Initialize(Data.Languages currentLanguage, Action<Data.Languages> onChangeCallback)
        {
            if (_labelText != null)
            {
                _labelText.text = Localization.Instance.Get("start_settings_language");
            }

            if (_dropdown != null)
            {
                // Populate dropdown with language options
                var languageOptions = new List<Dropdown.OptionData>();
                var languages = Enum.GetValues(typeof(Data.Languages)).Cast<Data.Languages>().ToList();
                
                foreach (var lang in languages)
                {
                    languageOptions.Add(new Dropdown.OptionData(lang.ToString()));
                }
                
                _dropdown.ClearOptions();
                _dropdown.AddOptions(languageOptions);
                _dropdown.value = (int)currentLanguage;
                _dropdown.RefreshShownValue();

                _dropdown.onValueChanged.AddListener((index) =>
                {
                    var selectedLanguage = (Data.Languages)index;
                    onChangeCallback?.Invoke(selectedLanguage);
                });
            }
        }

        private void OnDestroy()
        {
            if (_dropdown != null)
            {
                _dropdown.onValueChanged.RemoveAllListeners();
            }
        }
    }
}
