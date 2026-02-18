using Framework;
using UnityEngine;
using UnityEngine.UI;

namespace Game
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