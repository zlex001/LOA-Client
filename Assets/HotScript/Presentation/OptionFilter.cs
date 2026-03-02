using Framework;
using Game.Data;
using Game.Net;
using UnityEngine;
using UnityEngine.UI;

namespace Game.Presentation
{
    using Config = Game.Data.Config;
    using Protocol = Game.Net.Protocol;

    public class OptionFilter : OptionItem
    {
        void Start()
        {
            GetComponent<InputField>().onEndEdit.AddListener((text) => NetManager.Instance.Send(new Protocol.OptionFilter(index,text)));
        }
        public void Refresh(int index, OptionItemData item)
        {
            this.index = index;
            transform.Find("Text").GetComponent<Text>().text = item.data["Text"];
            transform.Find("Placeholder").GetComponent<Text>().text = item.data["PlaceholderText"];
        }

    }
}