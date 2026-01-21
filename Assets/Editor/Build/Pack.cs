using Framework;
using System.IO;
using UnityEditor;
using UnityEngine;

public class Pack
{

    //强更新模块
    [MenuItem("Build/打包三平台")]
    public static void BuildAllPack()
    {
        BuildAndroidApk();
        BuildIpa();
        BuildWin();
        Debug.Log("打包三平台Package完成");
    }

    [MenuItem("Build/安卓Apk")]
    public static void BuildAndroidApk()
    {
        BuildAndroidHotUpdate();
        Build.StartPackageBuild();
        Debug.Log("安卓Apk完成");
    }

    [MenuItem("Build/苹果Ipa")]
    public static void BuildIpa()
    {
        BuildIOSHotUpdate();
        Build.StartPackageBuild();
        Debug.Log("苹果Ipa完成");
    }
    [MenuItem("Build/Win")]
    public static void BuildWin()
    {
        BuildWinHotUpdate();
        Build.StartPackageBuild();
        Debug.Log("Win完成");
    }
    [MenuItem("Build/----------")]
    public static void Build_____()
    {
    }

    //热更新模块
    [MenuItem("Build/热更新三平台")]
    public static void BuildAllHotUpdate()
    {
        BuildAndroidHotUpdate();
        BuildIOSHotUpdate();
        BuildWinHotUpdate();
        Debug.Log("三平台热更新完成");
    }

    [MenuItem("Build/安卓热更新")]
    public static void BuildAndroidHotUpdate()
    {
        SwitchPlatform(BuildTarget.Android);
        Build.StartHotUpdateBuild();
        Debug.Log("安卓热更新完成");
    }

    [MenuItem("Build/苹果热更新")]
    public static void BuildIOSHotUpdate()
    {
        SwitchPlatform(BuildTarget.iOS);
        Build.StartHotUpdateBuild();
        Debug.Log("苹果热更新完成");
    }

    [MenuItem("Build/win热更新")]
    public static void BuildWinHotUpdate()
    {
        SwitchPlatform(BuildTarget.StandaloneWindows64);
        Build.StartHotUpdateBuild();
        Debug.Log("win热更新完成");
    }

    [MenuItem("Build/-----------")]
    public static void Build______()
    {
    }

    [MenuItem("Build/清除打包空间")]
    public static void ClearPackPath()
    {
        FileUtil.DeleteFileOrDirectory(Path.Artifacts);
        System.IO.Directory.CreateDirectory(Path.Artifacts);
        Debug.Log("清理完成");
    }




    //切换平台
    private static void SwitchPlatform(BuildTarget buildTarget)
    {
        //切换平台到Android
        if (buildTarget == BuildTarget.Android)
        {
            EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.Android, BuildTarget.Android);
        }
        else if (buildTarget == BuildTarget.iOS)
        {
            EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.iOS, BuildTarget.iOS);
        }
        else if (buildTarget == BuildTarget.StandaloneWindows64)
        {
            EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.Standalone, BuildTarget.StandaloneWindows64);
        }
    }

}
