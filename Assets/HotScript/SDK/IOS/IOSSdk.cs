using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
#if UNITY_IPHONE
namespace Game
{
    public class IOSSdk : BaseSdk
    {
        //[DllImport("__Internal")]
        //public static extern string _CallPlatform(string data);

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
