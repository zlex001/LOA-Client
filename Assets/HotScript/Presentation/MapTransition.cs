using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

namespace Game
{
    public class MapTransition : MonoBehaviour
    {
        private GameObject overlay;
        private Image overlayImage;
        private bool isInitialized = false;

        void Awake()
        {
            InitializeOverlay();
        }

        private void InitializeOverlay()
        {
            if (isInitialized) return;

            overlay = new GameObject("TransitionOverlay");
            overlay.transform.SetParent(transform, false);
            overlay.transform.SetAsLastSibling();
            
            var rect = overlay.AddComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.sizeDelta = Vector2.zero;
            rect.anchoredPosition = Vector2.zero;
            
            overlayImage = overlay.AddComponent<Image>();
            overlayImage.color = new Color(0, 0, 0, 0);
            overlayImage.raycastTarget = false;

            overlay.SetActive(false);

            isInitialized = true;
        }

        public void PlayTransition(System.Action onMiddle)
        {
            if (!isInitialized) 
            {
                InitializeOverlay();
            }

            overlay.SetActive(true);
            overlayImage.color = new Color(0, 0, 0, 0);

            float duration = 0.2f;

            Sequence sequence = DOTween.Sequence();
            
            sequence.Append(overlayImage.DOFade(0.95f, duration).SetEase(Ease.OutCubic));
            
            sequence.AppendCallback(() =>
            {
                onMiddle?.Invoke();
            });
            
            sequence.AppendInterval(0.05f);
            
            sequence.Append(overlayImage.DOFade(0f, duration).SetEase(Ease.InCubic));
            
            sequence.OnComplete(() =>
            {
                overlay.SetActive(false);
            });
        }
    }
}

