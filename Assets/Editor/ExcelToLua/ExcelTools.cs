//using ExcelDataReader;
//using Framework;
//using System;
//using System.Collections.Generic;
//using System.Data;
//using System.Globalization;
//using System.IO;
//using System.Linq;
//using UnityEditor;
//using UnityEngine;

//public class ExcelTools : Editor
//{
//    [MenuItem("Tools/生成ExcelLua文件")]
//    public static void Start()
//    {
//        string path = Path.Utils + "/Excel";
//#if UNITY_EDITOR_WIN        
//        //DirectoryUtils.CopyDirectory("D:/Project/GameWord/游戏/测试表", path);
//        Framework.Directory.CopyDirectory("D:/Project/GameWord/游戏/配置表", path);
//        System.IO.Directory.CreateDirectory(Path.Scripts + "/Data/Excel/");
//#endif
//        foreach (var item in Framework.File.GetFiles(path))
//        {
//            Debug.Log("item" + item);
//            if (item.Contains(".DS_Store") || item.Contains("$"))
//            {
//                continue;
//            }
//            Do(item);
//        }
//    }
//    public class ExcelConfig
//    {
//        public string name;
//        public string type;
//    }

//    private static void Do(string path)
//    {
//        List<ExcelConfig> list = new List<ExcelConfig>();
//        List<List<string>> valueList = new List<List<string>>();
//        using (var stream = System.IO.File.Open(path, FileMode.Open, FileAccess.Read))
//        {
//            // Auto-detect format, supports:
//            //  - Binary Excel files (2.0-2003 format; *.xls)
//            //  - OpenXml Excel files (2007 format; *.xlsx, *.xlsb)
//            using (var reader = ExcelReaderFactory.CreateReader(stream))
//            {
//                // Choose one of either 1 or 2:

//                // 1. Use the reader methods
//                do
//                {
//                    while (reader.Read())
//                    {
//                        // reader.GetDouble(0);
//                    }
//                } while (reader.NextResult());

//                // 2. Use the AsDataSet extension method
//                DataSet result = reader.AsDataSet();

//                int columns = result.Tables[0].Columns.Count;
//                int rows = result.Tables[0].Rows.Count;

//                //Debug.Log(columns);
//                //Debug.Log(rows);


//                for (int i = 0; i < rows; i++)
//                {
//                    DataRow dataRow = result.Tables[0].Rows[i];
//                    List<string> cellValue = new List<string>();

//                    for (int j = 0; j < columns; j++)
//                    {
//                        string value = dataRow[j].ToString();

//                        if (i == 0)
//                        {
//                            list.Add(new ExcelConfig());
//                        }
//                        else if (i == 1)
//                        {
//                            list[j].type = value;
//                        }
//                        else if (i == 2)
//                        {
//                            list[j].name = value;
//                        }
//                        else
//                        {
//                            cellValue.Add(value);
//                        }
//                    }
//                    if (cellValue.Count > 0)
//                    {
//                        valueList.Add(cellValue);
//                    }
//                }

//            }
//        }

//        string fileName = System.IO.Path.GetFileNameWithoutExtension(path);

//        string content = "ExcelData =	ExcelData or {}\n";
//        content += "ExcelData." + fileName + " =\n";
//        content += GetContent(list, valueList);

//        string funcContent = GetFunctionContent(fileName, list);

//        funcContent += GetKeyListContent(fileName, valueList, list[0].type == "string");
//        funcContent += GetCountListContent(fileName, valueList.Count);
//        content += funcContent;

//        System.IO.File.WriteAllText(Path.Scripts + "/Data/Excel/Export/" + fileName + ".lua", content);

//        Debug.Log("excel fileName : " + fileName + "写入成功");
//    }

//    private static string GetCountListContent(string fileName, int excelSize)
//    {
//        string tempStr = @"
//ExcelData.fileName.Count = function ()
//	return excelSize
//end";
//        return tempStr.Replace("fileName", fileName).Replace("excelSize", excelSize + "");
//    }



//    private static string GetKeyListContent(string fileName, List<List<string>> valueList, bool isstring)
//    {
//        string tempStr = @"
//ExcelData.fileName.KeyList = function ()
//	return {allkey}
//end";

//        string allContent = "";

//        for (int i = 0; i < valueList.Count; i++)
//        {
//            for (int j = 0; j < valueList[i].Count; j++)
//            {
//                if (j == 0)
//                {
//                    if (isstring)
//                    {
//                        allContent += AppendString(valueList[i][j]) + ",";
//                    }
//                    else
//                    {
//                        allContent += valueList[i][j] + ",";
//                    }
//                }
//            }
//        }
//        allContent = allContent.TrimEnd(',');

//        return tempStr.Replace("fileName", fileName).Replace("allkey", allContent);
//    }
//    private static string GetFunctionContent(string fileName, List<ExcelConfig> confgList)
//    {
//        string tempStr = @"
//ExcelData.fileName.GetpfuncName = function (key)
//	return ExcelData.fileName[key].funcName
//end";

//        string allContent = "";
//        for (int i = 0; i < confgList.Count; i++)
//        {
//            if (i > 0)
//            {
//                string content = tempStr.Replace("fileName", fileName);

//                content = content.Replace("pfuncName", StringUtils.FirstLetterToUpper(confgList[i].name));

//                content = content.Replace("funcName", confgList[i].name);

//                allContent += content;
//            }
//        }
//        return allContent;
//    }
//    private static string GetContent(List<ExcelConfig> confgList, List<List<string>> lineList)
//    {
//        List<string> contentList = new List<string>();

//        for (int i = 0; i < lineList.Count; i++)
//        {
//            string lineContent = "";

//            List<string> valueList = lineList[i];
//            for (int j = 0; j < valueList.Count; j++)
//            {
//                string value = valueList[j];

//                if (j == 0)
//                {
//                    lineContent += "[";


//                    if (confgList[j].type == "string")
//                    {
//                        lineContent += AppendString(value);
//                    }
//                    else
//                    {
//                        lineContent += value;
//                    }


//                    lineContent += "] = ";

//                    lineContent += "{";
//                }
//                else
//                {

//                    if (confgList[j].type == "string")
//                    {
//                        if (string.IsNullOrEmpty(value))
//                        {
//                            value = "null";
//                        }
//                        lineContent += confgList[j].name + " = " + AppendString(value) + ",";
//                    }
//                    else
//                    {
//                        if (string.IsNullOrEmpty(value))
//                        {
//                            lineContent += confgList[j].name + " = " + AppendString("null") + ",";
//                        }
//                        else
//                        {
//                            lineContent += confgList[j].name + " = " + value + ",";
//                        }
//                    }
//                }

//                if (j == valueList.Count - 1)
//                {
//                    lineContent = lineContent.TrimEnd(',');
//                    lineContent += "}";
//                }
//            }

//            contentList.Add(lineContent);

//            //Debug.Log("lineContent : " + lineContent);

//        }
//        //Debug.Log(GetContent(contentList));
//        return GetContent(contentList);
//    }

//    private static string GetContent(List<string> contentList)
//    {
//        string content = "{\n";

//        for (int i = 0; i < contentList.Count; i++)
//        {
//            string value = contentList[i];
//            content += "\t" + value + "," + "\n";
//        }
//        content += "}";
//        return content;
//    }
//    private static string AppendString(string str)
//    {
//        return "\"" + str + "\"";
//    }

//}