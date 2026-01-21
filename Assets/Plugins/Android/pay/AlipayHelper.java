package com.unity3d.player;

import android.app.Activity;
import android.os.Bundle;
import android.util.Log;

import com.alipay.sdk.app.PayTask;

public class AlipayHelper
{
    private static Activity mActivity;

    public static void init(Activity activity) {
        mActivity = activity;
    }


    public static void pay(final String orderInfo)
    {
        Log.i("Unity", "调用支付宝支付");
        Runnable payRun = new Runnable()
        {
            public void run()
            {
                PayTask task = new PayTask(mActivity);
                String result = task.pay(orderInfo, true);
                UnityPlayer.UnitySendMessage("GamePaySDK", "ALiPayResult", result);
            }
        };
        Thread payThread = new Thread(payRun);
        payThread.start();
    }
}