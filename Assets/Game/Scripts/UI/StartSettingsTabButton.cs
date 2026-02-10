using UnityEngine;
using UnityEngine.UI;
using System;

namespace Game
{
    public class StartSettingsTabButton : MonoBehaviour
    {
        private Button _button;
        private Text _text;
        private Image _background;
        private Action<int> _onClickCallback;
        private int _tabIndex;
        private bool _isSelected;

        private void Awake()
        {
            _button = GetComponent<Button>();
            _background = GetComponent<Image>();
            _text = transform.Find("Text").GetComponent<Text>();
            
            if (_button != null)
            {
                _button.onClick.AddListener(OnClick);
            }
        }

        public void Initialize(int tabIndex, string labelKey, Action<int> onClickCallback, bool isSelected = false)
        {
            _tabIndex = tabIndex;
            _onClickCallback = onClickCallback;
            _isSelected = isSelected;
            
            if (_text != null)
            {
                _text.text = Localization.Instance.Get(labelKey);
            }
            
            UpdateVisualState();
        }

        public void SetSelected(bool selected)
        {
            _isSelected = selected;
            UpdateVisualState();
        }

        private void UpdateVisualState()
        {
            if (_background != null)
            {
                _background.color = _isSelected 
                    ? new Color(0.78f, 0.78f, 0.78f, 1f)  // Selected: pressed color
                    : Color.white;  // Normal: white
            }
        }

        private void OnClick()
        {
            _onClickCallback?.Invoke(_tabIndex);
        }

        private void OnDestroy()
        {
            if (_button != null)
            {
                _button.onClick.RemoveListener(OnClick);
            }
        }
    }
}
