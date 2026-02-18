using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityTimer;
using Framework;
using Game.Basic;
using Game.Logic;
using Config = Game.Logic.Config;
using System.Linq;
using Newtonsoft.Json;
using DG.Tweening;
using LitJson;
using System;
using System.Net.Http;
using Protocol = Game.Net.Protocol;

namespace Game.Presentation
{
    public class UI : Singleton<UI>
    {
        public enum Tips
        {
            Fly,
            Numerical,
            Attach,
        }

        public enum Event
        {
            Click
        }

        private Vector2 AnchorMin { get; set; }
        private Vector2 AnchorMax { get; set; }
        private GameObject root;
        private List<Core> content = new List<Core>();

        private List<GameObject> flys = new List<GameObject>();
        private List<Timer> flyTimers = new List<Timer>();
        private Timer loginDarkTimer;
        
        public (string, string, int, bool)? Current { get; private set; }

        private const float UnitHeight = 83f;
        private const float GoldenRatio = 0.618f;
        private const float MoveSpeed = 0.6f;
        private const float FadeSpeed = 0.382f;
        private const float StayDuration = 1.0f;
        private int maxFlyTips = 5;

        public abstract class Core : MonoBehaviour
        {
            [HideInInspector]
            public bool isInit = false;
            public string Id { get; set; }
            public virtual void OnCreate(params object[] args) { }
            public virtual void OnEnter(params object[] args) { }
            public virtual void OnExit() { }
            public virtual void OnClose() { }
            public virtual void OnScreenAdaptationChanged() { }
            protected void Close() { Instance.Close(Id); }
            public void ResetTransform(Vector2 anchorMin, Vector2 anchorMax)
            {
                RectTransform rectTrans = gameObject.GetComponent<RectTransform>();
                rectTrans.anchorMin = anchorMin;
                rectTrans.anchorMax = anchorMax;
                rectTrans.anchoredPosition3D = new Vector3(0, 0, 0);
                rectTrans.sizeDelta = new Vector2(0, 0);
                rectTrans.anchoredPosition = Vector2.zero;
                rectTrans.localScale = Vector3.one;
                rectTrans.offsetMin = rectTrans.offsetMax = Vector2.zero;
            }
            public void SetActive(bool active)
            {
                gameObject.SetActive(active);
            }
        }

        public void Init(float weight, float height)
        {
            root = Instantiate(AssetManager.Instance.LoadPrefab("Prefabs/UI", "Root"));
            DontDestroyOnLoad(root);
            root.name = "Root";
            root.transform.SetParent(transform);

            float s = (float)Screen.width / Screen.height, t = weight / height, r = s / t;
            float m = Mathf.Abs(1 - (r > 1 ? 1f / r : r)) * 0.5f;
            AnchorMin = r > 1 ? new Vector2(m, 0) : new Vector2(0, m);
            AnchorMax = r > 1 ? new Vector2(1 - m, 1) : new Vector2(1, 1 - m);

            ApplyScreenUIAdaptation();

            Game.Basic.Event.Instance.Add(Event.Click, OnClick);
            Data.Instance.after.Register(Data.Type.LoginAccount, OnAfterLoginAccountChanged);
            Data.Instance.after.Register(Data.Type.Online, OnAfterOnlineChanged);
            Data.Instance.after.Register(Data.Type.Initialize, OnAfterInitializeChanged);
            Data.Instance.after.Register(Data.Type.Home, OnAfterHomeChanged);
            Data.Instance.after.Register(Data.Type.Option, OnAfterOptionChanged);
            Data.Instance.after.Register(Data.Type.StoryDialogues, OnAfterStoryChanged);
            Data.Instance.after.Register(Data.Type.TutorialIndex, OnAfterTutorialIndexChanged);
            Data.Instance.after.Register(Data.Type.TutorialStep, OnAfterTutorialStepChanged);
            Data.Instance.after.Register(Data.Type.Dark, OnAfterDataDark);

            Data.Instance.after.Register(Data.Type.Tip, OnAfterTipChanged);
        }

        public void ApplyScreenUIAdaptation()
        {
            Rect safeArea = Screen.safeArea;
            float screenHeight = Screen.height;
            
            float autoTopMargin = (screenHeight - safeArea.height - safeArea.y) / screenHeight;
            float autoBottomMargin = safeArea.y / screenHeight;
            
            float adaptationValue = Data.Instance.User.ScreenUIAdaptation;
            if (adaptationValue <= 0) adaptationValue = 1.0f;
            
            float topMargin = (autoTopMargin + 0.02f) * adaptationValue;
            float bottomMargin = (autoBottomMargin + 0.02f) * adaptationValue;
            
            AnchorMin = new Vector2(AnchorMin.x, bottomMargin);
            AnchorMax = new Vector2(AnchorMax.x, 1 - topMargin);
            
            foreach (var ui in content)
            {
                ui.ResetTransform(AnchorMin, AnchorMax);
                ui.OnScreenAdaptationChanged();
            }
        }

        void Update()
        {
            if (Input.GetMouseButtonUp(0))
            {
                Game.Basic.Event.Instance.Fire(Event.Click);
            }
        }

        public Core Get((string, string, int, bool) config)
        {
            Core core = content.FirstOrDefault(ui => ui.Id == $"{config.Item1}_{config.Item2}");
            if (core == null)
            {
                
                GameObject prefab = AssetManager.Instance.LoadPrefab(config.Item1, config.Item2);
                if (prefab == null)
                {
                    
                    Utils.Debug.LogError("UI", $"Prefab not found: {config.Item1}/{config.Item2}");
                    return null;
                }

                
                GameObject obj = Instantiate(prefab);
                obj.name = config.Item2;
                obj.transform.SetParent(root.transform);

                Canvas canvas = obj.AddComponent<Canvas>();
                canvas.overrideSorting = true;
                canvas.sortingOrder = config.Item3;

                core = obj.GetComponent<Core>();
                
                
                if (core == null)
                {
                    
                    Utils.Debug.LogError("UI", $"Prefab missing UI.Core component: {config.Item1}/{config.Item2}");
                    Destroy(obj);
                    return null;
                }

                
                core.Id = $"{config.Item1}_{config.Item2}";
                
                
                core.gameObject.AddComponent<GraphicRaycaster>();
                
                
                var imageComponent = core.gameObject.GetComponent<Image>();
                if (imageComponent == null)
                {
                    imageComponent = core.gameObject.AddComponent<Image>();
                }
                
                
                imageComponent.color = new Color(0f, 0f, 0f, config.Item4 ? 0f : 1f);
                core.ResetTransform(AnchorMin, AnchorMax);
                content.Add(core);
            }
            return core;
        }

        public void Open((string, string, int, bool) config, params object[] args)
        {
            Core ui = Get(config);
            if (ui == null)
            {
                Utils.Debug.LogError("UI", $"Failed to open UI: {config.Item1}/{config.Item2}");
                return;
            }

            if (!ui.isInit)
            {
                ui.isInit = true;
                ui.OnCreate(args);
            }
            ui.OnEnter(args);
            Current = config;
        }

        public void Close((string, string, int, bool) config) => Close($"{config.Item1}_{config.Item2}");

        public void Close(string id)
        {
            Core ui = content.FirstOrDefault(u => u.Id == id);
            if (ui != null)
            {
                ui.OnExit();
                ui.OnClose();
                Destroy(ui.gameObject);
                content.Remove(ui);
            }
        }

        public void Close()
        {
            content.Select(item => item.Id).ToList().ForEach(Close);
        }

        private void OnClick(params object[] args)
        {
            var canvas = root.GetComponent<Canvas>();
            var camera = canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : canvas.worldCamera ?? Camera.main;
            if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(root.GetComponent<RectTransform>(), Input.mousePosition, camera, out var pos)) return;
            Utils.Ring.Click(root, pos, this);
        }

        private void OnAfterTipChanged(params object[] args)
        {
            (Tips, string) tip = ((Tips, string))args[0];

            switch (tip.Item1)
            {
                case Tips.Fly:
                    ShowFlyTip(tip.Item2);
                    break;
                case Tips.Numerical:
                    ShowNumericalTip(tip.Item2);
                    break;
                case Tips.Attach:
                    ShowAttachTip(tip.Item2);
                    break;
            }
        }

        private void CalculateMaxFlyTips()
        {
            float effectiveHeight = Screen.height * (GoldenRatio + (1 - GoldenRatio) * GoldenRatio);
            maxFlyTips = Mathf.FloorToInt(effectiveHeight / UnitHeight);
            if (maxFlyTips < 1) maxFlyTips = 1;
        }

        private float GetTopMargin()
        {
            float effectiveHeight = maxFlyTips * UnitHeight;
            return (Screen.height - effectiveHeight) / 2f;
        }

        private float GetSlotPositionY(int slotIndex)
        {
            float topMargin = GetTopMargin();
            return Screen.height / 2f - topMargin - UnitHeight * slotIndex;
        }

        private void ShowFlyTip(string message)
        {
            CalculateMaxFlyTips();

            Transform flyTransform = root.transform.Find("Fly");

            GameObject fly = Instantiate(flyTransform.gameObject, root.transform);
            fly.SetActive(true);

            Text textComponent = fly.transform.Find("Text").GetComponent<Text>();
            textComponent.text = message;

            RectTransform tipRect = fly.GetComponent<RectTransform>();
            float bottomStartY = -Screen.height / 2f - UnitHeight;
            tipRect.anchoredPosition = new Vector2(0, bottomStartY);
            tipRect.localScale = Vector3.one;
            tipRect.sizeDelta = new Vector2(Screen.width, UnitHeight);

            Canvas flyCanvas = fly.GetComponent<Canvas>();
            if (flyCanvas == null)
            {
                flyCanvas = fly.AddComponent<Canvas>();
                flyCanvas.overrideSorting = true;
            }
            flyCanvas.sortingOrder = 10;

            GraphicRaycaster raycaster = fly.GetComponent<GraphicRaycaster>();
            if (raycaster == null)
            {
                raycaster = fly.AddComponent<GraphicRaycaster>();
            }

            if (flys.Count >= maxFlyTips)
            {
                RemoveOldestFlyTipWithAnimation();
            }

            flys.Add(fly);
            LayoutAllFlyTips(true);
        }

        private void LayoutAllFlyTips(bool animated)
        {
            ClearAllFlyTimers();

            int lastIndex = -1;
            for (int i = 0; i < flys.Count; i++)
            {
                if (flys[i] != null)
                {
                    lastIndex = i;
                }
            }

            for (int i = 0; i < flys.Count; i++)
            {
                if (flys[i] != null)
                {
                    RectTransform tipRect = flys[i].GetComponent<RectTransform>();
                    Text textComponent = flys[i].transform.Find("Text").GetComponent<Text>();
                    Image backgroundImage = flys[i].GetComponent<Image>();

                    float targetY = GetSlotPositionY(i);
                    int currentIndex = i;

                    tipRect.DOKill();
                    textComponent.DOKill();
                    textComponent.color = new Color(textComponent.color.r, textComponent.color.g, textComponent.color.b, 1f);
                    
                    if (backgroundImage != null)
                    {
                        backgroundImage.DOKill();
                        backgroundImage.color = new Color(backgroundImage.color.r, backgroundImage.color.g, backgroundImage.color.b, 1f);
                    }

                    if (animated)
                    {
                        tipRect.DOAnchorPosY(targetY, MoveSpeed).SetEase(Ease.Linear).OnComplete(() =>
                        {
                            if (currentIndex == lastIndex && flys.Count > 0)
                            {
                                StartAutoRemoveTimer();
                            }
                        });
                    }
                    else
                    {
                        tipRect.anchoredPosition = new Vector2(0, targetY);
                    }
                }
            }

            if (!animated && flys.Count > 0)
            {
                StartAutoRemoveTimer();
            }
        }

        private void StartAutoRemoveTimer()
        {
            ClearAllFlyTimers();
            
            if (flys.Count > 0)
            {
                Timer timer = Timer.Register(StayDuration, () =>
                {
                    if (flys.Count > 0 && flys[0] != null)
                    {
                        RemoveOldestFlyTipWithAnimation();
                        LayoutAllFlyTips(true);
                    }
                });
                flyTimers.Add(timer);
            }
        }

        private void ClearAllFlyTimers()
        {
            foreach (var timer in flyTimers)
            {
                if (timer != null)
                {
                    timer.Cancel();
                }
            }
            flyTimers.Clear();
        }

        private void RemoveOldestFlyTipWithAnimation()
        {
            if (flys.Count > 0)
            {
                GameObject oldest = flys[0];
                flys.RemoveAt(0);

                if (oldest != null)
                {
                    RectTransform tipRect = oldest.GetComponent<RectTransform>();
                    Text textComponent = oldest.transform.Find("Text").GetComponent<Text>();
                    Image backgroundImage = oldest.GetComponent<Image>();

                    float slideOutY = GetSlotPositionY(-1);

                    tipRect.DOKill();
                    textComponent.DOKill();
                    if (backgroundImage != null)
                    {
                        backgroundImage.DOKill();
                    }

                    Sequence seq = DOTween.Sequence();
                    seq.Append(tipRect.DOAnchorPosY(slideOutY, FadeSpeed).SetEase(Ease.Linear));
                    seq.Join(textComponent.DOFade(0, FadeSpeed));
                    if (backgroundImage != null)
                    {
                        seq.Join(backgroundImage.DOFade(0, FadeSpeed));
                    }
                    seq.OnComplete(() =>
                    {
                        if (oldest != null)
                        {
                            Destroy(oldest);
                        }
                    });
                }
            }
        }

        private void ShowNumericalTip(string jsonData)
        {
            GameObject numericalTip = root.transform.Find("Numerical").gameObject;

            JsonData jsonObject = JsonMapper.ToObject(jsonData);
            string text = jsonObject["text"].ToString();
            int start = Convert.ToInt32(jsonObject["start"].ToString());
            int end = Convert.ToInt32(jsonObject["end"].ToString());

            Color color = start < end ? Color.yellow : Color.red;
            Image image = numericalTip.GetComponent<Image>();
            Text titleText = numericalTip.transform.Find("Title").GetComponent<Text>();
            Text valueText = numericalTip.transform.Find("Value").GetComponent<Text>();
            titleText.color = Color.white;
            image.color = valueText.color = color;
            image.color = new Color(image.color.r, image.color.g, image.color.b, 0.25f);
            titleText.text = text;
            valueText.text = start.ToString();

            Canvas numericalCanvas = numericalTip.GetComponent<Canvas>();
            if (numericalCanvas == null)
            {
                numericalCanvas = numericalTip.AddComponent<Canvas>();
                numericalCanvas.overrideSorting = true;
            }
            numericalCanvas.sortingOrder = 10;

            numericalTip.SetActive(true);

            DOTween.To(() => start, x => valueText.text = x.ToString(), end, 1)
                   .SetEase(Ease.Linear)
                   .OnComplete(() => FadeOutNumericalTip(1));
        }

        private void FadeOutNumericalTip(float duration)
        {
            GameObject numericalTip = root.transform.Find("Numerical").gameObject;

            Text titleText = numericalTip.transform.Find("Title").GetComponent<Text>();
            Text valueText = numericalTip.transform.Find("Value").GetComponent<Text>();
            Image image = numericalTip.GetComponent<Image>();
            titleText.DOFade(0, duration).OnComplete(() => OnNumericalTipFadeComplete());
            valueText.DOFade(0, duration);
            image.DOFade(0, duration);
        }

        private void OnNumericalTipFadeComplete()
        {
            GameObject numericalTip = root.transform.Find("Numerical").gameObject;

            numericalTip.SetActive(false);

            Text titleText = numericalTip.transform.Find("Title").GetComponent<Text>();
            Text valueText = numericalTip.transform.Find("Value").GetComponent<Text>();
            Image image = numericalTip.GetComponent<Image>();

            Color titleColor = titleText.color;
            titleColor.a = 1f;
            titleText.color = titleColor;

            Color valueColor = valueText.color;
            valueColor.a = 1f;
            valueText.color = valueColor;

            Color imageColor = image.color;
            imageColor.a = 0.25f;
            image.color = imageColor;
        }

        private void ShowAttachTip(string jsonData)
        {
            GameObject attachTip = root.transform.Find("Attach").gameObject;

            if (string.IsNullOrEmpty(jsonData))
            {
                attachTip.SetActive(false);
                return;
            }

            JsonData json = JsonMapper.ToObject(jsonData);
            string text = json["text"].ToString();
            JsonData screen = json["screen"];
            Vector2 screenPos = new Vector2(float.Parse(screen[0].ToString()), float.Parse(screen[1].ToString()));

            var textComponent = attachTip.transform.Find("Text").GetComponent<Text>();
            textComponent.text = text;

            float paddingX = 40f;
            float paddingY = 20f;
            float width = textComponent.preferredWidth + paddingX;
            float height = textComponent.preferredHeight + paddingY;

            var tipRect = attachTip.GetComponent<RectTransform>();
            tipRect.sizeDelta = new Vector2(width, height);

            RectTransform canvasRect = root.GetComponent<RectTransform>();
            Utils.Pos.Attach(canvasRect, tipRect, screenPos, out Vector2 localPos, out Vector2 pivot);
            tipRect.pivot = pivot;
            tipRect.anchoredPosition = localPos;

            Canvas attachCanvas = attachTip.GetComponent<Canvas>();
            if (attachCanvas == null)
            {
                attachCanvas = attachTip.AddComponent<Canvas>();
                attachCanvas.overrideSorting = true;
            }
            attachCanvas.sortingOrder = 10;

            attachTip.SetActive(true);
        }

        private void OnAfterLoginAccountChanged(params object[] args)
        {
            Account account = (Account)args[0];
            if (account != null)
            {
                loginDarkTimer?.Cancel();
                loginDarkTimer = Timer.Register(1f, () => 
                {
                    Open(Config.UI.Dark, Localization.Instance.Get("connecting"));
                });
            }
        }

        private void OnAfterOnlineChanged(params object[] args)
        {
            bool onLine = (bool)args[0];
            if (!onLine)
            {
                loginDarkTimer?.Cancel();
                Close(Config.UI.Dark);
                Close(Config.UI.Home);
                Close(Config.UI.Initialize);
                Close(Config.UI.Option);
                Close(Config.UI.Story);
                Close(Config.UI.Tutorial);
            }
        }

        private void OnAfterInitializeChanged(params object[] args)
        {
            loginDarkTimer?.Cancel();
            Close(Config.UI.Dark);
            Open(Config.UI.Initialize);
        }

        private void OnAfterHomeChanged(params object[] args)
        {
            loginDarkTimer?.Cancel();
            Close(Config.UI.Dark);
            Open(Config.UI.Home);
        }

        private void OnAfterOptionChanged(params object[] args)
        {
            Protocol.Option option = (Protocol.Option)args[0];
            if (option != null && !option.Empty)
            {
                Open(Config.UI.Option);
            }
            else
            {
                Close(Config.UI.Option);
            }
        }

        private void OnAfterStoryChanged(params object[] args)
        {
            Open(Config.UI.Story);
        }
        private void OnAfterDataDark(params object[] args)
        {
            string? v = (string?)args[0];
            if (v != null)
            {
                Open(Config.UI.Dark, v);
            }
            else
            {
                loginDarkTimer?.Cancel();
                Close(Config.UI.Dark);
            }
        }
        private void OnAfterTutorialIndexChanged(params object[] args)
        {
            Open(Config.UI.Tutorial);
        }

        private void OnAfterTutorialStepChanged(params object[] args)
        {
            var step = args[0] as Protocol.Tutorial;
            if (step != null)
            {
                Open(Config.UI.Tutorial);
            }
            else
            {
                Close(Config.UI.Tutorial);
            }
        }
    }
}