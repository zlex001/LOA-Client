using Framework;
using Game.Basic;
using Game.Data;
using Game.Utils;
using Config = Game.Data.Config;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Purchasing;
using UnityEngine.Purchasing.Extension;

namespace Game.Presentation
{
    public class OptionAlipay : OptionItem
    {
        public Button button;
        public Text text;
        private void Awake()
        {
            button.AddOnPointerSoundClick(OnButtonClick);
        }
        private void OnButtonClick(Button obj)
        {
            //DataManager.Instance.Change(DataManager.Type.PlayerState, DataManager.PlayerState.Purchasing);
        }

        public void OnPurchaseComplete(Product product)
        {
            Game.Basic.Event.Instance.Fire("PurchaseComplete", product.receipt);
            UI.Instance.Close(Config.UI.Option);
            //DataManager.Instance.Change(DataManager.Type.PlayerState, DataManager.PlayerState.Normal);
        }
        public void OnPurchaseFail(Product product, PurchaseFailureDescription a)
        {
            UI.Instance.Close(Config.UI.Option);
            //DataManager.Instance.Change(DataManager.Type.PlayerState, DataManager.PlayerState.Normal);
        }

        public void SetProdut(string text)
        {
            // TODO: CodelessIAPButton may not exist in Unity IAP 4.9.4, needs investigation
            // GetComponent<CodelessIAPButton>().productId = text;
            // GetComponent<CodelessIAPButton>().onPurchaseComplete.AddListener(OnPurchaseComplete);
            // GetComponent<CodelessIAPButton>().onPurchaseFailed.AddListener(OnPurchaseFail);
        }
        public void SetText(string text)
        {
            this.text.text = text;
        }
        public void SetScrollRect(GameObject rectObj)
        {
            DragElement dragElement = button.GetComponent<DragElement>();
            dragElement.SetScrollRect(rectObj);
        }
    }
}