using LitJson;
using Game.Logic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using SuperScrollView;
using Protocol = Game.Net.Protocol;

namespace Game.Presentation
{
    /// <summary>
    /// Tutorial guidance UI that highlights targets based on server instructions.
    /// No mask overlay - users can freely click other areas.
    /// Supports UI elements, map locations, creatures, and items.
    /// </summary>
    public class Tutorial : UI.Core
    {
        #region Fields
        private TutorialData currentStep;
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
            // Avoid accessing singleton during application quit to prevent recreation
            if (!isQuitting)
            {
                Data.Instance.Tip = (TipType.Attach, null);
            }
        }

        private static bool isQuitting = false;
        private void OnApplicationQuit()
        {
            isQuitting = true;
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
                Data.Instance.Tip = (TipType.Attach, JsonMapper.ToJson(new { text = currentStep.hint, screen = new[] { screen.x, screen.y } }));
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
                    var mapData = mapField.GetValue(map) as MapData;
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
                Data.Instance.Tip = (TipType.Attach, JsonMapper.ToJson(new { text = currentStep.hint, screen = new[] { screen.x, screen.y } }));
            }
        }
        #endregion

        #region Creature/Item Highlighting
        private IEnumerator HighlightCreature()
        {
            Utils.Debug.Log("Tutorial", $"HighlightCreature: targetId={currentStep.targetId}, targetPath={currentStep.targetPath}");
            yield return HighlightCharacterByConfigId();
        }

        private IEnumerator HighlightItem()
        {
            Utils.Debug.Log("Tutorial", $"HighlightItem: targetId={currentStep.targetId}, targetPath={currentStep.targetPath}");
            yield return HighlightCharacterByConfigId();
        }

        /// <summary>
        /// Highlights a character/item in the character list by configId,
        /// and optionally waits for Option menu to highlight a specific button.
        /// </summary>
        private IEnumerator HighlightCharacterByConfigId()
        {
            // Parse targetPath to determine the guidance flow
            // Format: "characters/goto" or "actions/interact"
            string[] pathParts = currentStep.targetPath?.Split('/') ?? new string[0];
            string targetArea = pathParts.Length > 0 ? pathParts[0] : "";
            string targetButton = pathParts.Length > 1 ? pathParts[1] : "";

            Utils.Debug.Log("Tutorial", $"Parsed targetPath: area={targetArea}, button={targetButton}");

            if (targetArea == "characters")
            {
                yield return HighlightCharacterListItem(targetButton);
            }
            else if (targetArea == "actions")
            {
                // Direct Option button highlighting
                yield return HighlightOptionButton(targetButton);
            }
            else
            {
                Utils.Debug.LogWarning("Tutorial", $"Unknown target area: {targetArea}");
                if (!string.IsNullOrEmpty(currentStep.hint))
                {
                    Data.Instance.Tip = (TipType.Fly, currentStep.hint);
                }
            }
        }

        /// <summary>
        /// Highlights a character list item by configId and optionally monitors for Option button.
        /// </summary>
        private IEnumerator HighlightCharacterListItem(string optionButtonName)
        {
            // Find the Home UI
            var homeUI = FindObjectOfType<Home>();
            if (homeUI == null)
            {
                Utils.Debug.LogWarning("Tutorial", "Home UI not found for character highlighting");
                yield break;
            }

            // Find character index by configId in Data.Instance.Characters
            var characters = Data.Instance.Characters ?? Data.Instance.Home?.characters;
            if (characters == null)
            {
                Utils.Debug.LogWarning("Tutorial", "Characters list is null. Data.Instance.Characters=" + 
                    (Data.Instance.Characters == null ? "null" : "exists") + 
                    ", Data.Instance.Home=" + (Data.Instance.Home == null ? "null" : "exists"));
                yield break;
            }

            // Debug: log all characters for troubleshooting
            Utils.Debug.Log("Tutorial", $"Searching for targetId={currentStep.targetId} in {characters.Count()} characters:");
            for (int i = 0; i < characters.Count(); i++)
            {
                Utils.Debug.Log("Tutorial", $"  [{i}] name={characters[i].name}, configId={characters[i].configId}, hash={characters[i].hash}");
            }

            int targetIndex = -1;
            for (int i = 0; i < characters.Count(); i++)
            {
                // Match by configId first, fallback to hash if configId is 0
                if (characters[i].configId == currentStep.targetId || 
                    (characters[i].configId == 0 && characters[i].hash == currentStep.targetId))
                {
                    targetIndex = i;
                    Utils.Debug.Log("Tutorial", $"Found target character at index {i}: name={characters[i].name}, configId={characters[i].configId}, hash={characters[i].hash}");
                    break;
                }
            }

            if (targetIndex < 0)
            {
                Utils.Debug.LogWarning("Tutorial", $"Character with targetId {currentStep.targetId} not found in list");
                if (!string.IsNullOrEmpty(currentStep.hint))
                {
                    Data.Instance.Tip = (TipType.Fly, currentStep.hint);
                }
                yield break;
            }

            // Get the LoopListView2 and scroll to the target item
            var areaTransform = homeUI.transform.Find("Area");
            if (areaTransform == null)
            {
                Utils.Debug.LogWarning("Tutorial", "Area not found in Home UI");
                yield break;
            }

            var listView = areaTransform.GetComponent<SuperScrollView.LoopListView2>();
            if (listView == null)
            {
                Utils.Debug.LogWarning("Tutorial", "LoopListView2 not found on Area");
                yield break;
            }

            // Scroll to make the target item visible
            listView.MovePanelToItemIndex(targetIndex, 0);
            yield return new WaitForSeconds(0.1f);

            // Find the visible item
            Transform targetItem = null;
            for (int i = 0; i < listView.ShownItemCount; i++)
            {
                var item = listView.GetShownItemByIndex(i);
                if (item != null && item.ItemIndex == targetIndex)
                {
                    targetItem = item.transform;
                    Utils.Debug.Log("Tutorial", $"Found visible item at ShownIndex {i}, ItemIndex {item.ItemIndex}");
                    break;
                }
            }

            if (targetItem == null)
            {
                Utils.Debug.LogWarning("Tutorial", $"Target item at index {targetIndex} is not visible");
                yield break;
            }

            highlightedTarget = targetItem;

            // Add click handler
            var button = targetItem.GetComponent<Button>();
            if (button != null)
            {
                button.onClick.AddListener(OnCharacterClicked);
                Utils.Debug.Log("Tutorial", "Click handler added to character item");
            }

            // Show ripple effect
            RectTransform tutorialRect = GetComponent<RectTransform>();
            Vector2 localPos = Utils.Pos.LocalPos(tutorialRect, targetItem);
            Utils.Debug.Log("Tutorial", $"Showing ripple effect at character localPos: {localPos}");
            Utils.Ring.Loop(gameObject, localPos, this);

            // Show hint text
            if (!string.IsNullOrEmpty(currentStep.hint))
            {
                Vector2 screen = RectTransformUtility.WorldToScreenPoint(Camera.main, targetItem.position);
                Utils.Debug.Log("Tutorial", $"Showing hint: {currentStep.hint}");
                Data.Instance.Tip = (TipType.Attach, JsonMapper.ToJson(new { text = currentStep.hint, screen = new[] { screen.x, screen.y } }));
            }

            // If there's a button name to wait for in Option menu, register listener
            if (!string.IsNullOrEmpty(optionButtonName))
            {
                pendingOptionButton = optionButtonName;
                Data.Instance.after.Register(Data.Type.Option, OnOptionChanged);
                Utils.Debug.Log("Tutorial", $"Registered Option listener for button: {optionButtonName}");
            }
        }

        private string pendingOptionButton = null;

        private void OnCharacterClicked()
        {
            Utils.Debug.Log("Tutorial", "Character clicked, waiting for Option menu");
            // Don't close yet if we're waiting for Option button
            if (string.IsNullOrEmpty(pendingOptionButton))
            {
                StopHighlight();
                Data.Instance.TutorialStep = null;
                Close();
            }
            else
            {
                // Stop the current highlight but keep Tutorial open
                StopHighlight();
            }
        }

        private void OnOptionChanged(params object[] args)
        {
            if (string.IsNullOrEmpty(pendingOptionButton))
            {
                return;
            }

            var option = Data.Instance.Option;
            if (option == null || option.Empty)
            {
                return;
            }

            Utils.Debug.Log("Tutorial", $"Option changed, looking for button: {pendingOptionButton}");
            StartCoroutine(HighlightOptionButtonDelayed(pendingOptionButton));
        }

        private IEnumerator HighlightOptionButtonDelayed(string buttonName)
        {
            yield return new WaitForSeconds(0.2f);
            yield return HighlightOptionButton(buttonName);
        }

        /// <summary>
        /// Highlights a button in the Option menu by matching button text or key.
        /// </summary>
        private IEnumerator HighlightOptionButton(string buttonName)
        {
            var optionUI = UI.Instance.Get(Config.UI.Option) as Option;
            if (optionUI == null)
            {
                Utils.Debug.LogWarning("Tutorial", "Option UI not found");
                yield break;
            }

            // Map button name to expected text patterns
            string[] searchPatterns = GetButtonSearchPatterns(buttonName);
            Utils.Debug.Log("Tutorial", $"Searching for Option button with patterns: {string.Join(", ", searchPatterns)}");

            // Search in both Left and Right panels
            Transform targetButton = null;
            foreach (string panel in new[] { "Left/Viewport/Content", "Right/Viewport/Content" })
            {
                var content = optionUI.transform.Find(panel);
                if (content == null) continue;

                foreach (Transform child in content)
                {
                    // Check OptionTitleButton components
                    var titleBtn = child.GetComponent<OptionTitleButton>();
                    if (titleBtn != null)
                    {
                        var btnText = child.Find("Button/Text")?.GetComponent<Text>();
                        if (btnText != null && MatchesPattern(btnText.text, searchPatterns))
                        {
                            targetButton = child.Find("Button");
                            Utils.Debug.Log("Tutorial", $"Found matching TitleButton: {btnText.text}");
                            break;
                        }
                    }

                    // Check OptionButton components
                    var optionBtn = child.GetComponent<OptionButton>();
                    if (optionBtn != null)
                    {
                        var btnText = child.Find("Text")?.GetComponent<Text>();
                        if (btnText != null && MatchesPattern(btnText.text, searchPatterns))
                        {
                            targetButton = child;
                            Utils.Debug.Log("Tutorial", $"Found matching Button: {btnText.text}");
                            break;
                        }
                    }
                }

                if (targetButton != null) break;
            }

            if (targetButton == null)
            {
                Utils.Debug.LogWarning("Tutorial", $"Option button not found for: {buttonName}");
                yield break;
            }

            // Unregister Option listener
            Data.Instance.after.Unregister(Data.Type.Option, OnOptionChanged);
            pendingOptionButton = null;

            highlightedTarget = targetButton;

            // Add click handler
            var button = targetButton.GetComponent<Button>();
            if (button != null)
            {
                button.onClick.AddListener(OnTargetClicked);
                Utils.Debug.Log("Tutorial", "Click handler added to Option button");
            }

            // Show ripple effect
            RectTransform tutorialRect = GetComponent<RectTransform>();
            Vector2 localPos = Utils.Pos.LocalPos(tutorialRect, targetButton);
            Utils.Debug.Log("Tutorial", $"Showing ripple effect at Option button localPos: {localPos}");
            Utils.Ring.Loop(gameObject, localPos, this);

            // Update hint position
            if (!string.IsNullOrEmpty(currentStep.hint))
            {
                Vector2 screen = RectTransformUtility.WorldToScreenPoint(Camera.main, targetButton.position);
                Data.Instance.Tip = (TipType.Attach, JsonMapper.ToJson(new { text = currentStep.hint, screen = new[] { screen.x, screen.y } }));
            }
        }

        /// <summary>
        /// Gets search patterns for a button name based on targetPath convention.
        /// </summary>
        private string[] GetButtonSearchPatterns(string buttonName)
        {
            // Map targetPath button names to possible button text patterns
            // These patterns should match the server-sent button text
            return buttonName switch
            {
                "goto" => new[] { "goto", "go to", "move", "walk" },
                "interact" => new[] { "interact", "use", "talk" },
                "attack" => new[] { "attack", "fight", "battle" },
                "give" => new[] { "give", "gift", "hand over" },
                "pickup" => new[] { "pickup", "pick up", "take", "collect" },
                _ => new[] { buttonName }
            };
        }

        private bool MatchesPattern(string text, string[] patterns)
        {
            if (string.IsNullOrEmpty(text)) return false;
            string lowerText = text.ToLower();
            foreach (var pattern in patterns)
            {
                if (lowerText.Contains(pattern.ToLower()))
                {
                    return true;
                }
            }
            return false;
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
            // Unregister Option listener if still registered
            if (!string.IsNullOrEmpty(pendingOptionButton))
            {
                Data.Instance.after.Unregister(Data.Type.Option, OnOptionChanged);
                pendingOptionButton = null;
            }

            if (highlightedTarget != null)
            {
                var button = highlightedTarget.GetComponent<Button>();
                if (button != null)
                {
                    button.onClick.RemoveListener(OnTargetClicked);
                    button.onClick.RemoveListener(OnCharacterClicked);
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
            Data.Instance.Tip = (TipType.Attach, JsonMapper.ToJson(new { text, screen = new[] { screen.x, screen.y } }));
        }
        #endregion
    }
}
