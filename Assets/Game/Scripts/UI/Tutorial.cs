using LitJson;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;  

namespace Game
{
    public class Tutorial : UI.Core
    {
        public override void OnEnter(params object[] args)
        {
            var tutorial = Data.Instance.Tutorial[Data.Instance.TutorialIndex];
            (string, string, int, bool) config = tutorial.Item1;
            string path = tutorial.Item2;
            string text = tutorial.Item3;

            StartCoroutine(WaitAndContinue(config, path, text));
        }

        private IEnumerator WaitAndContinue((string, string, int, bool) config, string path, string text)
        {
            yield return new WaitForSeconds(0.145865942f);

            UI.Core ui = UI.Instance.Get(config);
            Transform target = ui.GetComponentsInChildren<Transform>(true)
                .FirstOrDefault(t => t.gameObject.activeInHierarchy && Utils.Path.Get(t, ui.transform) == path);

            Canvas canvas = target.gameObject.AddComponent<Canvas>();
            canvas.overrideSorting = true;
            //originalSortingOrder = canvas.sortingOrder;
            canvas.sortingOrder = GetComponent<Canvas>().sortingOrder + 1;
            GraphicRaycaster graphicRaycaster = target.gameObject.AddComponent<GraphicRaycaster>();
            target.GetComponent<Button>().onClick.AddListener(() =>
            {
                Destroy(graphicRaycaster);
                Destroy(canvas);
                Close();
                if (Data.Instance.TutorialIndex < Data.Instance.Tutorial.Count - 1)
                {
                    Data.Instance.TutorialIndex++;
                }
                else
                {
                    Close();
                };
            });

            RectTransform uiRect = ui.GetComponent<RectTransform>();
            Vector2 localPos = Utils.Pos.LocalPos(uiRect, target);

            Utils.Ring.Loop(gameObject, localPos, this);

            Vector2 screen = RectTransformUtility.WorldToScreenPoint(Camera.main, target.position);
            Data.Instance.Tip = (UI.Tips.Attach, JsonMapper.ToJson(new { text, screen = new[] { screen.x, screen.y } }));
        }

        public override void OnClose()
        {
            Data.Instance.Tip = (UI.Tips.Attach, null);
        }
    }
}
