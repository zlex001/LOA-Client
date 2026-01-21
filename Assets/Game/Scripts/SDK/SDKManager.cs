using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Game
{
    public class SDKManager : Singleton<SDKManager>
    {
        BaseSdk sdk;
        public void Init()
        {
#if UNITY_ANDROID
            sdk = new AndroidSdk();
#elif UNITY_IPHONE
                sdk = new IOSSdk();
#else
                sdk = new WindowsSdk();
#endif
        }
        /// <summary>
        /// ��ȡ�ֻ����
        /// </summary>
        /// <returns></returns>
        public float GetBattery()
        {
            return SystemInfo.batteryLevel;
        }

        /// <summary>
        /// ��ȡ���а�����
        /// </summary>
        /// <returns></returns>
        public string GetClipBoard()
        {
            return GUIUtility.systemCopyBuffer;
        }
    }
}