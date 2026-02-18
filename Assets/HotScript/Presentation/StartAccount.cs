using SuperScrollView;
using UnityEngine;
using UnityEngine.UI;
using Framework;

namespace Game.Presentation
{
    public class StartAccount : MonoBehaviour
    {
        public int index;
        void Start()
        {
            GetComponent<Toggle>().onValueChanged.AddListener(OnToggleValueChanged);
        }
        public void Refresh(int index)
        {
            this.index = index;
            Account account = Data.Instance.User.Accounts[index];
            string note = string.IsNullOrEmpty(account.Note) ? "" : $"[{account.Note}]";
            transform.Find("Label").GetComponent<Text>().text = $"[{index}]{account.Id}{note}";
            GetComponent<Toggle>().SetIsOnWithoutNotify(Data.Instance.User.SelectedAccountIndex == index);
        }
        public void Error()
        {
            transform.Find("Background/Checkmark").GetComponent<Image>().color = new Color(195 / 255f, 81 / 255f, 92 / 255f, 255 / 255f);
            transform.Find("Label").GetComponent<Text>().color = new Color(195 / 255f, 81 / 255f, 92 / 255f, 255 / 255f);
        }
        private void OnToggleValueChanged(bool isOn)
        {
            if (isOn)
            {
                Data.Instance.User.SelectedAccountIndex = index;
                Local.Instance.Save(Data.Instance.User);
            }
        }
    }
}