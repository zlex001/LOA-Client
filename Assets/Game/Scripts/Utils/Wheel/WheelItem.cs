using UnityEngine;
using System.Collections;
using UnityEngine.UI;
namespace Game
{
    public class WheelItem : MonoBehaviour
    {

        public Text ItemText;
        public int ItemIndex { get; set; }

        public virtual void Init(string text, int index)
        {
            if (ItemText != null)
                ItemText.text = text;
            ItemIndex = index;
        }
    }
}
