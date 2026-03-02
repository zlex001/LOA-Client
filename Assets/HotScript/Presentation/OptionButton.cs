using Framework;
using Game.Data;
using Game.Net;
using UnityEngine.UI;

namespace Game.Presentation
{
    using Config = Game.Data.Config;
    using Protocol = Game.Net.Protocol;

    public class OptionButton : OptionItem
    {
        void Start()
        {
            GetComponent<Button>().onClick.AddListener(OnClick);
        }
        public void Refresh(int index, OptionItemData item)
        {
            this.index = index;
            transform.Find("Text").GetComponent<Text>().text = item.data["Text"];
        }
        private void OnClick()
        {
            Utils.ClickEffect.TextPress(transform.Find("Text"), transform.Find("Text").GetComponent<Text>());
            NetManager.Instance.Send(new Protocol.OptionButton(index, side));
        }
    }
}