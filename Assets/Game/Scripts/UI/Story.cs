using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Game
{
    public class Story : UI.Core
    {
        #region Constants
        private const float UnitHeight = 83f;
        private const float GoldenRatio = 0.618f;
        #endregion

        #region Fields
        private GameObject itemPrefab;
        private Transform contentRoot;
        private ScrollRect scrollRect;

        private List<Protocol.Story.Dialogue> dialogues;
        private int currentIndex = 0;
        private bool isTyping = false;
        private Coroutine typingCoroutine;
        private Text currentWordsText;
        private string currentFullText;
        #endregion

        #region Lifecycle Methods

        public override void OnCreate(params object[] args)
        {
            ApplyAbsoluteLayout();

            dialogues = Data.Instance.StoryDialogues;
            gameObject.AddComponent<Button>().onClick.AddListener(OnClick);
            transform.Find("Confirm").GetComponent<Button>().onClick.AddListener(() => Close());
            contentRoot = transform.Find("ListView/Viewport/Content");
            itemPrefab = transform.Find("ListView/Viewport/Content/Item").gameObject;
            scrollRect = transform.Find("ListView").GetComponent<ScrollRect>();
            itemPrefab.SetActive(false);

            
            currentIndex = 0;
            OnClick();
        }

        public override void OnScreenAdaptationChanged()
        {
            ApplyAbsoluteLayout();
            UpdateContentHeight();
        }

        public override void OnClose()
        {
            // Notify server that story playback is complete
            Net.Instance.Send(new Protocol.StoryComplete());
        }

        private void ApplyAbsoluteLayout()
        {
            float screenWidth = GetComponent<RectTransform>().rect.width;
            float screenHeight = GetComponent<RectTransform>().rect.height;

            float totalMargin = screenHeight * (1 - GoldenRatio);
            float bottomMargin = totalMargin * GoldenRatio;
            float topMargin = totalMargin * (1 - GoldenRatio);

            float confirmHeight = UnitHeight;
            float listViewHeight = screenHeight - topMargin - bottomMargin;
            float horizontalMarginRatio = (1 - GoldenRatio) * GoldenRatio;
            float listViewWidth = screenWidth * (1 - horizontalMarginRatio);

            var listViewRect = transform.Find("ListView")?.GetComponent<RectTransform>();
            if (listViewRect != null)
            {
                listViewRect.anchorMin = new Vector2(0.5f, 0f);
                listViewRect.anchorMax = new Vector2(0.5f, 0f);
                listViewRect.pivot = new Vector2(0.5f, 0f);
                listViewRect.sizeDelta = new Vector2(listViewWidth, listViewHeight);
                listViewRect.anchoredPosition = new Vector2(0, bottomMargin);
            }

            var confirmRect = transform.Find("Confirm")?.GetComponent<RectTransform>();
            if (confirmRect != null)
            {
                confirmRect.anchorMin = new Vector2(0.5f, 0f);
                confirmRect.anchorMax = new Vector2(0.5f, 0f);
                confirmRect.pivot = new Vector2(0.5f, 0.5f);
                confirmRect.sizeDelta = new Vector2(listViewWidth * GoldenRatio, confirmHeight);
                confirmRect.anchoredPosition = new Vector2(0, bottomMargin / 2);
            }
        }

        #endregion

        #region Event Handlers
        private void OnClick()
        {
            if (isTyping && currentWordsText != null)
            {
                StopCoroutine(typingCoroutine);
                currentWordsText.text = currentFullText;
                isTyping = false;
                return;
            }

            if (currentIndex >= dialogues.Count || isTyping) return;

            var dialogue = dialogues[currentIndex++];
            bool isNarration = string.IsNullOrEmpty(dialogue.character);
            var item = Instantiate(itemPrefab, contentRoot);
            item.SetActive(true);

            // 设置Life文本和可见性
            GameObject lifeObj = item.transform.Find("Life").gameObject;
            lifeObj.SetActive(!isNarration);

            float lifeHeight = 0;
            if (!isNarration)
            {
                Text lifeText = lifeObj.GetComponent<Text>();
                lifeText.text = dialogue.character;
                RectTransform lifeRect = lifeText.GetComponent<RectTransform>();
                lifeRect.sizeDelta = new Vector2(0, 0);
                lifeRect.sizeDelta = new Vector2(0, lifeText.preferredHeight);
                lifeHeight = lifeText.preferredHeight;
                lifeText.alignment =  currentIndex > 0 && dialogue.character == dialogues[currentIndex - 1].character
                    ? TextAnchor.MiddleLeft
                    : TextAnchor.MiddleRight;
            }

            // 设置Words文本
            Text wordsText = item.transform.Find("Words").GetComponent<Text>();
            wordsText.text = dialogue.words;
            RectTransform wordsRect = wordsText.GetComponent<RectTransform>();
            wordsRect.sizeDelta = new Vector2(0, 0);
            wordsRect.sizeDelta = new Vector2(0, wordsText.preferredHeight);
            float wordsHeight = wordsText.preferredHeight;
            
            // 只有当 Life 可见时才需要加 spacing（因为 spacing 是 Life 和 Words 之间的间距）
            float spacingInsideItem = item.GetComponent<VerticalLayoutGroup>()?.spacing ?? 0f;
            float totalItemHeight = lifeHeight + wordsHeight;
            if (!isNarration)
            {
                totalItemHeight += spacingInsideItem;
            }
            item.GetComponent<RectTransform>().sizeDelta = new Vector2(0, totalItemHeight);

            // 开始打字效果
            currentWordsText = wordsText;
            currentFullText = dialogue.words;
            typingCoroutine = StartCoroutine(TypeWriter(wordsText, dialogue.words));

            // 更新内容高度并滚动
            UpdateContentHeight();
            StartCoroutine(ScrollToBottom());

            // 显示确认按钮
            if (currentIndex >= dialogues.Count)
            {
                StartCoroutine(WaitAndShowConfirm());
            }
        }
        #endregion

        #region Coroutines
        private IEnumerator TypeWriter(Text text, string content)
        {
            isTyping = true;
            text.text = "";
            foreach (char c in content)
            {
                text.text += c;
                yield return new WaitForSeconds(0.03f);
            }
            isTyping = false;
        }

        private IEnumerator ScrollToBottom()
        {
            yield return null; // 等待一帧让UI更新

            // 平滑滚动到底部
            float targetPosition = 0;
            float duration = 0.5f;
            float startPosition = scrollRect.verticalNormalizedPosition;
            float timeElapsed = 0;

            while (timeElapsed < duration)
            {
                scrollRect.verticalNormalizedPosition = Mathf.Lerp(startPosition, targetPosition, timeElapsed / duration);
                timeElapsed += Time.deltaTime;
                yield return null;
            }
            scrollRect.verticalNormalizedPosition = targetPosition;
        }

        private IEnumerator WaitAndShowConfirm()
        {
            while (isTyping)
                yield return null;

            transform.Find("Confirm").gameObject.SetActive(true);
        }
        #endregion

        #region Debug
        void Update()
        {
            if (Input.GetKeyDown(KeyCode.F1))
            {
                OnScreenAdaptationChanged();
            }
        }

        [System.Diagnostics.Conditional("UNITY_EDITOR")]
        public static void Test()
        {
            var testDialogues = new List<Protocol.Story.Dialogue>
            {
                new Protocol.Story.Dialogue { character = "", words = "The wind whispered through the ancient forest, carrying tales of forgotten heroes..." },
                new Protocol.Story.Dialogue { character = "Guardian", words = "You have come a long way, traveler. What is it that you seek?" },
                new Protocol.Story.Dialogue { character = "Player", words = "I seek the truth. The truth about the gods who once walked among us." },
                new Protocol.Story.Dialogue { character = "Guardian", words = "The truth... is a burden few can bear. Are you prepared for what you may find?" },
                new Protocol.Story.Dialogue { character = "", words = "A cold breeze swept across the clearing, as if the very air held its breath..." },
                new Protocol.Story.Dialogue { character = "Player", words = "I am ready. Whatever the cost may be." },
                new Protocol.Story.Dialogue { character = "Guardian", words = "Then listen well, for I shall tell you the story of the Atlanteans..." },
            };
            Data.Instance.StoryDialogues = testDialogues;
        }
        #endregion

        #region Utility Methods
        private void UpdateContentHeight()
        {
            RectTransform contentRect = contentRoot.GetComponent<RectTransform>();
            VerticalLayoutGroup layoutGroup = contentRoot.GetComponent<VerticalLayoutGroup>();
            
            // 给 VerticalLayoutGroup 添加 bottom padding，确保最后一行完全可见
            if (layoutGroup != null)
            {
                var padding = layoutGroup.padding;
                padding.bottom = 100; // 添加底部边距
                layoutGroup.padding = padding;
            }
            
            // 确保 Content 有 ContentSizeFitter 组件，让 Unity 自动计算高度
            var sizeFitter = contentRoot.GetComponent<ContentSizeFitter>();
            if (sizeFitter == null)
            {
                sizeFitter = contentRoot.gameObject.AddComponent<ContentSizeFitter>();
            }
            sizeFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            
            // 强制刷新布局
            LayoutRebuilder.ForceRebuildLayoutImmediate(contentRect);
        }
        #endregion
    }
}

