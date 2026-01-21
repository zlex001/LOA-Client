using System;
using UnityEngine;
using UnityEngine.UI;

namespace Framework
{
    [RequireComponent(typeof(Text))]
    public class FontScaler : MonoBehaviour
    {
        public const int DefaultSize = 30;

        public static event Action<int> OnFontSizeChanged;
        public static Func<int> GetCurrentFontSize;

        private Text text;
        private int defaultFontSize;

        void Awake()
        {
            text = GetComponent<Text>();
            defaultFontSize = text.fontSize;
        }

        void OnEnable()
        {
            if (GetCurrentFontSize != null)
            {
                int currentSize = GetCurrentFontSize();
                ApplySize(currentSize);
            }
            
            OnFontSizeChanged += HandleFontSizeChanged;
        }

        void OnDisable()
        {
            OnFontSizeChanged -= HandleFontSizeChanged;
        }

        private void HandleFontSizeChanged(int size)
        {
            ApplySize(size);
        }

        private void ApplySize(int size)
        {
            if (size <= 0) size = defaultFontSize > 0 ? defaultFontSize : DefaultSize;
            text.fontSize = size;
        }

        public static void NotifyFontSizeChanged(int size)
        {
            OnFontSizeChanged?.Invoke(size);
        }
    }
}
