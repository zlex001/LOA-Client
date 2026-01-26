using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace Game
{
    /// <summary>
    /// Fullscreen black cutscene UI with typewriter text effect.
    /// Triggered by Protocol.Cutscene, sends Protocol.CutsceneComplete on finish or skip.
    /// </summary>
    public class Cutscene : UI.Core
    {
        #region Constants
        private const float UnitHeight = 83f;
        private const float GoldenRatio = 0.618f;
        private const int DefaultCharInterval = 30;
        private const int DefaultTextInterval = 2000;
        #endregion

        #region Fields
        private Text contentText;
        private Button skipButton;
        private Protocol.Cutscene cutsceneData;
        private int currentTextIndex;
        private bool isTyping;
        private bool isSkipped;
        private Coroutine playbackCoroutine;
        #endregion

        #region Lifecycle Methods
        public override void OnCreate(params object[] args)
        {
            ApplyAbsoluteLayout();

            contentText = transform.Find("Content").GetComponent<Text>();
            skipButton = transform.Find("Skip").GetComponent<Button>();
            skipButton.onClick.AddListener(OnSkipClicked);

            // Click anywhere to speed up or advance
            gameObject.AddComponent<Button>().onClick.AddListener(OnScreenClicked);
        }

        public override void OnEnter(params object[] args)
        {
            cutsceneData = Data.Instance.Cutscene;
            if (cutsceneData == null || cutsceneData.texts == null || cutsceneData.texts.Length == 0)
            {
                Utils.Debug.LogWarning("Cutscene", "Cutscene data is null or empty");
                Close();
                return;
            }

            currentTextIndex = 0;
            isSkipped = false;
            contentText.text = "";

            // Play background music if specified
            if (!string.IsNullOrEmpty(cutsceneData.bgm))
            {
                // TODO: Audio.Instance.PlayBGM(cutsceneData.bgm);
            }

            // Play sound effect if specified
            if (!string.IsNullOrEmpty(cutsceneData.sfx))
            {
                // TODO: Audio.Instance.PlaySFX(cutsceneData.sfx);
            }

            playbackCoroutine = StartCoroutine(PlayCutscene());
        }

        public override void OnClose()
        {
            if (playbackCoroutine != null)
            {
                StopCoroutine(playbackCoroutine);
                playbackCoroutine = null;
            }
        }

        public override void OnScreenAdaptationChanged()
        {
            ApplyAbsoluteLayout();
        }
        #endregion

        #region Layout
        private void ApplyAbsoluteLayout()
        {
            float screenWidth = GetComponent<RectTransform>().rect.width;
            float screenHeight = GetComponent<RectTransform>().rect.height;

            // Content text - centered with golden ratio margins
            float horizontalMargin = screenWidth * (1 - GoldenRatio) / 2;
            float verticalMargin = screenHeight * (1 - GoldenRatio) / 2;

            var contentRect = transform.Find("Content")?.GetComponent<RectTransform>();
            if (contentRect != null)
            {
                contentRect.anchorMin = Vector2.zero;
                contentRect.anchorMax = Vector2.one;
                contentRect.offsetMin = new Vector2(horizontalMargin, verticalMargin);
                contentRect.offsetMax = new Vector2(-horizontalMargin, -verticalMargin);
            }

            // Skip button - bottom right corner
            var skipRect = transform.Find("Skip")?.GetComponent<RectTransform>();
            if (skipRect != null)
            {
                skipRect.anchorMin = new Vector2(1, 0);
                skipRect.anchorMax = new Vector2(1, 0);
                skipRect.pivot = new Vector2(1, 0);
                skipRect.sizeDelta = new Vector2(UnitHeight * 2, UnitHeight);
                skipRect.anchoredPosition = new Vector2(-UnitHeight / 2, UnitHeight / 2);
            }
        }
        #endregion

        #region Playback
        private IEnumerator PlayCutscene()
        {
            int charInterval = cutsceneData.charInterval > 0 ? cutsceneData.charInterval : DefaultCharInterval;
            int textInterval = cutsceneData.textInterval > 0 ? cutsceneData.textInterval : DefaultTextInterval;

            while (currentTextIndex < cutsceneData.texts.Length)
            {
                string text = cutsceneData.texts[currentTextIndex];
                yield return StartCoroutine(TypeText(text, charInterval));

                currentTextIndex++;

                // Wait between texts (unless it's the last one)
                if (currentTextIndex < cutsceneData.texts.Length)
                {
                    yield return new WaitForSeconds(textInterval / 1000f);
                }
            }

            // All texts played, wait a moment then complete
            yield return new WaitForSeconds(1f);
            CompleteCutscene(false);
        }

        private IEnumerator TypeText(string text, int charIntervalMs)
        {
            isTyping = true;
            contentText.text = "";
            float interval = charIntervalMs / 1000f;

            foreach (char c in text)
            {
                if (!isTyping)
                {
                    // Typing was interrupted, show full text
                    contentText.text = text;
                    break;
                }

                contentText.text += c;
                yield return new WaitForSeconds(interval);
            }

            isTyping = false;
        }
        #endregion

        #region Event Handlers
        private void OnScreenClicked()
        {
            if (isTyping)
            {
                // Speed up: show full current text immediately
                isTyping = false;
            }
        }

        private void OnSkipClicked()
        {
            isSkipped = true;
            CompleteCutscene(true);
        }

        private void CompleteCutscene(bool skipped)
        {
            if (playbackCoroutine != null)
            {
                StopCoroutine(playbackCoroutine);
                playbackCoroutine = null;
            }

            // Send completion callback to server
            Net.Instance.Send(new Protocol.CutsceneComplete(cutsceneData.id, skipped));

            // Clear cutscene data
            Data.Instance.Cutscene = null;

            Close();
        }
        #endregion
    }
}
