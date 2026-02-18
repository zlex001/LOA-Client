using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Framework;
using HybridCLR.Editor;
using UnityEngine;
using UnityEditor;

public class Build
{
    public static string tempPath;
    public static string abOutPath;
    public static string allOutPath;
    public static string packageTempPath;
    public static string resPath;
    public static string srcPath;

    public static void Init()
    {
        tempPath = Path.Artifacts + "/" + "TEMP";
        abOutPath = Path.Artifacts + "/" + "ABOUT";
        allOutPath = Path.Artifacts + "/" + "ALLOUT" + "/HotUpdate/" + Path.PlatformName;
        packageTempPath = Path.Artifacts + "/" + "PACKAGETEMP";
        resPath = FrameworkPath.HotResources;
        srcPath = FrameworkPath.Scripts;
    }

    private static void ClearAllDirectory()
    {
        FileUtil.DeleteFileOrDirectory(tempPath);
        FileUtil.DeleteFileOrDirectory(abOutPath);
        FileUtil.DeleteFileOrDirectory(allOutPath);
        FileUtil.DeleteFileOrDirectory(Application.streamingAssetsPath);
        FileUtil.DeleteFileOrDirectory(packageTempPath);

        System.IO.Directory.CreateDirectory(tempPath);
        System.IO.Directory.CreateDirectory(abOutPath);
        System.IO.Directory.CreateDirectory(allOutPath);
        System.IO.Directory.CreateDirectory(Application.streamingAssetsPath);
        System.IO.Directory.CreateDirectory(packageTempPath);

        System.IO.Directory.CreateDirectory(Path.Artifacts + "/" + "ALLOUT" + "/Export/");
    }

    //热更新
    public static void StartHotUpdateBuild()
    {
        //设置配置
        SetConfig();
        //初始化
        Init();

        ClearAllDirectory();
        //清除Bundle名字
        ClearBundleName(resPath);
        //设置Bundle名字
        SetBundleName();
        //开始打包
        AssetBundleManifest assetBundleManifest = BuildPipeline.BuildAssetBundles(tempPath, BuildAssetBundleOptions.StrictMode | BuildAssetBundleOptions.ChunkBasedCompression, EditorUserBuildSettings.activeBuildTarget);

        //生成依赖文件
        GenerateDependFile(assetBundleManifest);

        //拷贝AB
        MoveABToOut(assetBundleManifest);

        //编译Dll包括所有的代码
        HybridCLR.Editor.Commands.PrebuildCommand.GenerateAll();

        //移动Scripts文件
        MoveScriptsFiles();

        //拷贝AOT
        CopyAOTAssembliesToStreamingAssets();

        //生成MD5文件
        GenerateFileMd5();

        //生成版本号
        GenerateVersion();

        //拷贝到本地
        CopyToNative();

        //if (File.Exists(EditorPathUtils.PACK_PATH + "/" + "ALLOUT" + "/updateNotice.json"))
        //{
        //    File.Delete(EditorPathUtils.PACK_PATH + "/" + "ALLOUT" + "/updateNotice.json");
        //}
        //File.Copy("D:/Project/GameWord/游戏/updateNotice.json", EditorPathUtils.PACK_PATH + "/" + "ALLOUT" + "/updateNotice.json");

        //刷新
        AssetDatabase.Refresh();
    }

    //打包
    public static void StartPackageBuild()
    {
        if (EditorUserBuildSettings.activeBuildTarget == BuildTarget.Android)
        {
            string project_path = packageTempPath + "/" + $"World{AssetDatabase.LoadAssetAtPath<Config>("Assets/Launcher/Config.asset").appVersion}.apk";//目标目录
            string[] outScenes = { "Assets/Launcher/Main.unity" };
            BuildPipeline.BuildPlayer(outScenes, project_path, BuildTarget.Android, BuildOptions.CompressWithLz4);

            string copyPath = Path.Artifacts + "/" + "ALLOUT" + "/Export/" + $"World{AssetDatabase.LoadAssetAtPath<Config>("Assets/Launcher/Config.asset").appVersion}.apk";

            if (System.IO.File.Exists(copyPath))
            {
                System.IO.File.Delete(copyPath);
            }

            System.IO.File.Copy(project_path, copyPath);
        }
        else if (EditorUserBuildSettings.activeBuildTarget == BuildTarget.iOS)
        {
            string project_path = packageTempPath + "/" + "IOSProject";//目标目录
            string[] outScenes = { "Assets/Launcher/Main.unity" };
            BuildPipeline.BuildPlayer(outScenes, project_path, BuildTarget.iOS, BuildOptions.CompressWithLz4);

            System.IO.Directory.CreateDirectory(packageTempPath + "/Payload");


            string copyPath = Path.Artifacts + "/" + "ALLOUT" + "/Export/" + "IOS";
            if (System.IO.File.Exists(copyPath))
            {
                System.IO.File.Delete(copyPath);
            }

            Framework.DirectoryUtils.CopyDirectory(packageTempPath, copyPath);

            Debug.Log("请打开Xcode生成,拷贝MetaFarmer.app到 PackageTempPath/Payload");
        }
        else if (EditorUserBuildSettings.activeBuildTarget == BuildTarget.StandaloneWindows64)
        {
            string project_path = packageTempPath + "/MetaFarmer.exe";//目标目录
            string[] outScenes = { "Assets/Launcher/Main.unity" };
            BuildPipeline.BuildPlayer(outScenes, project_path, BuildTarget.StandaloneWindows64, BuildOptions.CompressWithLz4);

            string copyPath = Path.Artifacts + "/" + "ALLOUT" + "/Export/" + "Win";
            if (System.IO.File.Exists(copyPath))
            {
                System.IO.File.Delete(copyPath);
            }

            Framework.DirectoryUtils.CopyDirectory(packageTempPath, copyPath);
        }
    }

    public static void GenerateIpa()
    {
        Init();

        string payloadPath = Path.Artifacts + "/PACKAGETEMP/Payload";
        string outPath = Path.Artifacts + "/" + "ALLOUT" + "/" + "MetaFarmer.ipa";

    }

    /// <summary>
    /// 设置bundle名字
    /// </summary>
    public static void SetBundleName()
    {
        AssetDatabase.RemoveUnusedAssetBundleNames();

        //获取所有的文件除了meta
        string[] dataArr = Framework.File.GetFiles(resPath, ".meta");

        foreach (var path in dataArr)
        {
            FileInfo fileInfo = new FileInfo(path);
            string filePath = System.IO.Path.Combine(fileInfo.DirectoryName).Replace("\\", "/");

            if (filePath.Contains("Fonts/TTF"))
            {
                continue;
            }

            string bundleName = filePath.Replace(resPath + "/", "").Replace("/", "_");

            bundleName = bundleName.ToLower();
            string formatPath = System.IO.Path.Combine(fileInfo.FullName.Replace("\\", "/"));


            string bundlePath = FrameworkPath.ConvertToAssets(formatPath);

            AssetImporter importer = AssetImporter.GetAtPath(bundlePath);

            if (importer)
            {
                importer.assetBundleName = bundleName;
                importer.assetBundleVariant = null;
            }
        }

        AssetDatabase.Refresh();
        AssetDatabase.RemoveUnusedAssetBundleNames();
    }
    /// <summary>
    /// 清除bundle名字
    /// </summary>
    /// <param name="path"></param>
    public static void ClearBundleName(string path)
    {
        string[] files = System.IO.Directory.GetFiles(path);
        string[] dirs = System.IO.Directory.GetDirectories(path);

        foreach (var file in files.Concat(dirs).ToArray())
        {
            AssetImporter importer = AssetImporter.GetAtPath(FrameworkPath.ConvertToAssets(file));
            if (importer)
            {
                importer.SetAssetBundleNameAndVariant(null, null);
            }
        }
        foreach (var dir in dirs)
        {
            ClearBundleName(dir);
        }
    }

    /// <summary>
    /// 移动脚本文件
    /// </summary>
    private static void MoveScriptsFiles()
    {
        var target = EditorUserBuildSettings.activeBuildTarget;

        //创建scripts文件夹
        System.IO.Directory.CreateDirectory($"{allOutPath}/{"scripts"}");

        string hotfixDllSrcDir = HybridCLR.Editor.SettingsUtil.GetHotUpdateDllsOutputDirByTarget(target);
        foreach (var dll in HybridCLR.Editor.SettingsUtil.HotUpdateAssemblyFilesExcludePreserved)
        {
            string dllPath = $"{hotfixDllSrcDir}/{dll}";
            Debug.Log("dllPath ------ " + dllPath);
            string dllBytesPath = $"{allOutPath}/{"scripts"}/{dll.ToLower()}.bytes";
            System.IO.File.Copy(dllPath, dllBytesPath, true);
            Debug.Log($"[CopyHotUpdateAssembliesToStreamingAssets] copy hotfix dll {dllPath} -> {dllBytesPath}");
        }

    }

    public static void CopyAOTAssembliesToStreamingAssets()
    {
        var target = EditorUserBuildSettings.activeBuildTarget;
        string aotAssembliesSrcDir = SettingsUtil.GetAssembliesPostIl2CppStripDir(target);
        string aotAssembliesDstDir = Application.streamingAssetsPath;

        foreach (var dll in SettingsUtil.AOTAssemblyNames)
        {
            string srcDllPath = $"{aotAssembliesSrcDir}/{dll}.dll";
            if (!System.IO.File.Exists(srcDllPath))
            {
                Debug.LogError($"ab中添加AOT补充元数据dll:{srcDllPath} 时发生错误,文件不存在。裁剪后的AOT dll在BuildPlayer时才能生成，因此需要你先构建一次游戏App后再打包。");
                continue;
            }
            string dllBytesPath = $"{aotAssembliesDstDir}/{dll}.dll.bytes";
            System.IO.File.Copy(srcDllPath, dllBytesPath, true);
            Debug.Log($"[CopyAOTAssembliesToStreamingAssets] copy AOT dll {srcDllPath} -> {dllBytesPath}");
        }
    }


    private static void MoveABToOut(AssetBundleManifest assetBundleManifest)
    {
        string resPath = allOutPath + "/" + "res";
        System.IO.Directory.CreateDirectory(resPath);

        string[] bundleNames = assetBundleManifest.GetAllAssetBundles();
        for (int i = 0; i < bundleNames.Length; i++)
        {
            string bundleName = bundleNames[i];

            string sourcePath = tempPath + "/" + bundleName;
            string destPath = resPath + "/" + bundleName;

            System.IO.File.Move(sourcePath, destPath);
        }
    }

    private static void GenerateFileMd5()
    {
        string fileMd5Content = "";

        foreach (var filePath in Framework.File.GetFiles(allOutPath))
        {
            FileInfo fileInfo = new FileInfo(filePath);

            string relativePath = filePath.Replace(allOutPath + "/", "");

            fileMd5Content += relativePath + "|" + Framework.File.GetFileMd5(filePath) + "|" + fileInfo.Length + "\n";
        }

        //去除最后一个\n
        fileMd5Content = fileMd5Content.Substring(0, fileMd5Content.Length - 1);
        System.IO.File.WriteAllText(allOutPath + "/" + "filemd5.txt", fileMd5Content);
    }

    private static void GenerateDependFile(AssetBundleManifest assetBundleManifest)
    {
        AssetDependData assetData = new AssetDependData();
        assetData.dependEntries = new List<AssetDependData.DependencyEntry>();

        string[] bundleNames = assetBundleManifest.GetAllAssetBundles();

        for (int i = 0; i < bundleNames.Length; i++)
        {
            string bundleName = bundleNames[i];

            string[] depends = assetBundleManifest.GetAllDependencies(bundleName);

            Debug.Log("bundleName : " + bundleName + depends.Length);
            
            AssetDependData.DependencyEntry entry = new AssetDependData.DependencyEntry();
            entry.key = bundleName;
            entry.dependencies = new List<string>(depends);
            assetData.dependEntries.Add(entry);
        }
        string jsonStr = JsonUtility.ToJson(assetData);

        System.IO.File.WriteAllText(allOutPath + "/resdepend.txt", jsonStr);
    }

    public static string[] ConvertNameArrPath(string[] depends)
    {
        string[] newArr = new string[depends.Length];

        for (int i = 0; i < depends.Length; i++)
        {
            newArr[i] = "\"" + ConvertLowerToNormalPath(depends[i]) + "\"";
        }
        //MakeFirstCharToBigChar(depends[i])
        return newArr;
    }

    public static string ConvertLowerToNormalPath(string lowerStr)
    {
        string[] files = Framework.File.GetFiles(Path.Res, "meta");
        for (int i = 0; i < files.Length; i++)
        {
            string lowPath = files[i].ToLower().Replace("/", "_");
            if (lowPath.Contains(lowerStr))
            {
                string path = files[i].Substring(Path.Res.Length + 1, lowerStr.Length);
                //files[i].Replace(EditorPathUtils.RES_PATH, "");
                return path;
            }
        }
        return "";
    }
    public static string ConvertNamePath(string str)
    {
        string[] arr = str.Split('_');
        string[] newArr = new string[arr.Length];

        for (int i = 0; i < arr.Length; i++)
        {
            newArr[i] = MakeFirstCharToBigChar(arr[i]);
        }

        return string.Join("/", newArr);
    }

    public static string MakeFirstCharToBigChar(string str)
    {

        char[] arr = str.ToCharArray();
        char[] tempArr = new char[arr.Length];

        for (int i = 0; i < arr.Length; i++)
        {
            if (i == 0)
            {
                string s = arr[0].ToString().ToUpper();
                char[] t = s.ToCharArray();
                tempArr[0] = s.ToCharArray()[0];
            }
            else
            {
                tempArr[i] = arr[i];
            }
        }
        string h = new string(tempArr);

        //Debug.Log("tempArr.ToString() : " + h);
        return h;
    }

    [Serializable]
    public class VersionClass
    {
        public string version;
        public string updateInfo;
        public string url;
    }

    private static void GenerateVersion()
    {
        System.IO.File.WriteAllText(allOutPath + "/version.txt", GetHotVersion());
    }

    /// <summary>
    /// 拷贝到本地
    /// </summary>
    private static void CopyToNative()
    {
        ZipSharp zip = new ZipSharp();
        zip.CompressFolder(allOutPath, Application.streamingAssetsPath + "/Native");
    }

    private static void SetConfig()
    {
        PlayerSettings.applicationIdentifier = "com.Paramedise.Qiyujianghu";
        PlayerSettings.bundleVersion = AssetDatabase.LoadAssetAtPath<Config>("Assets/Launcher/Config.asset").appVersion;
        PlayerSettings.Android.bundleVersionCode = 1;
        PlayerSettings.iOS.buildNumber = "1";
        PlayerSettings.iOS.appleEnableAutomaticSigning = true;
    }


    public static string GetHotVersion()
    {
        Config packageConfig = AssetDatabase.LoadAssetAtPath<Config>("Assets/Launcher/Config.asset");
        return packageConfig.hotVersion;
    }
    

}
