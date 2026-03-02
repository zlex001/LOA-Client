using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Game.Utils
{
    public static class RectTransformUtils
    {
        /// <summary>
        /// ��Ļ����ת���� UI ����
        /// </summary>
        /// <param name="targetParentRect"> Ŀ�� UI ������� RectTransform </param>
        /// <param name="mousePos"> ���λ�� </param>
        /// <param name="canvasCam"> ���Canvas����ȾģʽΪ: Screen Space - Overlay, Camera ����Ϊ null;
        /// Screen Space-Camera or World Space, Camera ����Ϊ Camera.main></param>
        /// <returns>UI �ľֲ�����</returns>
        public static Vector2 ScreenPointToLocalPointInRectangle(RectTransform parentTransform, Vector3 screenPos, Camera uiCamera = null)
        {
            Vector2 locPos;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(parentTransform, screenPos, uiCamera, out locPos);
            return locPos;
        }

        /// <summary>
        /// ��Ļ����ת���� UI ����
        /// </summary>
        /// <param name="targetRect"> Ŀ�� UI ����� RectTransform </param>
        /// <param name="mousePos"> ���λ�� </param>
        /// <param name="canvasCam"> ���Canvas����ȾģʽΪ: Screen Space - Overlay, Camera ����Ϊ null;
        /// Screen Space-Camera or World Space, Camera ����Ϊ Camera.main></param>
        /// <returns> UI ������ </returns>
        public static Vector3 ScreenToUIWorldPos(RectTransform targetRect, Vector2 mousePos, Camera canvasCam = null)
        {
            //UI �ľֲ�����
            Vector3 worldPos;
            RectTransformUtility.ScreenPointToWorldPointInRectangle(targetRect, mousePos, canvasCam, out worldPos);
            return worldPos;
        }

    }
}