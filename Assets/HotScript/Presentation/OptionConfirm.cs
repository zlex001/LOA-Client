using Framework;
using Game.Net;
using UnityEngine;
using UnityEngine.UI;
using Protocol = Game.Net.Protocol;

namespace Game.Presentation
{
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