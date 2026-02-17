using SuperScrollView;
using System;
using System.Collections;
using UnityEngine.UI;
using UnityEngine;
using DG.Tweening;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using Framework;

namespace Game.Utils
{
    public static class Pos
    {
        public static void Attach(RectTransform canvasRect, RectTransform tipRect, Vector2 screenPos, out Vector2 localPos, out Vector2 pivot)
        {
            LayoutRebuilder.ForceRebuildLayoutImmediate(tipRect);
            Vector2 tipSize = tipRect.sizeDelta;
            float margin = 10f;

            pivot = new Vector2(0.5f, 0.5f);

            if (screenPos.y + tipSize.y + margin > Screen.height)
            {
                pivot.y = 1;
            }
            else
            {
                pivot.y = 0;
            }

            if (screenPos.x + tipSize.x / 2 + margin > Screen.width)
            {
                pivot.x = 1;
            }
            else if (screenPos.x - tipSize.x / 2 - margin < 0)
            {
                pivot.x = 0;
            }
            else
            {
                pivot.x = 0.5f;
            }

            RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, screenPos, Camera.main, out localPos);

            tipRect.pivot = pivot;
        }


        public static Vector2 LocalPos(RectTransform rect, Transform target)
        {
            Vector3 worldPos = target.position;

            Canvas canvas = rect.GetComponentInParent<Canvas>();
            Camera cam = canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : Camera.main;

            Vector2 screenPos = RectTransformUtility.WorldToScreenPoint(cam, worldPos);
            RectTransformUtility.ScreenPointToLocalPointInRectangle(rect, screenPos, cam, out var localPos);

            RectTransform targetRect = target.GetComponent<RectTransform>();
            Vector2 pivot = targetRect.pivot;

            localPos.x -= targetRect.rect.width * (pivot.x - 0.5f);
            localPos.y -= targetRect.rect.height * (pivot.y - 0.5f);
            return localPos;
        }

    }
    public static class Ring
    {
        public static void Click(GameObject root, Vector2 pos, MonoBehaviour context)
        {
            var obj = Creat(root, pos);

            context.StartCoroutine(DestroyRingAfterDelay(obj, 0.3f));
        }

        public static void Loop(GameObject root, Vector2 pos, MonoBehaviour context)
        {
            context.StartCoroutine(LoopRoutine(root, pos));
        }

        private static GameObject Creat(GameObject root, Vector2 pos)
        {
            var obj = new GameObject("Ring");
            obj.transform.SetParent(root.transform, false);

            var rect = obj.AddComponent<RectTransform>();
            rect.localPosition = pos;
            rect.sizeDelta = new Vector2(70, 70);

            var img = obj.AddComponent<Image>();
            img.sprite = AssetManager.Instance.LoadSprite("RawAssets/Texture", "Ring");
            img.color = new Color(1, 1, 1, 0.7f);
            img.raycastTarget = false;
            img.preserveAspect = true;

            var c = obj.AddComponent<Canvas>();
            c.overrideSorting = true;
            c.sortingOrder = 32767;

            var t = Time.time;

            obj.AddComponent<Ripple>().Init(img, t, 0.3f);

            return obj; 
        }

        private static IEnumerator DestroyRingAfterDelay(GameObject obj, float delay)
        {
            yield return new WaitForSeconds(delay);
            if (obj != null)
            {
                UnityEngine.Object.Destroy(obj);
            }
        }
        private static IEnumerator LoopRoutine(GameObject root, Vector2 pos)
        {
            while (true)
            {
                var obj = Creat(root, pos);
                yield return new WaitForSeconds(0.3f);
                UnityEngine.Object.Destroy(obj);
            }
        }
    }
    public class Ripple : MonoBehaviour
    {
        private Image targetImage;
        private float startTime;
        private float duration;

        public void Init(Image image, float start, float animDuration) { targetImage = image; startTime = start; duration = animDuration; }

        void Update()
        {
            float elapsedTime = Time.time - startTime;
            float progress = Mathf.Clamp01(elapsedTime / duration);
            float smoothProgress = progress * (2 - progress);
            float scale = 1f + smoothProgress * 1.5f;
            transform.localScale = new Vector3(scale, scale, 1);
            float alpha = 0.9f * (1f - Mathf.Pow(progress, 0.5f));
            targetImage.color = new Color(1f, 1f, 1f, alpha);
        }
    }
    public static class Tutorial
    {
        public static void Show(MonoBehaviour context, GameObject target, Action onClick, string text = "", Color? maskColor = null)
        {
            Canvas parentCanvas = target.GetComponentInParent<Canvas>();
            if (parentCanvas == null)
            {
                Debug.LogError("Tutorial", "Target has no Canvas parent");
                return;
            }


            GameObject maskObj = new GameObject("GuideMask");
            maskObj.transform.SetParent(parentCanvas.transform, false);
            maskObj.transform.SetAsLastSibling();

            RectTransform maskRect = maskObj.AddComponent<RectTransform>();
            maskRect.anchorMin = Vector2.zero;
            maskRect.anchorMax = Vector2.one;
            maskRect.offsetMin = Vector2.zero;
            maskRect.offsetMax = Vector2.zero;

            Image maskImage = maskObj.AddComponent<Image>();
            maskImage.color = maskColor ?? new Color(0, 0, 0, 0.6f);
            maskImage.raycastTarget = true;

            target.transform.SetAsLastSibling();

            Button blocker = maskObj.AddComponent<Button>();
            blocker.onClick.AddListener(() =>
            {
                onClick?.Invoke();
                GameObject.Destroy(maskObj);
            });

            context.StartCoroutine(ClickLoopEffect(target.transform));

            if (!string.IsNullOrEmpty(text))
            {
                GameObject textObj = new GameObject("Text");
                textObj.transform.SetParent(parentCanvas.transform, false);

                RectTransform textRect = textObj.AddComponent<RectTransform>();
                textRect.sizeDelta = new Vector2(800, 120);
                textRect.position = target.transform.position + new Vector3(0, 100, 0);

                Text uiText = textObj.AddComponent<Text>();
                uiText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
                uiText.fontSize = 38;
                uiText.alignment = TextAnchor.MiddleCenter;
                uiText.color = Color.white;
                uiText.text = text;

                textObj.transform.SetAsLastSibling();

                maskObj.AddComponent<DestroyWith>().targets.Add(textObj);
            }
        }

        private static IEnumerator ClickLoopEffect(Transform target)
        {
            while (target != null)
            {
                GameObject fx = GameObject.CreatePrimitive(PrimitiveType.Quad);
                fx.name = "ClickCircle";
                fx.transform.SetParent(target.parent, false);
                fx.transform.position = target.position;
                fx.transform.localScale = Vector3.one * 0.5f;

                var group = fx.AddComponent<CanvasGroup>();
                group.alpha = 1f;

                fx.GetComponent<Renderer>().material = new Material(Shader.Find("UI/Default"));

                fx.transform.DOScale(1.2f, 0.5f);
                DOTween.To(() => group.alpha, x => group.alpha = x, 0f, 0.5f)
                    .OnComplete(() => GameObject.Destroy(fx));

                yield return new WaitForSeconds(0.6f);
            }
        }

        private class DestroyWith : MonoBehaviour
        {
            public List<GameObject> targets = new List<GameObject>();

            private void OnDestroy()
            {
                foreach (var go in targets)
                {
                    if (go != null)
                        GameObject.Destroy(go);
                }
            }
        }
    }
    public static class SmoothScroll
    {
        public static void MoveToRowColumnWithTime(MonoBehaviour context, LoopGridView gridView, int row, int column, float offsetX, float offsetY, float duration)
        {
            context.StartCoroutine(ToRowColumn(gridView, row, column, offsetX, offsetY, duration));
        }

        private static IEnumerator ToRowColumn(LoopGridView gridView, int row, int column, float offsetX, float offsetY, float duration)
        {
            ScrollRect scrollRect = gridView.ScrollRect;
            float startVertical = scrollRect.verticalNormalizedPosition;
            float startHorizontal = scrollRect.horizontalNormalizedPosition;
            gridView.MovePanelToItemByRowColumn(row, column, offsetX, offsetY);
            yield return null;
            float endVertical = scrollRect.verticalNormalizedPosition;
            float endHorizontal = scrollRect.horizontalNormalizedPosition;
            scrollRect.verticalNormalizedPosition = startVertical;
            scrollRect.horizontalNormalizedPosition = startHorizontal;
            yield return null;
            float time = 0f;
            while (time < duration)
            {
                time += Time.deltaTime;
                float t = Mathf.Clamp01(time / duration);

                scrollRect.verticalNormalizedPosition = Mathf.Lerp(startVertical, endVertical, t);
                scrollRect.horizontalNormalizedPosition = Mathf.Lerp(startHorizontal, endHorizontal, t);

                yield return null;
            }
            scrollRect.verticalNormalizedPosition = endVertical;
            scrollRect.horizontalNormalizedPosition = endHorizontal;
        }
    }
    public static class ClickEffect
    {
        public static void TextPress(Transform target, UnityEngine.UI.Text text = null, (string, string, bool, bool)? sfx = null)
        {
            target.DOKill();
            target.localScale = Vector3.one;
            target.DOScale(0.9f, 0.05f).SetLoops(2, LoopType.Yoyo);

            if (text != null)
            {
                var original = text.color;
                var pressed = Color.gray;
                text.color = pressed;
                DOTween.To(() => text.color, x => text.color = x, original, 0.2f);
            }

            if (sfx != null)
            {
                // TODO: Audio call moved to upper layer
                // Audio.Instance.Play(sfx.Value);
            }
        }
        public static void TextEcho(
       Transform target,
       Text text,
       (string, string, bool, bool)? sfx = null,
       Action onComplete = null,
       float duration = 0.5f
   )
        {
            var echo = GameObject.Instantiate(text.gameObject, text.transform.parent);
            var echoText = echo.GetComponent<Text>();

            echoText.raycastTarget = false;

            string rawText = Regex.Replace(text.text, @"<\/?color.*?>", "", RegexOptions.IgnoreCase);
            echoText.text = rawText;

            echoText.color = Color.white;

            echo.transform.localScale = Vector3.one;
            echo.transform.localPosition = text.transform.localPosition;
            echo.transform.SetAsLastSibling();

            var group = echo.AddComponent<CanvasGroup>();
            group.alpha = 1f;

            echo.transform.DOScale(1.5f, duration);
            DOTween.To(() => group.alpha, x => group.alpha = x, 0f, duration)
                   .OnComplete(() =>
                   {
                       GameObject.Destroy(echo);
                       onComplete?.Invoke();
                   });
        }


    }
    public static class Path
    {

      public static  string Get(Transform target, Transform root)
        {
            string path = target.name;
            while (target.parent != null && target.parent != root)
            {
                target = target.parent;
                path = target.name + "/" + path;
            }
            return path;
        }
    }






    public static class Debug
    {
        public static bool EnableLog = true;
        public static bool EnableHeartbeatLog = false;

        private const string COLOR_INFO = "#00FFFF";
        private const string COLOR_WARNING = "#FFFF00";  // yellow
        private const string COLOR_ERROR = "#FF0000";  // red
        private const string COLOR_SUCCESS = "#00FF00";  // green

        private static string ColorText(string text, string color) => $"<color={color}>{text}</color>";

        private static string GetPrefix(string module)
        {
            return string.IsNullOrEmpty(module) ? "" : $"[{module}] ";
        }

        public static void Log(string message)
        {
            if (!EnableLog) return;
            UnityEngine.Debug.Log(ColorText(message, COLOR_INFO));
        }

        public static void Log(string module, string message)
        {
            if (!EnableLog) return;
            UnityEngine.Debug.Log(ColorText(GetPrefix(module) + message, COLOR_INFO));
        }
        
        public static void LogHeartbeat(string module, string message)
        {
            if (!EnableLog || !EnableHeartbeatLog) return;
            UnityEngine.Debug.Log(ColorText(GetPrefix(module) + "[Heartbeat] " + message, COLOR_INFO));
        }

        public static void LogFormat(string module, string format, params object[] args)
        {
            Log(module, string.Format(format, args));
        }

        public static void LogWarning(string message)
        {
            if (!EnableLog) return;
            UnityEngine.Debug.LogWarning(ColorText(message, COLOR_WARNING));
        }

        public static void LogWarning(string module, string message)
        {
            if (!EnableLog) return;
            UnityEngine.Debug.LogWarning(ColorText(GetPrefix(module) + message, COLOR_WARNING));
        }

        public static void LogWarningFormat(string module, string format, params object[] args)
        {
            LogWarning(module, string.Format(format, args));
        }

        public static void LogError(string message)
        {
            if (!EnableLog) return;
            UnityEngine.Debug.LogError(ColorText(message, COLOR_ERROR));
        }

        public static void LogError(string module, string message)
        {
            if (!EnableLog) return;
            UnityEngine.Debug.LogError(ColorText(GetPrefix(module) + message, COLOR_ERROR));
        }

        public static void LogErrorFormat(string module, string format, params object[] args)
        {
            LogError(module, string.Format(format, args));
        }

        public static void LogError(string module, string message, System.Exception ex)
        {
            if (!EnableLog) return;
            UnityEngine.Debug.LogError(ColorText($"{GetPrefix(module)}{message}\n{ex}", COLOR_ERROR));
        }

        public static void LogSuccess(string message)
        {
            if (!EnableLog) return;
            UnityEngine.Debug.Log(ColorText(message, COLOR_SUCCESS));
        }

        public static void LogSuccess(string module, string message)
        {
            if (!EnableLog) return;
            UnityEngine.Debug.Log(ColorText(GetPrefix(module) + message, COLOR_SUCCESS));
        }

        public static void LogSuccessFormat(string module, string format, params object[] args)
        {
            LogSuccess(module, string.Format(format, args));
        }
    }




}
