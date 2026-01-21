using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
namespace Game
{
    public class HomeCharacter : MonoBehaviour
    {
        private int hash;
        private bool isListenerAdded = false;

        private void OnClick()
        {
            Utils.ClickEffect.TextEcho(transform, transform.Find("Name").GetComponent<Text>());
            Net.Instance.Send(new Protocol.ClickCharacter(hash));
        }

        public void Refresh(Protocol.Characters.CharacterData data)
        {
            this.hash = data.hash;
            
            if (!isListenerAdded)
            {
                var button = GetComponent<Button>();
                if (button != null)
                {
                    button.onClick.AddListener(OnClick);
                    isListenerAdded = true;
                }
            }
            
            transform.Find("Name").GetComponent<Text>().text = data.name;
            if (data.progress > 0)
            {
                transform.Find("Progress").gameObject.SetActive(true);
                transform.Find("Progress/Value").GetComponent<Image>().fillAmount = (float)data.progress / 100;
            }
            else
            {
                transform.Find("Progress").gameObject.SetActive(false);
            }
        }

        public void UpdateBattleProgress(double progress)
        {
            if (progress > 0)
            {
                transform.Find("Progress").gameObject.SetActive(true);
                transform.Find("Progress/Value").GetComponent<Image>().fillAmount = (float)progress / 100;
            }
            else
            {
                transform.Find("Progress").gameObject.SetActive(false);
            }
        }
    }
}