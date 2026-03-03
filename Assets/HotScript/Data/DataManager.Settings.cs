using UnityEngine;

namespace Game.Data
{
    public partial class DataManager
    {
        #region Settings Properties
        public Languages Language { get => Get<Languages>(Type.Language); set => Change(Type.Language, value); }
        public int FontSize { get => Get<int>(Type.FontSize); set => Change(Type.FontSize, value); }
        public float SceneScale { get => Get<float>(Type.SceneScale); set => Change(Type.SceneScale, value); }
        public float SceneContainerSize { get; set; } = 600f;
        public float SceneScaleBeforeWorldMap { get; set; } = 1.0f;
        #endregion

        #region Settings Monitor Handlers

        private void OnAfterLanguageChanged(params object[] args)
        {
            Languages language = (Languages)args[0];
            User.Language = language.ToString();
            PlayerPrefs.SetString("LANGUAGE", language.ToString());
            PlayerPrefs.Save();
            Local.Instance.Save(User);
        }

        private void OnAfterFontSizeChanged(params object[] args)
        {
            int size = (int)args[0];
            User.FontSize = size;
            Local.Instance.Save(User);
            Framework.FontScaler.NotifyFontSizeChanged(size);
        }

        private void OnHomeSceneZoomIn(params object[] args)
        {
            float newScale = SceneScale - 1f;
            if (newScale < MinSceneScale)
            {
                newScale = MinSceneScale;
            }
            User.SceneScale = SceneScale = newScale;
            Local.Instance.Save(User);
        }

        private void OnHomeSceneZoomOut(params object[] args)
        {
            float maxScale = GetMaxSceneScale();
            float newScale = SceneScale + 1f;
            if (newScale > maxScale)
            {
                newScale = maxScale;
            }
            User.SceneScale = SceneScale = newScale;
            Local.Instance.Save(User);
        }

        public float GetMaxSceneScale()
        {
            return SceneContainerSize / MinCellSize;
        }

        #endregion
    }
}
