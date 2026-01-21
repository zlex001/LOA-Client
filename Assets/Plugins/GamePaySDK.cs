using System;
using UnityEngine;

// gamoobject 名字必须叫GamePaySDK
public class GamePaySDK : MonoBehaviour
{
    AndroidJavaClass alipayHelper;
    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        alipayHelper = new AndroidJavaClass("com.unity3d.player.AlipayHelper");
        
        // 获取当前Activity并初始化AlipayHelper
        using (AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
        {
            AndroidJavaObject currentActivity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
            alipayHelper.CallStatic("init", currentActivity);
        }
    }

    public void AliPay(string orderInfo)
    {
        alipayHelper.CallStatic("pay", orderInfo);
    }

    public void AliPayResult(string result)
    {
        Debug.Log("AliPayResult: " + result);
    }
}