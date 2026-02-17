using Framework;
using UnityEngine.UI;

namespace Game
{
    public class OptionTitleButton : OptionItem
    {
        private bool isListenerAdded = false;

        public void Refresh(int index, OptionItemData item)
        {
            this.index = index;
            
            if (!isListenerAdded)
            {
                var button = transform.Find("Button")?.GetComponent<Button>();
                if (button != null)
                {
                    button.onClick.AddListener(OnClick);
                    isListenerAdded = true;
                }
            }
            
            transform.Find("Button/Text").GetComponent<Text>().text = item.data["Button"];
            transform.Find("Title/Text").GetComponent<Text>().text = item.data["Title"];
        }
        
        private void OnClick()
        {
            Utils.ClickEffect.TextPress(transform.Find("Button/Text"), transform.Find("Button/Text").GetComponent<Text>());
            Net.Instance.Send(new Protocol.OptionButton(index, side));
        }
    }
}