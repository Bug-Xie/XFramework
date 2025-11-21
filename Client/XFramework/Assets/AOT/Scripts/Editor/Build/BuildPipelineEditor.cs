using UnityEditor;
using UnityEngine;


public partial class BuildPipelineEditor
{
    // 菜单：构建内部测试全量包
    //[MenuItem("YooAsset/构建/全量包(离线)")]
    public static void BuildOfflineAPK()
    {
        BuildLogger.WriteLog("开始构建内部测试全量包...");
        BuildLogger.WriteLog("步骤1/6: 设置离线宏");
        SetScriptingDefineSymbol(BuildToolPanel.GetOfflineModeSymbol()); // 设置编译符号为离线模式
        BuildLogger.WriteLog("步骤2/6: 添加EnableLog宏");
        SetEnableLogSymbol(BuildToolPanel.IsEnableLog()); // 离线包需要日志功能
        BuildLogger.WriteLog("步骤3/6: 初始化清空资源输出目录");
        BuildInit(true);
        BuildLogger.WriteLog("步骤4/6: 构建DLL");
        BuildDLL();
        BuildLogger.WriteLog("步骤5/6: 构建资源包");
        BuildFullAB();
        BuildLogger.WriteLog("步骤6/6: 构建APK");
        BuildPlayer( "Offline"); // 构建包含所有资源的APK
        BuildLogger.WriteLog("========== 全量包(离线)完成 ==========");
    }

    // 构建支持热更的全量包
    //[MenuItem("YooAsset/构建/全量包APK(热更)")]
    public static void BuildFullPackageAPK()
    {
        BuildLogger.WriteLog("开始构建全量包APK(热更)...");
        BuildLogger.WriteLog("步骤1/7: 设置热更宏");
        SetScriptingDefineSymbol(BuildToolPanel.GetAssetBundleModeSymbol()); // 设置编译符号为资源包模式
        BuildLogger.WriteLog("步骤2/7: 移除EnableLog宏");
        SetEnableLogSymbol(BuildToolPanel.IsEnableLog()); // 热更包移除日志功能，减少包体大小
        BuildLogger.WriteLog("步骤3/7: 初始化清空资源输出目录");
        BuildInit(true);
        BuildLogger.WriteLog("步骤4/7: 构建DLL");
        BuildDLL();
        BuildLogger.WriteLog("步骤5/7: 构建资源包");
        BuildFullAB();
        BuildLogger.WriteLog("步骤6/7: 服务器资源同步");
        //BuildFullSeverSync();
        BuildLogger.WriteLog("步骤7/7: 构建APK");
        BuildPlayer( "Release"); // 构建包含所有资源的APK
        BuildLogger.WriteLog("========== 全量包APK(热更)完成 ==========");
    }

    // 构建支持热更的空包
    //[MenuItem("YooAsset/构建/空包APK(热更)")]
    public static void BuildNulllPackageAPK()
    {
        BuildLogger.WriteLog("开始构建空包APK(热更)...");
        BuildLogger.WriteLog("步骤1/6: 设置热更宏");
        SetScriptingDefineSymbol(BuildToolPanel.GetAssetBundleModeSymbol()); // 设置编译符号为资源包模式
        BuildLogger.WriteLog("步骤2/6: 移除EnableLog宏");
        SetEnableLogSymbol(BuildToolPanel.IsEnableLog()); // 空包移除日志功能，减少包体大小
        BuildLogger.WriteLog("步骤3/6: 初始化清空资源输出目录");
        BuildInit(true);
        BuildLogger.WriteLog("步骤4/6: 构建DLL");
        BuildDLL();
        BuildLogger.WriteLog("步骤5/6: 构建资源包");
        BuildIncrementalAB();
        BuildLogger.WriteLog("步骤6/6: 构建APK");
        BuildPlayer("Release"); // 构建包含所有资源的APK
        BuildLogger.WriteLog("========== 空包APK(热更)完成 ==========");
    }

    // 菜单：构建热更新资源包
    //[MenuItem("YooAsset/构建/增量资源包(热更)")]
    public static void BuildIncrementalPackageNoAPK()
    {
        BuildLogger.WriteLog("开始构建增量资源包(热更)...");
        BuildLogger.WriteLog("步骤1/5: 设置热更宏");
        SetScriptingDefineSymbol(BuildToolPanel.GetAssetBundleModeSymbol()); // 设置编译符号为资源包模式
        BuildLogger.WriteLog("步骤2/5: 移除EnableLog宏");
        SetEnableLogSymbol(BuildToolPanel.IsEnableLog()); // 热更资源包移除日志功能
        BuildLogger.WriteLog("步骤3/5: 初始化清空资源输出目录");
        BuildInit(false);
        BuildLogger.WriteLog("步骤4/5: 构建DLL");
        BuildDLL();
        BuildLogger.WriteLog("步骤5/5: 构建资源包");
        BuildIncrementalAB();
        BuildLogger.WriteLog("========== 增量资源包(热更)完成 ==========");
    }


}