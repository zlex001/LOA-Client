using Framework;
using Game.Net;
using UnityEngine;
using UnityEngine.UI;

namespace Game.Presentation
{
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
            NetManager.Instance.Send(new Protocol.OptionConfirm(index, side));
        }
    }
}