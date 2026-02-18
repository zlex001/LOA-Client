using Framework;
using Game.Net;
using UnityEngine;
using UnityEngine.UI;

namespace Game.Presentation
{
    using Net = Game.Net.Net;
    using Protocol = Game.Net.Protocol;

    public class OptionConfirm : OptionItem
    {
        public Button button;

        private void Awake()
        {
            transform.GetComponent<Button>().onClick.AddListener(OnButtonClick);
        }

        private void OnButtonClick()
        {
            Net.Instance.Send(new Protocol.OptionConfirm(index, side));
        }
    }
}