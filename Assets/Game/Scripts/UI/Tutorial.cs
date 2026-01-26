using LitJson;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;

namespace Game
{
    /// <summary>
    /// Tutorial guidance UI that highlights targets based on server instructions.
    /// Supports UI elements, map locations, creatures, and items.
    /// </summary>
    public class Tutorial : UI.Core
    {
        #region Fields
        private Protocol.Tutorial currentStep;
        private Canvas targetCanvas;
        private GraphicRaycaster targetRaycaster;
        private Transform highlightedTarget;
        #endregion

        #region Lifecycle Methods
        public override void OnEnter(params object[] args)
        {
            currentStep = Data.Instance.TutorialStep;
            if (currentStep == null)
            {
                // Fallback to legacy TutorialIndex-based flow
                HandleLegacyTutorial();
                return;
            }

            StartCoroutine(ProcessTutorialStep());
        }

        public override void OnClose()
        {
            CleanupHighlight();
            Data.Instance.Tip = (UI.Tips.Attach, null);
        }
        #endregion

        #region Tutorial Processing
        private IEnumerator ProcessTutorialStep()
        {
            // Wait for UI to stabilize
            yield return new WaitForSeconds(0.15f);

            switch ((Protocol.Tutorial.TargetType)currentStep.targetType)
            {
                case Protocol.Tutorial.TargetType.UI:
                    yield return HighlightUIElement();
                    break;

                case Protocol.Tutorial.TargetType.Map:
                    yield return HighlightMapLocation();
                    break;

                case Protocol.Tutorial.TargetType.Creature:
                    yield return HighlightCreature();
                    break;

                case Protocol.Tutorial.TargetType.Item:
                    yield return HighlightItem();
                    break;

                default:
                    Utils.Debug.LogWarning("Tutorial", $"Unknown targetType: {currentStep.targetType}");
                    break;
            }
        }
        #endregion

        #region UI Element Highlighting
        private IEnumerator HighlightUIElement()
        {
            if (string.IsNullOrEmpty(currentStep.targetPath))
            {
                Utils.Debug.LogWarning("Tutorial", "targetPath is empty for UI target type");
                yield break;
            }

            // Find the target UI element across all open UIs
            Transform target = null;
            foreach (var ui in FindObjectsOfType<UI.Core>())
            {
                if (ui == this) continue;

                target = ui.GetComponentsInChildren<Transform>(true)
                    .FirstOrDefault(t => t.gameObject.activeInHierarchy && Utils.Path.Get(t, ui.transform) == currentStep.targetPath);

                if (target != null) break;
            }

            if (target == null)
            {
                Utils.Debug.LogWarning("Tutorial", $"UI target not found: {currentStep.targetPath}");
                yield break;
            }

            highlightedTarget = target;

            // Elevate target above tutorial mask
            targetCanvas = target.gameObject.AddComponent<Canvas>();
            targetCanvas.overrideSorting = true;
            targetCanvas.sortingOrder = GetComponent<Canvas>().sortingOrder + 1;
            targetRaycaster = target.gameObject.AddComponent<GraphicRaycaster>();

            // Add click handler
            var button = target.GetComponent<Button>();
            if (button != null)
            {
                button.onClick.AddListener(OnTargetClicked);
            }

            // Show ripple effect
            RectTransform uiRect = GetComponent<RectTransform>();
            Vector2 localPos = Utils.Pos.LocalPos(uiRect, target);
            Utils.Ring.Loop(gameObject, localPos, this);

            // Show hint text
            if (!string.IsNullOrEmpty(currentStep.hint))
            {
                Vector2 screen = RectTransformUtility.WorldToScreenPoint(Camera.main, target.position);
                Data.Instance.Tip = (UI.Tips.Attach, JsonMapper.ToJson(new { text = currentStep.hint, screen = new[] { screen.x, screen.y } }));
            }
        }
        #endregion

        #region Map Highlighting
        private IEnumerator HighlightMapLocation()
        {
            // Find HomeMap component
            var homeMap = FindObjectOfType<HomeMap>();
            if (homeMap == null)
            {
                Utils.Debug.LogWarning("Tutorial", "HomeMap not found for map highlighting");
                yield break;
            }

            // TODO: Implement map tile highlighting by config ID
            // homeMap.HighlightTile(currentStep.targetId, currentStep.hint);

            Utils.Debug.Log("Tutorial", $"Map highlight requested for ID: {currentStep.targetId}");

            // Show hint if provided
            if (!string.IsNullOrEmpty(currentStep.hint))
            {
                Data.Instance.Tip = (UI.Tips.Fly, currentStep.hint);
            }

            yield break;
        }
        #endregion

        #region Creature Highlighting
        private IEnumerator HighlightCreature()
        {
            // Find HomeMap component which manages creatures
            var homeMap = FindObjectOfType<HomeMap>();
            if (homeMap == null)
            {
                Utils.Debug.LogWarning("Tutorial", "HomeMap not found for creature highlighting");
                yield break;
            }

            // TODO: Implement creature highlighting by config ID
            // Find closest creature with matching config ID and highlight it
            // homeMap.HighlightCreature(currentStep.targetId, currentStep.hint);

            Utils.Debug.Log("Tutorial", $"Creature highlight requested for ID: {currentStep.targetId}");

            // Show hint if provided
            if (!string.IsNullOrEmpty(currentStep.hint))
            {
                Data.Instance.Tip = (UI.Tips.Fly, currentStep.hint);
            }

            yield break;
        }
        #endregion

        #region Item Highlighting
        private IEnumerator HighlightItem()
        {
            // Find HomeMap component which manages items
            var homeMap = FindObjectOfType<HomeMap>();
            if (homeMap == null)
            {
                Utils.Debug.LogWarning("Tutorial", "HomeMap not found for item highlighting");
                yield break;
            }

            // TODO: Implement item highlighting by config ID
            // Find closest item with matching config ID and highlight it
            // homeMap.HighlightItem(currentStep.targetId, currentStep.hint);

            Utils.Debug.Log("Tutorial", $"Item highlight requested for ID: {currentStep.targetId}");

            // Show hint if provided
            if (!string.IsNullOrEmpty(currentStep.hint))
            {
                Data.Instance.Tip = (UI.Tips.Fly, currentStep.hint);
            }

            yield break;
        }
        #endregion

        #region Event Handlers
        private void OnTargetClicked()
        {
            CleanupHighlight();
            Data.Instance.TutorialStep = null;
            Close();
        }
        #endregion

        #region Cleanup
        private void CleanupHighlight()
        {
            if (highlightedTarget != null)
            {
                var button = highlightedTarget.GetComponent<Button>();
                if (button != null)
                {
                    button.onClick.RemoveListener(OnTargetClicked);
                }
            }

            if (targetRaycaster != null)
            {
                Destroy(targetRaycaster);
                targetRaycaster = null;
            }

            if (targetCanvas != null)
            {
                Destroy(targetCanvas);
                targetCanvas = null;
            }

            highlightedTarget = null;
        }
        #endregion

        #region Legacy Support
        /// <summary>
        /// Handles the legacy TutorialIndex-based flow for backward compatibility.
        /// This will be deprecated once all tutorials migrate to SDUI protocol.
        /// </summary>
        private void HandleLegacyTutorial()
        {
            if (Data.Instance.Tutorial == null || Data.Instance.Tutorial.Count == 0)
            {
                Close();
                return;
            }

            var tutorial = Data.Instance.Tutorial[Data.Instance.TutorialIndex];
            (string, string, int, bool) config = tutorial.Item1;
            string path = tutorial.Item2;
            string text = tutorial.Item3;

            StartCoroutine(LegacyWaitAndContinue(config, path, text));
        }

        private IEnumerator LegacyWaitAndContinue((string, string, int, bool) config, string path, string text)
        {
            yield return new WaitForSeconds(0.15f);

            UI.Core ui = UI.Instance.Get(config);
            if (ui == null)
            {
                Utils.Debug.LogWarning("Tutorial", $"Legacy tutorial UI not found: {config}");
                yield break;
            }

            Transform target = ui.GetComponentsInChildren<Transform>(true)
                .FirstOrDefault(t => t.gameObject.activeInHierarchy && Utils.Path.Get(t, ui.transform) == path);

            if (target == null)
            {
                Utils.Debug.LogWarning("Tutorial", $"Legacy tutorial target not found: {path}");
                yield break;
            }

            highlightedTarget = target;

            targetCanvas = target.gameObject.AddComponent<Canvas>();
            targetCanvas.overrideSorting = true;
            targetCanvas.sortingOrder = GetComponent<Canvas>().sortingOrder + 1;
            targetRaycaster = target.gameObject.AddComponent<GraphicRaycaster>();

            target.GetComponent<Button>().onClick.AddListener(() =>
            {
                CleanupHighlight();
                Close();
                if (Data.Instance.TutorialIndex < Data.Instance.Tutorial.Count - 1)
                {
                    Data.Instance.TutorialIndex++;
                }
            });

            RectTransform uiRect = ui.GetComponent<RectTransform>();
            Vector2 localPos = Utils.Pos.LocalPos(uiRect, target);
            Utils.Ring.Loop(gameObject, localPos, this);

            Vector2 screen = RectTransformUtility.WorldToScreenPoint(Camera.main, target.position);
            Data.Instance.Tip = (UI.Tips.Attach, JsonMapper.ToJson(new { text, screen = new[] { screen.x, screen.y } }));
        }
        #endregion
    }
}
