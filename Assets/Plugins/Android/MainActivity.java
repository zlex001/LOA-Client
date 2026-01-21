// package com.unity3d.player;

// import android.os.Bundle;
// import android.util.Log;

// import com.alipay.sdk.app.PayTask;

// public class MainActivity extends UnityPlayerActivity {
//     @Override
//     protected void onCreate(Bundle savedInstanceState) {
//         super.onCreate(savedInstanceState);
//         Log.i("Unity", "项目启动");
//     }

//     public void AliPayByApp(final String orderInfo){
//         Runnable payRun=new Runnable() {
//             public void run() {
//                 PayTask task=new PayTask(MainActivity.this);
//                 String result= task.pay(orderInfo, true);
//                 //支付回调
//                 UnityPlayer.UnitySendMessage("GamePaySDK", "ALiPayResult", result);
//             }
//         };
//         Thread payThread = new Thread(payRun);
//         payThread.start();
//     }
// }