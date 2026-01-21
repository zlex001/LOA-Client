//using Framework;
//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;

//public class EditorStringUtils
//{

//    public static string AlignmentFormat(string content, string symbol)
//    {
//        string str = StringUtils.Remove(content, "\t", " ");
//        string[] strArr = str.Split('\n');

//        int maxSymbolIndex = 0;
//        for (int i = 0; i < strArr.Length; i++)
//        {
//            int index = strArr[i].IndexOf(symbol);
//            if (maxSymbolIndex <= index)
//            {
//                maxSymbolIndex = index;
//            }
//        }

//        for (int i = 0; i < strArr.Length; i++)
//        {
//            int index = strArr[i].IndexOf(symbol);
//            if (index > 0)
//            {
//                int spaceCount = maxSymbolIndex - index + 2;

//                string space = "";
//                for (int j = 0; j < spaceCount; j++)
//                {
//                    space += " ";
//                }

//                strArr[i] = strArr[i].Insert(index, space);
//                strArr[i] = strArr[i].Insert(index + space.Length + 1, "  ");
//            }
//        }

//        string content2 = "";
//        for (int i = 0; i < strArr.Length; i++)
//        {
//            content2 += "\t" + strArr[i] + "\n";
//        }
//        return content2;
//    }
//}
