using Framework;
using Game.Data;
using Game.Net;
using UnityEngine;
using UnityEngine.UI;

namespace Game.Presentation
{
    using Config = Game.Logic.Config;
    using Protocol = Game.Net.Protocol;

    public class OptionSlider : OptionItem
    {
        private const string ScreenAdaptationId = "ScreenAdaptation";

        public Text titleText;
        public Slider slider;
        public InputField inputField;
        public Button subtractButton, plusButton;

        private string[] labels;
        private string sliderId;
        private static readonly int[] FontSizes = { 25, 30, 35, 40 };

        void Start ()
        {
            slider.onValueChanged.AddListener(OnSliderValueChanged);
            subtractButton.onClick.AddListener(OnSubtractButtonClick);
            plusButton.onClick.AddListener(OnPlusButtonClick);
            inputField.onEndEdit.AddListener(OnInputFieldEndEdit);
        }

        private bool IsFontSizeMode => index == 1 && labels != null;
        private bool IsScreenAdaptation => sliderId == ScreenAdaptationId;

        public void SetId(string id)
        {
            sliderId = id;
        }

        private void OnInputFieldEndEdit(string text)
        {
            if (IsFontSizeMode) return;
            int value = int.Parse(text);
            slider.value = value;
            SendMessage();
        }
        private void OnSubtractButtonClick()
        {
            slider.value -= 1;
            SendMessage();
        }
        private void OnPlusButtonClick()
        {
            slider.value += 1;
            SendMessage();
        }
        private void OnSliderValueChanged(float value)
        {
            if (IsFontSizeMode)
            {
                int idx = Mathf.Clamp((int)value, 0, labels.Length - 1);
                inputField.text = labels[idx];
                if (idx < FontSizes.Length)
                {
                    Data.Instance.FontSize = FontSizes[idx];
                }
            }
            else
            {
                inputField.text = ((int)value).ToString();

                if (IsScreenAdaptation)
                {
                    PreviewScreenAdaptation(value);
                }
            }
        }

        private void PreviewScreenAdaptation(float value)
        {
            Data.Instance.User.ScreenUIAdaptation = value / 100f;
            var uiManager = Object.FindObjectOfType<UI>();
            uiManager?.ApplyScreenUIAdaptation();
        }
        public void OnPointerUp()
        {
            SendMessage();
        }
        public void SetText(string text)
        {
            titleText.text = text;
        }
        public void SetValueColor(string color)
        {
            //string hexColor = ColorUtils.colorDict[color];
            //UnityEngine.Color newColor = ColorUtils.HexToColor(hexColor);
            //ui_progressImg.color = newColor;
        }
        public void SetLabels(string labelsText)
        {
            labels = labelsText.Split('|');
        }
        public void SetSlider(int[] values)
        {
            slider.minValue = values[0];
            slider.maxValue = values[2];
            slider.wholeNumbers = IsFontSizeMode;
            
            int currentValue = values[1];
            if (IsFontSizeMode)
            {
                int localIdx = System.Array.IndexOf(FontSizes, Data.Instance.FontSize);
                if (localIdx >= 0) currentValue = localIdx;
                
                slider.gameObject.SetActive(false);
                inputField.interactable = false;
            }
            
            slider.value = Mathf.Clamp(currentValue, values[0], values[2]);
            
            if (IsFontSizeMode && labels != null)
            {
                inputField.text = labels[Mathf.Clamp(currentValue, 0, labels.Length - 1)];
            }
            else
            {
                inputField.text = slider.value.ToString();
            }
        }
        private void SendMessage()
        {
            NetManager.Instance.Send(new Protocol.OptionSlider(index, (int)slider.value));
        }
    }
}