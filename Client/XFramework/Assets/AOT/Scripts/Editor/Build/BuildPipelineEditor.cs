using UnityEditor;
using UnityEngine;

namespace AOT.Scripts.Editor.Build
{
    public partial class BuildPipelineEditor
    {
        // 菜单：构建内部测试全量包
        [MenuItem("YooAsset/构建/全量包(离线)")]
        public static void BuildOfflineAPK()
        {
            Debug.Log("开始构建内部测试全量包...");
            Debug.Log("步骤1/5: 设置离宏");
            SetScriptingDefineSymbol(BuildHelper.OFFLINE_MODE_SYMBOL); // 设置编译符号为离线模式
            Debug.Log("步骤2/5: 初始化清空资源输出目录");
            BuildInit(true);
            Debug.Log("步骤3/5: 构建DLL");
            BuildDLL();    
            Debug.Log("步骤4/5: 构建资源包");
            BuildFullAB();   
            Debug.Log("步骤5/5: 构建APK");
            BuildPlayer(true,"Offline"); // 构建包含所有资源的APK
            Debug.Log("========== 全量包(离线)完成 ==========");
        }

        // 构建支持热更的全量包
        [MenuItem("YooAsset/构建/全量包APK(热更)")]
        public static void BuildFullPackageAPK()
        {
            Debug.Log("开始构建全量包APK(热更)...");
            Debug.Log("步骤1/6: 设置离宏");
            SetScriptingDefineSymbol(BuildHelper.ASSETBUNDLE_MODE_SYMBOL); // 设置编译符号为离线模式
            Debug.Log("步骤2/6: 初始化清空资源输出目录");
            BuildInit(true);
            Debug.Log("步骤3/6: 构建DLL");
            BuildDLL();    
            Debug.Log("步骤4/6: 构建资源包");
            BuildFullAB();
            Debug.Log("步骤5/6: 服务器资源同步");
            BuildFullSeverSync();
            Debug.Log("步骤6/6: 构建APK");
            BuildPlayer(true,"Release"); // 构建包含所有资源的APK
            Debug.Log("========== 全量包APK(热更)完成 ==========");
        }
        
        // 构建支持热更的全量包
        [MenuItem("YooAsset/构建/空包APK(热更)")]
        public static void BuildNulllPackageAPK()
        {
            Debug.Log("开始构建空包APK(热更)...");
            Debug.Log("步骤1/5: 设置离宏");
            SetScriptingDefineSymbol(BuildHelper.ASSETBUNDLE_MODE_SYMBOL); // 设置编译符号为离线模式
            Debug.Log("步骤2/6: 初始化清空资源输出目录");
            BuildInit(true);
            Debug.Log("步骤3/6: 构建DLL");
            BuildDLL();    
            Debug.Log("步骤4/6: 构建资源包");
            BuildIncrementalAB();
            Debug.Log("步骤5/6: 服务器资源同步");
            BuildSeverSync();
            Debug.Log("步骤6/6: 构建APK");
            BuildPlayer(true,"Release"); // 构建包含所有资源的APK
            Debug.Log("========== 空包APK(热更)完成 ==========");
        }

        // 菜单：构建热更新资源包
        [MenuItem("YooAsset/构建/增量资源包(热更)")]
        public static void BuildIncrementalPackageNoAPK()
        {
            Debug.Log("开始构建增量资源包(热更)...");
            Debug.Log("步骤1/4: 初始化清空资源输出目录");
            BuildInit(false);
            Debug.Log("步骤2/4: 构建DLL");
            BuildDLL();    
            Debug.Log("步骤3/4: 构建资源包");
            BuildIncrementalAB();
            Debug.Log("步骤4/4: 服务器资源同步");
            BuildSeverSync();
            Debug.Log("========== 增量资源包(热更) ==========");
        }
    }
}