using Game.Net;
using UnityEngine.UI;

namespace Game.Presentation
{
    using Protocol = Game.Net.Protocol;

    public class OptionToggle : OptionItem
    {
        public Text text;
        public Toggle toggle;

        private void Awake()
        {
            toggle.onValueChanged.AddListener(OnToggleValueChanged);
        }

        private void OnToggleValueChanged(bool value)
        {
            if (value)
            {
                string label = text.text == "全部" ? "" : text.text;
                NetManager.Instance.Send(new Protocol.OptionToggle(label, value));
            }
        }

        public void SetToggleValue(bool value)
        {
            toggle.SetIsOnWithoutNotify(value);
        }

        public override void OnClose()
        {
            toggle.onValueChanged.RemoveListener(OnToggleValueChanged);
        }
    }
}