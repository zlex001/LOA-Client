using Framework;
using Game.Net;
using UnityEngine.UI;

namespace Game.Presentation
{
    using Net = Game.Net.Net;
    using Protocol = Game.Net.Protocol;

    public class OptionAmount : OptionItem
    {
        public Text titleText;
        public InputField inputField;
        public Button subtractButton, plusButton;
        public int amount;
        public int Amount
        {
            get
            {
                return amount;
            }
            set
            {
                if (value > 1)
                {
                    amount = value;
                }
                else
                {
                    amount = 1;
                }
                inputField.text = amount.ToString();
                Net.Instance.Send(new Protocol.OptionAmount(amount));
            }
        }
        void Start()
        {
            subtractButton.onClick.AddListener(OnSubtractButtonClick);
            plusButton.onClick.AddListener(OnPlusButtonClick);
            inputField.onEndEdit.AddListener(OnInputFieldEndEdit);

        }
        public void SetTitle(string text)
        {
            titleText.text = text;
        }
        public void SetAmount(int amount)
        {
            this.amount = amount;
            inputField.text = amount.ToString();
        }

        private void OnInputFieldEndEdit(string text)
        {
            Amount = int.Parse(text);
        }
        private void OnSubtractButtonClick()
        {
            Amount--;
        }
        private void OnPlusButtonClick()
        {
            Amount++;
        }

    }
}