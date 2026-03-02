using Framework;
using Game.Data;
using Game.Net;
using UnityEngine.UI;

namespace Game.Presentation
{
    using Config = Game.Data.Config;
    using Protocol = Game.Net.Protocol;

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
            NetManager.Instance.Send(new Protocol.OptionButton(index, side));
        }
    }
}