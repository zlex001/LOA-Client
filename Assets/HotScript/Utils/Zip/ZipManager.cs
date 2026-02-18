//using System;
//using System.Collections;
//using System.Collections.Generic;
//using System.IO;
//using UnityEngine;
//using UnityEngine.Networking;
//namespace Framework
//{
//    public class ZipManager : SingletonGameObject<ZipManager>
//    {
//        /// <summary>
//        /// 本地版本号
//        /// </summary>
//        private string version;

//        /// <summary>
//        /// 本地文件名
//        /// </summary>
//        private string fileName;

//        /// <summary>
//        /// 初始化
//        /// </summary>
//        /// <param name="version"></param>
//        public void Init(string version, string fileName)
//        {
//            this.version = version;
//            this.fileName = fileName;
//        }

//        /// <summary>
//        /// 是否是第一次安装，是否需要解压缩
//        /// </summary>
//        public bool IsNeedUnZip()
//        {
//            if (!PlayerPrefs.HasKey("VERSION"))
//            {
//                PlayerPrefs.SetString("VERSION", "");
//            }

//            string version = PlayerPrefs.GetString("VERSION");

//            return this.version != version;
//        }

//        /// <summary>
//        /// 保存zip版本号
//        /// </summary>
//        public void SaveZipVersion()
//        {
//            PlayerPrefs.SetString("VERSION", version);
//        }
//        /// <summary>
//        /// 开始解压缩
//        /// </summary>
//        /// <param name="action">是否完成，完成进度，是否有错</param>
//        public void StartUnZip(Action<bool, float, string> action)
//        {
//            //获取本地zip文件
//            StartCoroutine(FileUtils.LoadNativeFileIEnumerator(fileName, delegate (bool isSuccess, byte[] data)
//            {
//                if (isSuccess)
//                {
//                    //打开解压界面，开始解压缩
//                    ZipUtils.UnZipByByte(data, PathUtils.GetRuntimePath(), action);
//                }
//                else
//                {
//                    //弹出解压失败的界面，重新开始解压或者重新下载安装包
//                    action(false, 0, "error");
//                    //清理工作环境
//                }
//            }));
//        }
//    }
//}