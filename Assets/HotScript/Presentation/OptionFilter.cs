using Framework;
using Game.Net;
using UnityEngine;
using UnityEngine.UI;
using Protocol = Game.Net.Protocol;

namespace Game.Presentation
{
    public class OptionFilter : OptionItem
    {
        void Start()
        {
            GetComponent<InputField>().onEndEdit.AddListener((text) => Net.Instance.Send(new Protocol.OptionFilter(index,text)));
        }
        public void Refresh(int index, OptionItemData item)
        {
            this.index = index;
            transform.Find("Text").GetComponent<Text>().text = item.data["Text"];
            transform.Find("Placeholder").GetComponent<Text>().text = item.data["PlaceholderText"];
        }

    }
}