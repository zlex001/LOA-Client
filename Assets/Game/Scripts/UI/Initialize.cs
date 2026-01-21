using Framework;
using Game.Protocol;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace Game
{
    public class Initialize : UI.Core
    {
        #region Enums and Constants
        private const float UnitHeight = 83f;
        private const float GoldenRatio = 0.618f;

        public enum Click
        {
            Random,
            Confirm,
        }
        #endregion

        #region Fields
        private Dictionary<string, string> _uiTexts;
        private float _radarMeshScale;
        #endregion

        #region Lifecycle Methods
        public override void OnCreate(params object[] args)
        {
            // Setup UI event listeners
            transform.Find("Name").GetComponent<InputField>().onEndEdit.AddListener(OnNameEndEdit);
            transform.Find("Confirm").GetComponent<Button>().onClick.AddListener(OnConfirmClick);
            transform.Find("Random").GetComponent<Button>().onClick.AddListener(OnRandomClick);

            // Register global events
            Data.Instance.after.Register(Data.Type.InitialResponse, AfterInitialResponseChange);

            ApplyAbsoluteLayout();
        }

        public override void OnEnter(params object[] args)
        {
            ApplyAbsoluteLayout();

            transform.Find("Description").GetComponent<Text>().text = Data.Instance.Initialize.description;
            FreshRadar(Data.Instance.Initialize.grade);
            
            if (Data.Instance.Initialize.ui != null)
            {
                var ui = Data.Instance.Initialize.ui;
                
                var namePlaceholder = transform.Find("Name/Placeholder")?.GetComponent<Text>();
                if (namePlaceholder != null && !string.IsNullOrEmpty(ui.namePlaceholder))
                    namePlaceholder.text = ui.namePlaceholder;
                
                var randomText = transform.Find("Random/Text")?.GetComponent<Text>();
                if (randomText != null && !string.IsNullOrEmpty(ui.randomButton))
                    randomText.text = ui.randomButton;
                
                var confirmText = transform.Find("Confirm/Text")?.GetComponent<Text>();
                if (confirmText != null && !string.IsNullOrEmpty(ui.confirmButton))
                    confirmText.text = ui.confirmButton;
                
                _uiTexts = new Dictionary<string, string>
                {
                    { "errorNameEmpty", ui.errorNameEmpty },
                    { "errorNameUnsafe", ui.errorNameUnsafe }
                };
            }
        }

        public override void OnScreenAdaptationChanged()
        {
            ApplyAbsoluteLayout();
        }

        private void ApplyAbsoluteLayout()
        {
            float screenWidth = GetComponent<RectTransform>().rect.width;
            float screenHeight = GetComponent<RectTransform>().rect.height;

            float panelWidth = screenWidth * GoldenRatio;
            float radarSize = UnitHeight * 5;
            float nameHeight = UnitHeight;
            float descriptionHeight = UnitHeight;
            float buttonHeight = UnitHeight;

            float nameToDescriptionGap = UnitHeight ;
            float descriptionToRadarGap = UnitHeight ;
            float radarToButtonsGap = UnitHeight * 2f;

            float contentHeight = nameHeight + nameToDescriptionGap + 
                                  descriptionHeight + descriptionToRadarGap + 
                                  radarSize + radarToButtonsGap + 
                                  buttonHeight;

            float remainingSpace = screenHeight - contentHeight;
            float topMargin = remainingSpace / 2;
            float startY = screenHeight - topMargin;

            float nameY = startY - nameHeight / 2;
            float descriptionY = nameY - nameHeight / 2 - nameToDescriptionGap - descriptionHeight / 2;
            float radarCenterY = descriptionY - descriptionHeight / 2 - descriptionToRadarGap - radarSize / 2;
            float buttonsY = radarCenterY - radarSize / 2 - radarToButtonsGap - buttonHeight / 2;

            var nameRect = transform.Find("Name")?.GetComponent<RectTransform>();
            if (nameRect != null)
            {
                nameRect.anchorMin = Vector2.zero;
                nameRect.anchorMax = Vector2.zero;
                nameRect.pivot = new Vector2(0.5f, 0.5f);
                nameRect.sizeDelta = new Vector2(panelWidth, nameHeight);
                nameRect.anchoredPosition = new Vector2(screenWidth / 2, nameY);
            }


            var descriptionRect = transform.Find("Description")?.GetComponent<RectTransform>();
            if (descriptionRect != null)
            {
                descriptionRect.anchorMin = Vector2.zero;
                descriptionRect.anchorMax = Vector2.zero;
                descriptionRect.pivot = new Vector2(0.5f, 0.5f);
                descriptionRect.sizeDelta = new Vector2(panelWidth, descriptionHeight);
                descriptionRect.anchoredPosition = new Vector2(screenWidth / 2, descriptionY);
            }

            var radarRect = transform.Find("Radar")?.GetComponent<RectTransform>();
            if (radarRect != null)
            {
                radarRect.anchorMin = Vector2.zero;
                radarRect.anchorMax = Vector2.zero;
                radarRect.pivot = new Vector2(0.5f, 0.5f);
                radarRect.sizeDelta = new Vector2(radarSize, radarSize);
                radarRect.anchoredPosition = new Vector2(screenWidth / 2, radarCenterY);
                _radarMeshScale = radarSize / 80f;
            }

            float buttonWidth = panelWidth * GoldenRatio * 0.5f;
            float buttonGap = UnitHeight * 0.5f;
            float panelLeft = screenWidth / 2 - panelWidth / 2;
            float panelRight = screenWidth / 2 + panelWidth / 2;

            var randomRect = transform.Find("Random")?.GetComponent<RectTransform>();
            if (randomRect != null)
            {
                randomRect.anchorMin = Vector2.zero;
                randomRect.anchorMax = Vector2.zero;
                randomRect.pivot = new Vector2(0.5f, 0.5f);
                randomRect.sizeDelta = new Vector2(buttonWidth, buttonHeight);
                randomRect.anchoredPosition = new Vector2(panelLeft + buttonWidth / 2, buttonsY);
            }

            var confirmRect = transform.Find("Confirm")?.GetComponent<RectTransform>();
            if (confirmRect != null)
            {
                confirmRect.anchorMin = Vector2.zero;
                confirmRect.anchorMax = Vector2.zero;
                confirmRect.pivot = new Vector2(0.5f, 0.5f);
                confirmRect.sizeDelta = new Vector2(buttonWidth, buttonHeight);
                confirmRect.anchoredPosition = new Vector2(panelRight - buttonWidth / 2, buttonsY);
            }
        }

        public override void OnClose()
        {
            Data.Instance.after.Unregister(Data.Type.InitialResponse, AfterInitialResponseChange);
        }
        #endregion

        #region UI Event Handlers
        private void OnRandomClick()
        {
            Game.Event.Instance.Fire(Click.Random);
        }

        private void OnConfirmClick()
        {
            string checkResult = NameCheck;
            if (checkResult == "")
            {
                Game.Event.Instance.Fire(Click.Confirm, transform.Find("Name").GetComponent<InputField>().text);
            }
            else
            {
                Data.Instance.Tip = (UI.Tips.Fly, checkResult);
            }
        }

        private void OnNameEndEdit(string value)
        {
        }
        #endregion

        #region Validation
        private string NameCheck
        {
            get
            {
                string name = transform.Find("Name").transform.GetComponent<InputField>().text;
                
                if (string.IsNullOrEmpty(name))
                    return _uiTexts != null && _uiTexts.ContainsKey("errorNameEmpty") ? _uiTexts["errorNameEmpty"] : "";
                
                if (!System.Text.RegularExpressions.Regex.IsMatch(name, @"^[\u4e00-\u9fa5a-zA-Z0-9]*$"))
                    return _uiTexts != null && _uiTexts.ContainsKey("errorNameUnsafe") ? _uiTexts["errorNameUnsafe"] : "";
                
                return "";
            }
        }
        #endregion

        #region Radar Visualization
        private void FreshRadar(Dictionary<string, int> grade)
        {
            // Setup radar attribute labels and values
            for (int i = 0; i < grade.Count; i++)
            {
                var g = grade.ElementAt(i);
                transform.Find($"Radar/{i}/Title").GetComponent<Text>().text = g.Key;
                transform.Find($"Radar/{i}/Value").GetComponent<Text>().text = g.Value.ToString();
            }

            // Get radar UI components
            var radarTransform = transform.Find("Radar");
            Material material = radarTransform.GetComponent<Image>().material;
            Texture texture = radarTransform.GetComponent<Image>().sprite.texture;
            CanvasRenderer render = radarTransform.GetComponent<CanvasRenderer>();

            // Use pre-calculated radar mesh scale
            float radarSize = _radarMeshScale > 0 ? _radarMeshScale : 8f;

            // Get data values
            List<int> dataList = grade.Values.ToList();

            // Build radar mesh if we have enough data points
            if (dataList.Count >= 5)
            {
                int _length = dataList.Count;
                Mesh mesh = new Mesh();
                Vector3[] vertices = new Vector3[_length + 1];
                Vector2[] uv = new Vector2[_length + 1];
                int[] triangles = new int[3 * _length];

                // Calculate vertex positions
                float angleIncrement = 360f / _length;
                vertices[0] = Vector3.zero;
                for (int i = 1; i <= _length; i++)
                {
                    vertices[i] = Quaternion.Euler(0, 0, -angleIncrement * (i - 1)) * Vector3.up * radarSize * dataList[i - 1];
                }

                // Set UV coordinates
                uv[0] = Vector2.zero;
                for (int i = 1; i <= _length; i++)
                {
                    uv[i] = Vector2.one;
                }

                // Set triangle indices
                for (int i = 0; i < _length * 3; i++)
                {
                    if (i % 3 == 0)
                    {
                        triangles[i] = 0;
                    }
                    else
                    {
                        if ((i - 1) % 3 == 0)
                        {
                            triangles[i] = (i - 1) / 3 + 1;
                        }
                        else
                        {
                            triangles[i] = (i - 2) / 3 + 2;
                        }
                    }
                    // Last triangle index is 1
                    if (i == _length * 3 - 1)
                    {
                        triangles[i] = 1;
                    }
                }

                // Apply mesh to renderer
                mesh.vertices = vertices;
                mesh.uv = uv;
                mesh.triangles = triangles;
                render.SetMesh(mesh);
                render.SetMaterial(material, texture);
            }
        }
        #endregion

        #region Response Handlers
        private void AfterInitialResponseChange(params object[] args)
        {
            int v = (int)args[0];
            InitialResponse.Code code = (InitialResponse.Code)v;
            
            if (code == InitialResponse.Code.Success)
            {
                Close();
            }
            else
            {
                Data.Instance.Dark = null;
                
                string message = Data.Instance.InitialResponseMessage;
                if (!string.IsNullOrEmpty(message))
                {
                    Data.Instance.Tip = (UI.Tips.Fly, message);
                }
            }
        }
        #endregion
    }
}