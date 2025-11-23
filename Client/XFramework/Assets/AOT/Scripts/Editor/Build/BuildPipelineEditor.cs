
public partial class BuildPipelineEditor
{
    public const string OFFLINE_MODE_SYMBOL = "RESOURCE_OFFLINE"; // offline
    public const string ASSETBUNDLE_MODE_SYMBOL = "RESOURCE_ASSETBUNDLE"; // ab热更
    public const string ENABLE_LOG_SYMBOL = "EnableLog";  //日志
    
    // 菜单：构建内部测试全量包
    //[MenuItem("YooAsset/构建/全量包(离线)")]
    public static void BuildOfflineAPK()
    {
        Log.Info("开始构建内部测试全量包...");
        Log.Info("步骤1/7: 设置离线宏");
        SetEnableLogSymbol(OFFLINE_MODE_SYMBOL,true); // 设置编译符号为离线模式
        SetEnableLogSymbol(ASSETBUNDLE_MODE_SYMBOL,false); // 设置编译符号为离线模式
        SetEnableLogSymbol(ENABLE_LOG_SYMBOL,BuildToolPanel.IsEnableLog()); // 离线包需要日志功能
        Log.Info("步骤2/7: 管理GUI日志Reporter");
        ManageReporter(BuildToolPanel.IsEnableLog()); // 根据日志设置管理Reporter组件
        Log.Info("步骤3/7: 初始化清空资源输出目录");
        BuildInit(true);
        Log.Info("步骤4/7: 构建DLL");
        BuildDLL();
        Log.Info("步骤5/7: 构建资源包");
        BuildAB(true,true); // 直接递增版本
        Log.Info("步骤6/7: 构建APK");
        BuildPlayer( "Offline"); // 构建包含所有资源的APK
        Log.Info("步骤7/7: 恢复初始场景，删除日志Reporter");
        ManageReporter(false); 
        Log.Info("========== 全量包(离线)完成 ==========");
    }

    // 构建支持热更的全量包
    //[MenuItem("YooAsset/构建/全量包APK(热更)")]
    public static void BuildFullPackageAPK()
    {
        Log.Info("开始构建全量包APK(热更)...");
        Log.Info("步骤1/8: 设置热更宏");
        SetEnableLogSymbol(OFFLINE_MODE_SYMBOL,false); // 设置编译符号为离线模式
        SetEnableLogSymbol(ASSETBUNDLE_MODE_SYMBOL,true); // 设置编译符号为离线模式
        SetEnableLogSymbol(ENABLE_LOG_SYMBOL,BuildToolPanel.IsEnableLog()); // 离线包需要日志功能
        Log.Info("步骤2/8: 管理GUI日志Reporter");
        ManageReporter(BuildToolPanel.IsEnableLog()); // 根据日志设置管理Reporter组件
        Log.Info("步骤3/8: 初始化清空资源输出目录");
        BuildInit(true);
        Log.Info("步骤4/8: 构建DLL");
        BuildDLL();
        Log.Info("步骤5/8: 构建资源包");
        BuildAB(true,true); // 直接递增版本
        Log.Info("步骤6/8: 服务器资源同步");
        BuildSeverSync();
        Log.Info("步骤7/8: 构建APK");
        BuildPlayer( "Release"); // 构建包含所有资源的APK
        Log.Info("步骤7/8: 恢复初始场景，删除日志Reporter");
        ManageReporter(false); 
        Log.Info("========== 全量包APK(热更)完成 ==========");
    }

    // 构建支持热更的空包
    //[MenuItem("YooAsset/构建/空包APK(热更)")]
    public static void BuildNulllPackageAPK()
    {
        Log.Info("开始构建空包APK(热更)...");
        Log.Info("步骤1/7: 设置热更宏");
        SetEnableLogSymbol(OFFLINE_MODE_SYMBOL,false); // 设置编译符号为离线模式
        SetEnableLogSymbol(ASSETBUNDLE_MODE_SYMBOL,true); // 设置编译符号为离线模式
        SetEnableLogSymbol(ENABLE_LOG_SYMBOL,BuildToolPanel.IsEnableLog()); // 离线包需要日志功能
        Log.Info("步骤2/7: 管理GUI日志Reporter");
        ManageReporter(BuildToolPanel.IsEnableLog()); // 根据日志设置管理Reporter组件
        Log.Info("步骤3/7: 初始化清空资源输出目录");
        BuildInit(true);
        Log.Info("步骤4/7: 构建DLL");
        BuildDLL();
        Log.Info("步骤5/7: 构建资源包");
        BuildAB(true,false); // 直接递增版本
        Log.Info("步骤6/7: 服务器资源同步");
        BuildSeverSync();
        Log.Info("步骤7/7: 构建APK");
        BuildPlayer("Release"); // 构建包含所有资源的APK
        Log.Info("步骤7/8: 恢复初始场景，删除日志Reporter");
        ManageReporter(false); 
        Log.Info("========== 空包APK(热更)完成 ==========");
    }

    // 菜单：构建热更新资源包
    //[MenuItem("YooAsset/构建/增量资源包(热更)")]
    public static void BuildIncrementalPackageNoAPK()
    {
        Log.Info("开始构建增量资源包(热更)...");
        Log.Info("步骤1/6: 管理GUI日志Reporter");
        ManageReporter(BuildToolPanel.IsEnableLog()); // 根据日志设置管理Reporter组件
        Log.Info("步骤2/6: 初始化清空资源输出目录");
        BuildInit(false);
        Log.Info("步骤3/6: 构建DLL");
        BuildDLL();
        Log.Info("步骤4/6: 构建资源包");
        BuildAB(false,false); // 直接递增版本
        Log.Info("步骤5/6: 服务器资源同步");
        BuildSeverSync();
        Log.Info("步骤6/6: 恢复初始场景，删除日志Reporter");
        ManageReporter(false); 
        Log.Info("========== 增量资源包(热更)完成 ==========");
    }


}