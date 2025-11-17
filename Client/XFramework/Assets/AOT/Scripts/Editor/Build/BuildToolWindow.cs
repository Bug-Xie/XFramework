using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections.Generic;

public class BuildToolWindow
{
    // // 配置数据
    // private string serverFolderPath = "";
    // private string apkFolderPath = "";
    // private BuildTarget buildTarget = BuildTarget.Android;
    //
    // [MenuItem("Tools/打包工具")]
    // public static void ShowWindow()
    // {
    //     GetWindow<BuildToolWindow>("打包工具");
    // }
    //
    // void OnEnable()
    // {
    //     // 从EditorPrefs加载保存的路径
    //     serverFolderPath = EditorPrefs.GetString("BuildTool_ServerPath", "");
    //     apkFolderPath = EditorPrefs.GetString("BuildTool_ApkPath", "");
    // }
    //
    // void OnGUI()
    // {
    //     GUILayout.Label("📦 打包配置", EditorStyles.boldLabel);
    //     
    //     // 服务器文件夹路径
    //     EditorGUILayout.BeginHorizontal();
    //     serverFolderPath = EditorGUILayout.TextField("服务器文件夹路径", serverFolderPath);
    //     if (GUILayout.Button("选择", GUILayout.Width(50)))
    //     {
    //         string path = EditorUtility.OpenFolderPanel("选择服务器文件夹", "", "");
    //         if (!string.IsNullOrEmpty(path))
    //         {
    //             serverFolderPath = path;
    //             SavePaths();
    //         }
    //     }
    //     EditorGUILayout.EndHorizontal();
    //     
    //     // APK文件夹路径
    //     EditorGUILayout.BeginHorizontal();
    //     apkFolderPath = EditorGUILayout.TextField("APK文件夹路径", apkFolderPath);
    //     if (GUILayout.Button("选择", GUILayout.Width(50)))
    //     {
    //         string path = EditorUtility.OpenFolderPanel("选择APK输出文件夹", "", "");
    //         if (!string.IsNullOrEmpty(path))
    //         {
    //             apkFolderPath = path;
    //             SavePaths();
    //         }
    //     }
    //     EditorGUILayout.EndHorizontal();
    //     
    //     // 构建平台
    //     buildTarget = (BuildTarget)EditorGUILayout.EnumPopup("目标平台", buildTarget);
    //     
    //     EditorGUILayout.Space();
    //     
    //     // 显示当前配置信息
    //     EditorGUILayout.HelpBox(
    //         $"当前平台: {GetPlatformName(buildTarget)}\n" +
    //         $"服务器路径: {serverFolderPath}\n" +
    //         $"APK路径: {apkFolderPath}", 
    //         MessageType.Info
    //     );
    //     
    //     // 一键打包按钮
    //     GUI.backgroundColor = Color.green;
    //     if (GUILayout.Button("🚀 一键打包", GUILayout.Height(40)))
    //     {
    //         StartBuildProcess();
    //     }
    //     GUI.backgroundColor = Color.white;
    //     
    //     // 分步打包按钮
    //     EditorGUILayout.Space();
    //     GUILayout.Label("分步打包", EditorStyles.boldLabel);
    //     
    //     EditorGUILayout.BeginHorizontal();
    //     if (GUILayout.Button("1. 处理HybridCLR DLL"))
    //     {
    //         ProcessHybridCLRDLLs();
    //     }
    //     if (GUILayout.Button("2. YooAsset打包"))
    //     {
    //         BuildYooAssetBundle();
    //     }
    //     EditorGUILayout.EndHorizontal();
    //     
    //     EditorGUILayout.BeginHorizontal();
    //     if (GUILayout.Button("3. 复制到服务器"))
    //     {
    //         CopyToServerFolder();
    //     }
    //     if (GUILayout.Button("4. 打包APK"))
    //     {
    //         BuildAPK();
    //     }
    //     EditorGUILayout.EndHorizontal();
    //     
    //     // 工具按钮
    //     EditorGUILayout.Space();
    //     GUILayout.Label("工具", EditorStyles.boldLabel);
    //     
    //     EditorGUILayout.BeginHorizontal();
    //     if (GUILayout.Button("打开服务器文件夹"))
    //     {
    //         if (Directory.Exists(serverFolderPath))
    //         {
    //             EditorUtility.RevealInFinder(serverFolderPath);
    //         }
    //     }
    //     
    //     if (GUILayout.Button("打开APK文件夹"))
    //     {
    //         if (Directory.Exists(apkFolderPath))
    //         {
    //             EditorUtility.RevealInFinder(apkFolderPath);
    //         }
    //     }
    //     EditorGUILayout.EndHorizontal();
    // }
    //
    // private void SavePaths()
    // {
    //     EditorPrefs.SetString("BuildTool_ServerPath", serverFolderPath);
    //     EditorPrefs.SetString("BuildTool_ApkPath", apkFolderPath);
    // }
    //
    // private void StartBuildProcess()
    // {
    //     if (string.IsNullOrEmpty(serverFolderPath) || string.IsNullOrEmpty(apkFolderPath))
    //     {
    //         EditorUtility.DisplayDialog("错误", "请先设置服务器文件夹路径和APK文件夹路径", "确定");
    //         return;
    //     }
    //     
    //     bool proceed = EditorUtility.DisplayDialog("确认打包", 
    //         $"即将开始打包流程：\n平台: {GetPlatformName(buildTarget)}\n服务器路径: {serverFolderPath}\nAPK路径: {apkFolderPath}", 
    //         "开始打包", "取消");
    //         
    //     if (!proceed) return;
    //     
    //     try
    //     {
    //         // 步骤1: 处理HybridCLR DLL
    //         EditorUtility.DisplayProgressBar("打包中", "正在处理HybridCLR DLL...", 0.1f);
    //         ProcessHybridCLRDLLs();
    //         
    //         // 步骤2: YooAsset打包
    //         EditorUtility.DisplayProgressBar("打包中", "正在打包YooAsset...", 0.3f);
    //         BuildYooAssetBundle();
    //         
    //         // 步骤3: 复制到服务器
    //         EditorUtility.DisplayProgressBar("打包中", "正在复制到服务器...", 0.6f);
    //         CopyToServerFolder();
    //         
    //         // 步骤4: 打包APK
    //         EditorUtility.DisplayProgressBar("打包中", "正在打包APK...", 0.8f);
    //         BuildAPK();
    //         
    //         EditorUtility.ClearProgressBar();
    //         
    //         // 完成提示
    //         bool openFolder = EditorUtility.DisplayDialog("打包完成", "打包流程已完成！是否打开APK文件夹？", "打开文件夹", "关闭");
    //         if (openFolder)
    //         {
    //             EditorUtility.RevealInFinder(apkFolderPath);
    //         }
    //     }
    //     catch (System.Exception e)
    //     {
    //         EditorUtility.ClearProgressBar();
    //         EditorUtility.DisplayDialog("错误", $"打包失败: {e.Message}", "确定");
    //         Debug.LogError($"打包失败: {e}");
    //     }
    // }
    //
    // private void ProcessHybridCLRDLLs()
    // {
    //     string platformName = GetPlatformName(buildTarget);
    //
    //     // 修改后的路径 - 直接使用相对路径
    //     string targetFolder = "Jit/PakageAsset/ScriptDLL";
    //
    //     // 确保目标文件夹存在
    //     if (!Directory.Exists(targetFolder))
    //     {
    //         Directory.CreateDirectory(targetFolder);
    //     }
    //
    //     // 删除目标文件夹内所有文件
    //     string[] existingFiles = Directory.GetFiles(targetFolder);
    //     foreach (string file in existingFiles)
    //     {
    //         File.Delete(file);
    //     }
    //
    //     // 复制AOT DLLs - 修改后的路径
    //     string aotSourcePath = $"HybridCLRData/AssembliesPostIl2CppStrip/{platformName}";
    //     string[] aotDlls = {"mscorlib.dll", "System.Core.dll", "System.dll"};
    //
    //     foreach (string dll in aotDlls)
    //     {
    //         CopyDLLWithBytesExtension(aotSourcePath, dll, targetFolder);
    //     }
    //
    //     // 复制热更DLL - 修改后的路径
    //     string hotUpdateSourcePath = $"HybridCLRData/HotUpdateDlls/{platformName}";
    //     CopyDLLWithBytesExtension(hotUpdateSourcePath, "HotUpdate.dll", targetFolder);
    //
    //     AssetDatabase.Refresh();
    //     Debug.Log("✅ HybridCLR DLL处理完成");
    // }
    //
    // private void CopyDLLWithBytesExtension(string sourceFolder, string dllName, string targetFolder)
    // {
    //     string sourcePath = Path.Combine(sourceFolder, dllName);
    //     string targetPath = Path.Combine(targetFolder, dllName + ".bytes");
    //     
    //     if (File.Exists(sourcePath))
    //     {
    //         File.Copy(sourcePath, targetPath, true);
    //         Debug.Log($"📄 已复制: {dllName} -> {dllName}.bytes");
    //     }
    //     else
    //     {
    //         Debug.LogWarning($"⚠️ 文件不存在: {sourcePath}");
    //     }
    // }
    //
    // private void BuildYooAssetBundle()
    // {
    //     // 调用YooAsset的构建命令
    //     EditorApplication.ExecuteMenuItem("YooAsset/AssetBundle Builder");
    //     Debug.Log("✅ YooAsset打包完成 - 请在YooAsset窗口中配置并执行构建");
    // }
    //
    // private void CopyToServerFolder()
    // {
    //     string platformName = GetPlatformName(buildTarget);
    //
    //     // 修改后的YooAsset打包路径
    //     string yooAssetBuildPath = $"Bundles/{platformName}/DefaultPackage";
    //
    //     if (!Directory.Exists(yooAssetBuildPath))
    //     {
    //         throw new System.Exception($"❌ YooAsset打包路径不存在: {yooAssetBuildPath}");
    //     }
    //
    //     // 获取版本文件夹
    //     string[] versionFolders = Directory.GetDirectories(yooAssetBuildPath);
    //     if (versionFolders.Length == 0)
    //     {
    //         throw new System.Exception($"❌ 在 {yooAssetBuildPath} 中找不到版本文件夹");
    //     }
    //
    //     string versionFolder = versionFolders[0];
    //     string versionName = Path.GetFileName(versionFolder);
    //
    //     Debug.Log($"📦 检测到版本: {versionName}");
    //
    //     // 确保服务器文件夹存在
    //     if (!Directory.Exists(serverFolderPath))
    //     {
    //         Directory.CreateDirectory(serverFolderPath);
    //     }
    //
    //     // 清空服务器文件夹
    //     ClearDirectory(serverFolderPath);
    //
    //     // 复制所有文件到服务器文件夹
    //     string[] filesToCopy = Directory.GetFiles(versionFolder);
    //     foreach (string file in filesToCopy)
    //     {
    //         string fileName = Path.GetFileName(file);
    //         string destPath = Path.Combine(serverFolderPath, fileName);
    //         File.Copy(file, destPath, true);
    //         Debug.Log($"📤 复制到服务器: {fileName}");
    //     }
    //
    //     Debug.Log($"✅ 已复制 {filesToCopy.Length} 个文件到服务器文件夹");
    // }
    //
    // private void BuildAPK()
    // {
    //     if (buildTarget != BuildTarget.Android)
    //     {
    //         EditorUtility.DisplayDialog("警告", "当前选择的平台不是Android，请先切换平台", "确定");
    //         return;
    //     }
    //     
    //     string apkName = $"{Application.productName}_{System.DateTime.Now:yyyyMMdd_HHmmss}.apk";
    //     string apkFullPath = Path.Combine(apkFolderPath, apkName);
    //     
    //     if (!Directory.Exists(apkFolderPath))
    //     {
    //         Directory.CreateDirectory(apkFolderPath);
    //     }
    //     
    //     // 构建场景列表
    //     List<string> scenes = new List<string>();
    //     foreach (EditorBuildSettingsScene scene in EditorBuildSettings.scenes)
    //     {
    //         if (scene.enabled)
    //         {
    //             scenes.Add(scene.path);
    //         }
    //     }
    //     
    //     if (scenes.Count == 0)
    //     {
    //         throw new System.Exception("❌ 没有找到可构建的场景");
    //     }
    //     
    //     // 构建APK
    //     BuildPlayerOptions buildOptions = new BuildPlayerOptions();
    //     buildOptions.scenes = scenes.ToArray();
    //     buildOptions.locationPathName = apkFullPath;
    //     buildOptions.target = BuildTarget.Android;
    //     buildOptions.options = BuildOptions.None;
    //     
    //     BuildPipeline.BuildPlayer(buildOptions);
    //     
    //     Debug.Log($"✅ APK构建完成: {apkFullPath}");
    // }
    //
    // private void ClearDirectory(string directoryPath)
    // {
    //     if (!Directory.Exists(directoryPath)) return;
    //     
    //     string[] files = Directory.GetFiles(directoryPath);
    //     string[] folders = Directory.GetDirectories(directoryPath);
    //     
    //     foreach (string file in files)
    //     {
    //         File.Delete(file);
    //     }
    //     foreach (string folder in folders)
    //     {
    //         Directory.Delete(folder, true);
    //     }
    //     
    //     Debug.Log($"🗑️ 已清空目录: {directoryPath}");
    // }
    //
    // private string GetPlatformName(BuildTarget target)
    // {
    //     switch (target)
    //     {
    //         case BuildTarget.Android: return "Android";
    //         case BuildTarget.iOS: return "iOS";
    //         case BuildTarget.StandaloneWindows:
    //         case BuildTarget.StandaloneWindows64: return "StandaloneWindows64";
    //         case BuildTarget.StandaloneOSX: return "StandaloneOSX";
    //         default: return target.ToString();
    //     }
    // }
}