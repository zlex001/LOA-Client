using Framework;
using UnityEngine;
using UnityEngine.UI;
using LitJson;

namespace Game
{
    public class OptionTitleButtonWithProgress : OptionItem
    {
        void Start()
        {
            transform.Find("Button").GetComponent<Button>().onClick.AddListener(OnClick);
        }
        
        public void Refresh(int index, Protocol.Option.Item item)
        {
            this.index = index;
            transform.Find("Button/Text").GetComponent<Text>().text = item.data["Button"];
            transform.Find("Title/Text").GetComponent<Text>().text = item.data["Title"];
            
            // 设置进度条（现在Progress在根级别）
            if (item.data.ContainsKey("SliderValues"))
            {
                var values = JsonMapper.ToObject<int[]>(item.data["SliderValues"]);
                Image progressBar = transform.Find("Progress/Value").GetComponent<Image>();
                progressBar.fillAmount = (float)values[0] / values[1];
                
                // 设置数值文本显示
                Text valueText = transform.Find("Progress/Text")?.GetComponent<Text>();
                if (valueText != null)
                {
                    valueText.text = $"{values[0]}/{values[1]}";
                }
                
                // 使用服务端发送的颜色（服务端现在总是发送颜色）
                if (item.data.ContainsKey("ValueColor") && !string.IsNullOrEmpty(item.data["ValueColor"]))
                {
                    progressBar.color = ColorUtility.TryParseHtmlString($"#{item.data["ValueColor"].TrimStart('#')}", out var color) ? color : Color.white;
                }
            }
        }
        
        private void OnClick()
        {
            Utils.ClickEffect.TextPress(transform.Find("Button/Text"), transform.Find("Button/Text").GetComponent<Text>());
            Net.Instance.Send(new Protocol.OptionButton(index, side));
        }
    }
}
