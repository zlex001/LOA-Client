using LitJson;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;

namespace Game
{
    /// <summary>
    /// Tutorial guidance UI that highlights targets based on server instructions.
    /// No mask overlay - users can freely click other areas.
    /// Supports UI elements, map locations, creatures, and items.
    /// </summary>
    public class Tutorial : UI.Core
    {
        #region Fields
        private Protocol.Tutorial currentStep;
        private Transform highlightedTarget;
        private Coroutine rippleCoroutine;
        #endregion

        #region Lifecycle Methods
        public override void OnCreate(params object[] args)
        {
            Utils.Debug.Log("Tutorial", "OnCreate called");
            // Make background fully transparent and non-blocking
            var images = GetComponents<Image>();
            foreach (var img in images)
            {
                img.color = new Color(0, 0, 0, 0);
                img.raycastTarget = false;
            }
        }

        public override void OnEnter(params object[] args)
        {
            Utils.Debug.Log("Tutorial", "OnEnter called");
            currentStep = Data.Instance.TutorialStep;
            if (currentStep == null)
            {
                Utils.Debug.Log("Tutorial", "TutorialStep is null, trying legacy flow");
                // Fallback to legacy TutorialIndex-based flow
                HandleLegacyTutorial();
                return;
            }

            Utils.Debug.Log("Tutorial", $"Processing step: targetType={(Protocol.Tutorial.TargetType)currentStep.targetType}");
            StartCoroutine(ProcessTutorialStep());
        }

        public override void OnClose()
        {
            StopHighlight();
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
            Utils.Debug.Log("Tutorial", $"HighlightUIElement: targetPath={currentStep.targetPath}");

            if (string.IsNullOrEmpty(currentStep.targetPath))
            {
                Utils.Debug.LogWarning("Tutorial", "targetPath is empty for UI target type");
                yield break;
            }

            // Find the target UI element across all open UIs
            Transform target = null;
            var allUIs = FindObjectsOfType<UI.Core>();
            Utils.Debug.Log("Tutorial", $"Found {allUIs.Length} UI.Core instances");

            foreach (var ui in allUIs)
            {
                if (ui == this) continue;

                Utils.Debug.Log("Tutorial", $"Searching in UI: {ui.Id}");
                target = ui.GetComponentsInChildren<Transform>(true)
                    .FirstOrDefault(t => t.gameObject.activeInHierarchy && Utils.Path.Get(t, ui.transform) == currentStep.targetPath);

                if (target != null)
                {
                    Utils.Debug.Log("Tutorial", $"Target found in {ui.Id}: {target.name}");
                    break;
                }
            }

            if (target == null)
            {
                Utils.Debug.LogWarning("Tutorial", $"UI target not found: {currentStep.targetPath}");
                yield break;
            }

            highlightedTarget = target;

            // Add click handler to complete tutorial step
            var button = target.GetComponent<Button>();
            if (button != null)
            {
                button.onClick.AddListener(OnTargetClicked);
                Utils.Debug.Log("Tutorial", "Click handler added to target button");
            }
            else
            {
                Utils.Debug.LogWarning("Tutorial", "Target has no Button component");
            }

            // Show ripple effect at target position
            RectTransform uiRect = GetComponent<RectTransform>();
            Vector2 localPos = Utils.Pos.LocalPos(uiRect, target);
            Utils.Debug.Log("Tutorial", $"Showing ripple effect at localPos: {localPos}");
            Utils.Ring.Loop(gameObject, localPos, this);

            // Show hint text
            if (!string.IsNullOrEmpty(currentStep.hint))
            {
                Vector2 screen = RectTransformUtility.WorldToScreenPoint(Camera.main, target.position);
                Utils.Debug.Log("Tutorial", $"Showing hint: {currentStep.hint} at screen pos: {screen}");
                Data.Instance.Tip = (UI.Tips.Attach, JsonMapper.ToJson(new { text = currentStep.hint, screen = new[] { screen.x, screen.y } }));
            }
        }
        #endregion

        #region Map Highlighting
        private IEnumerator HighlightMapLocation()
        {
            if (currentStep.targetPos == null || currentStep.targetPos.Length < 3)
            {
                Utils.Debug.LogWarning("Tutorial", "targetPos is null or invalid for Map target type");
                yield break;
            }

            Utils.Debug.Log("Tutorial", $"Map highlight requested for pos: [{currentStep.targetPos[0]},{currentStep.targetPos[1]},{currentStep.targetPos[2]}]");

            // Find all HomeMap components and locate the one with matching position
            var allMaps = FindObjectsOfType<HomeMap>();
            Utils.Debug.Log("Tutorial", $"Found {allMaps.Length} HomeMap instances");

            HomeMap targetMap = null;
            foreach (var map in allMaps)
            {
                // Access the map's position through its RectTransform name or internal data
                // HomeMap stores its Map data internally, we need to find by position
                var mapField = map.GetType().GetField("map", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                if (mapField != null)
                {
                    var mapData = mapField.GetValue(map) as Map;
                    if (mapData != null && mapData.pos != null && mapData.pos.Length >= 3)
                    {
                        if (mapData.pos[0] == currentStep.targetPos[0] &&
                            mapData.pos[1] == currentStep.targetPos[1] &&
                            mapData.pos[2] == currentStep.targetPos[2])
                        {
                            targetMap = map;
                            Utils.Debug.Log("Tutorial", $"Found target map: {mapData.name} at pos [{mapData.pos[0]},{mapData.pos[1]},{mapData.pos[2]}]");
                            break;
                        }
                    }
                }
            }

            if (targetMap == null)
            {
                Utils.Debug.LogWarning("Tutorial", $"Map not found at pos: [{currentStep.targetPos[0]},{currentStep.targetPos[1]},{currentStep.targetPos[2]}]");
                yield break;
            }

            highlightedTarget = targetMap.transform;

            // Show ripple effect at map position
            RectTransform tutorialRect = GetComponent<RectTransform>();
            Vector2 localPos = Utils.Pos.LocalPos(tutorialRect, targetMap.transform);
            Utils.Debug.Log("Tutorial", $"Showing ripple effect at map localPos: {localPos}");
            Utils.Ring.Loop(gameObject, localPos, this);

            // Add click handler to map button
            var button = targetMap.GetComponent<Button>();
            if (button != null)
            {
                button.onClick.AddListener(OnTargetClicked);
                Utils.Debug.Log("Tutorial", "Click handler added to map button");
            }

            // Show hint text
            if (!string.IsNullOrEmpty(currentStep.hint))
            {
                Vector2 screen = RectTransformUtility.WorldToScreenPoint(Camera.main, targetMap.transform.position);
                Utils.Debug.Log("Tutorial", $"Showing hint: {currentStep.hint} at screen pos: {screen}");
                Data.Instance.Tip = (UI.Tips.Attach, JsonMapper.ToJson(new { text = currentStep.hint, screen = new[] { screen.x, screen.y } }));
            }
        }
        #endregion

        #region Creature Highlighting
        private IEnumerator HighlightCreature()
        {
            var homeMap = FindObjectOfType<HomeMap>();
            if (homeMap == null)
            {
                Utils.Debug.LogWarning("Tutorial", "HomeMap not found for creature highlighting");
                yield break;
            }

            // TODO: Implement creature highlighting by config ID
            // homeMap.HighlightCreature(currentStep.targetId, currentStep.hint);

            Utils.Debug.Log("Tutorial", $"Creature highlight requested for ID: {currentStep.targetId}");

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
            var homeMap = FindObjectOfType<HomeMap>();
            if (homeMap == null)
            {
                Utils.Debug.LogWarning("Tutorial", "HomeMap not found for item highlighting");
                yield break;
            }

            // TODO: Implement item highlighting by config ID
            // homeMap.HighlightItem(currentStep.targetId, currentStep.hint);

            Utils.Debug.Log("Tutorial", $"Item highlight requested for ID: {currentStep.targetId}");

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
            StopHighlight();
            Data.Instance.TutorialStep = null;
            Close();
        }
        #endregion

        #region Cleanup
        private void StopHighlight()
        {
            if (highlightedTarget != null)
            {
                var button = highlightedTarget.GetComponent<Button>();
                if (button != null)
                {
                    button.onClick.RemoveListener(OnTargetClicked);
                }
                highlightedTarget = null;
            }
        }
        #endregion

        #region Legacy Support
        /// <summary>
        /// Handles the legacy TutorialIndex-based flow for backward compatibility.
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

            target.GetComponent<Button>().onClick.AddListener(() =>
            {
                StopHighlight();
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
