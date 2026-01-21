using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_ANDROID
namespace Game
{
    public class AndroidSdk : BaseSdk
    {
        private string className = "ethlua2.Ethlua2";
        AndroidJavaClass javaStaticClass;
        public AndroidSdk()
        {
            //javaStaticClass = new AndroidJavaClass(className);
        }

        public override string GetClipBoard()
        {
            throw new System.NotImplementedException();
        }

        public override void SetClipBoard(string data)
        {
            throw new System.NotImplementedException();
        }
    }
}
#endif