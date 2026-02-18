using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Game.SDK
{
    public abstract class BaseSdk
    {
        /// <summary>
        /// 设置剪切板
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public abstract void SetClipBoard(string data);
        /// <summary>
        /// 获取剪切板
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public abstract string GetClipBoard();

    }
}