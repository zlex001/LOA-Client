using Framework;
using UnityEngine;
using UnityEngine.UI;

namespace Game
{
    public class OptionFilter : OptionItem
    {
        void Start()
        {
            GetComponent<InputField>().onEndEdit.AddListener((text) => Net.Instance.Send(new Protocol.OptionFilter(index,text)));
        }
        public void Refresh(int index, Protocol.Option.Item item)
        {
            this.index = index;
            transform.Find("Text").GetComponent<Text>().text = item.data["Text"];
            transform.Find("Placeholder").GetComponent<Text>().text = item.data["PlaceholderText"];
        }

    }
}