using Framework;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Purchasing;
using UnityEngine.Purchasing.Extension;

namespace Game
{
    public class OptionIAP : OptionItem
    {
        public Button touchBtn;
        public Text text;
        private void Awake()
        {
            touchBtn.AddOnPointerSoundClick(OnButtonClick);
        }
        private void OnButtonClick(Button obj)
        {
            //Data.Instance.Change(Data.Type.PlayerState, Data.PlayerState.Purchasing);
        }
        
        public void OnPurchaseComplete(Product product)
        {
            Game.Event.Instance.Fire("PurchaseComplete", product.receipt);
            UI.Instance.Close(Config.UI.Option);
            //Data.Instance.Change(Data.Type.PlayerState, Data.PlayerState.Normal);
        }
        public void OnPurchaseFail(Product product, PurchaseFailureDescription a)
        {
            UI.Instance.Close(Config.UI.Option);
            //Data.Instance.Change(Data.Type.PlayerState, Data.PlayerState.Normal);
        }

        public void SetProdut(string text)
        {
            GetComponent<CodelessIAPButton>().productId = text;
            GetComponent<CodelessIAPButton>().onPurchaseComplete.AddListener(OnPurchaseComplete);
            GetComponent<CodelessIAPButton>().onPurchaseFailed.AddListener(OnPurchaseFail);
        }
        public void SetText(string text)
        {
            this.text.text = text;
        }
        public void SetScrollRect(GameObject rectObj)
        {
            DragElement dragElement = touchBtn.GetComponent<DragElement>();
            dragElement.SetScrollRect(rectObj);
        }
    }
}