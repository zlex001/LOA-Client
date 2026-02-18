using Framework;
using Game.Logic;
using Game.Net;
using UnityEngine;
using UnityEngine.UI;

namespace Game.Presentation
{
    using Net = Game.Net.Net;
    using Config = Game.Logic.Config;
    using Protocol = Game.Net.Protocol;

    public class OptionInput : OptionItem
    {
        void Start()
        {
            GetComponent<InputField>().onEndEdit.AddListener((text) => Net.Instance.Send(new Protocol.OptionInput(text)));
        }
        public void Refresh(int index, OptionItemData item)
        {
            this.index = index;
            transform.Find("Text").GetComponent<Text>().text = item.data["Text"];
            transform.Find("Placeholder").GetComponent<Text>().text = item.data["PlaceholderText"];
        }
    }
}