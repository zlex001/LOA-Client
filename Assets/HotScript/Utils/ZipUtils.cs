using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Framework;

namespace Game
{
    public class ZipUtils
    {
        public static void UnZipByByte(byte[] zipData, string destPath, Action<bool, float, string> statusAction)
        {
            //ZipSeven zip = new ZipSeven();
            ZipSharp zip = new ZipSharp();
            zip.UnZipByByte(zipData, destPath, statusAction);
        }
    }
}