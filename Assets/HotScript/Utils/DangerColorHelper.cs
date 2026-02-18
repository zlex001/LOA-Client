using UnityEngine;

namespace Game.Utils
{
    public static class DangerColorHelper
    {
        public static Color GetDangerColor(int current, int max)
        {
            if (max <= 0) return Color.white;
            
            float ratio = (float)current / max;
            
            if (ratio <= 0.2f)
            {
                return ColorUtility.TryParseHtmlString("#D43B2AFF", out Color color) ? color : Color.red;
            }
            else if (ratio <= 0.5f) 
            {
                return ColorUtility.TryParseHtmlString("#E69500FF", out Color color) ? color : new Color(1f, 0.65f, 0f, 1f);
            }
            else if (ratio <= 0.8f)
            {
                return ColorUtility.TryParseHtmlString("#6AA2E8FF", out Color color) ? color : new Color(0f, 0.5f, 1f, 1f);
            }
            else
            {
                return ColorUtility.TryParseHtmlString("#70CC70FF", out Color color) ? color : Color.green;
            }
        }
        
        public static Color GetDangerColor(float ratio)
        {
            if (ratio <= 0.2f)
            {
                return ColorUtility.TryParseHtmlString("#D43B2AFF", out Color color) ? color : Color.red;
            }
            else if (ratio <= 0.5f) 
            {
                return ColorUtility.TryParseHtmlString("#E69500FF", out Color color) ? color : new Color(1f, 0.65f, 0f, 1f);
            }
            else if (ratio <= 0.8f)
            {
                return ColorUtility.TryParseHtmlString("#6AA2E8FF", out Color color) ? color : new Color(0f, 0.5f, 1f, 1f);
            }
            else
            {
                return ColorUtility.TryParseHtmlString("#70CC70FF", out Color color) ? color : Color.green;
            }
        }
    }
}
