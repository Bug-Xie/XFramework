using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEditor.SceneManagement;
using YooAsset;

public class BuildPackageWindow : EditorWindow
{
    private AOTGlobalConfig aotGlobalConfig;
    private BuildTarget selectedPlatform = BuildTarget.Android;
    private EPlayMode selectedPlayMode = EPlayMode.HostPlayMode;
    private string serverFolderPath = "";
    private Vector2 scrollPosition = Vector2.zero;

    // 平台数组（用于下拉框）
    private BuildTarget[] availablePlatforms = new BuildTarget[]
    {
        BuildTarget.Android,
        BuildTarget.iOS,
        BuildTarget.StandaloneWindows64,
        BuildTarget.StandaloneOSX,
    };

    private EPlayMode[] availablePlayModes = new EPlayMode[]
    {
        EPlayMode.OfflinePlayMode,
        EPlayMode.HostPlayMode,
        EPlayMode.WebPlayMode,
    };

    // 临时保存原始配置
    private EPlayMode originalPlayMode;
    private string originalHostServerURL;

    // 平台名称映射
    private static readonly Dictionary<BuildTarget, string> PlatformNames = new Dictionary<BuildTarget, string>
    {
        { BuildTarget.Android, "Android" },
        { BuildTarget.iOS, "iOS" },
        { BuildTarget.StandaloneWindows64, "Windows64" },
        { BuildTarget.StandaloneOSX, "OSX" },
    };

    [MenuItem("XFramework/Build/Open Build Package Window")]
    public static void ShowWindow()
    {
        GetWindow<BuildPackageWindow>("Build Package");
    }

    private void OnEnable()
    {
        LoadAOTGlobalConfig();
    }

    private void OnGUI()
    {
        GUILayout.Label("XFramework Build Package", EditorStyles.largeLabel);
        EditorGUILayout.Space();

        scrollPosition = GUILayout.BeginScrollView(scrollPosition);

        // AOTGlobalConfig选择
        EditorGUILayout.LabelField("Configuration", EditorStyles.boldLabel);
        aotGlobalConfig = (AOTGlobalConfig)EditorGUILayout.ObjectField(
            "AOT Global Config",
            aotGlobalConfig,
            typeof(AOTGlobalConfig),
            false);

        if (aotGlobalConfig == null)
        {
            EditorGUILayout.HelpBox("Please assign AOTGlobalConfig asset", MessageType.Error);
            GUILayout.EndScrollView();
            return;
        }

        EditorGUILayout.Space();

        // 构建设置
        EditorGUILayout.LabelField("Build Settings", EditorStyles.boldLabel);
        BuildTarget currentTarget = EditorUserBuildSettings.activeBuildTarget;
        EditorGUILayout.LabelField($"Current Platform: {currentTarget}");

        EditorGUILayout.Space();

        // 平台选择下拉框 - 与标签在同一行
        int platformIndex = System.Array.IndexOf(availablePlatforms, selectedPlatform);
        if (platformIndex < 0) platformIndex = 0;

        int newPlatformIndex = EditorGUILayout.Popup("Target Platform", platformIndex, GetPlatformDisplayNames());
        if (newPlatformIndex != platformIndex)
        {
            selectedPlatform = availablePlatforms[newPlatformIndex];
        }

        EditorGUILayout.Space();

        // 打包模式选择下拉框 - 与标签在同一行
        int playModeIndex = System.Array.IndexOf(availablePlayModes, selectedPlayMode);
        if (playModeIndex < 0) playModeIndex = 1;

        int newPlayModeIndex = EditorGUILayout.Popup("Play Mode", playModeIndex, GetPlayModeDisplayNames());
        if (newPlayModeIndex != playModeIndex)
        {
            selectedPlayMode = availablePlayModes[newPlayModeIndex];
        }

        EditorGUILayout.Space();

        // 服务器文件夹路径输入 - 标签和输入在同一行
        EditorGUILayout.LabelField("Server Settings", EditorStyles.boldLabel);

        EditorGUILayout.BeginHorizontal();
        serverFolderPath = EditorGUILayout.TextField("Server Folder Path", serverFolderPath);
        if (GUILayout.Button("Browse", GUILayout.Width(80)))
        {
            string path = EditorUtility.OpenFolderPanel("Select Server Folder", "", "");
            if (!string.IsNullOrEmpty(path))
            {
                serverFolderPath = path;
            }
        }
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space();

        // 当前配置显示
        DisplayCurrentConfig();

        EditorGUILayout.Space();

        // 打包按钮
        EditorGUILayout.LabelField("Build Options", EditorStyles.boldLabel);

        // 热更包按钮 - 放在前面（蓝色）
        GUI.backgroundColor = new Color(0.5f, 0.8f, 1f);
        if (GUILayout.Button("Build Hot Update Only (Step 1-6)", GUILayout.Height(40)))
        {
            if (ValidateInputs())
            {
                StartHotUpdateBuild();
            }
        }

        EditorGUILayout.Space();

        // 完整打包按钮 - 放在后面（绿色）
        GUI.backgroundColor = Color.green;
        if (GUILayout.Button("Build Complete Package (All Steps)", GUILayout.Height(40)))
        {
            if (ValidateInputs())
            {
                StartCompletePackageBuild();
            }
        }
        GUI.backgroundColor = Color.white;

        GUILayout.EndScrollView();
    }

    private string[] GetPlatformDisplayNames()
    {
        string[] names = new string[availablePlatforms.Length];
        for (int i = 0; i < availablePlatforms.Length; i++)
        {
            names[i] = availablePlatforms[i].ToString();
        }
        return names;
    }

    private string[] GetPlayModeDisplayNames()
    {
        string[] names = new string[availablePlayModes.Length];
        for (int i = 0; i < availablePlayModes.Length; i++)
        {
            string mode = availablePlayModes[i].ToString();
            if (availablePlayModes[i] == EPlayMode.OfflinePlayMode)
                mode += " (Local only)";
            else if (availablePlayModes[i] == EPlayMode.HostPlayMode)
                mode += " (Server required)";
            else if (availablePlayModes[i] == EPlayMode.WebPlayMode)
                mode += " (Web server required)";
            names[i] = mode;
        }
        return names;
    }

    private void DisplayCurrentConfig()
    {
        EditorGUILayout.LabelField("Current Configuration", EditorStyles.boldLabel);
        EditorGUILayout.LabelField($"Play Mode: {aotGlobalConfig.aotGlobalYooAssetConfig.playMode}");
        EditorGUILayout.LabelField($"Selected Mode: {selectedPlayMode}");
        EditorGUILayout.LabelField($"Package Name: {aotGlobalConfig.aotGlobalYooAssetConfig.packageName}");
        EditorGUILayout.LabelField($"Server URL: {aotGlobalConfig.aotGlobalYooAssetConfig.hostServerURL}");
    }

    private void SetTargetPlatform(BuildTarget target)
    {
        if (EditorUserBuildSettings.activeBuildTarget != target)
        {
            EditorUtility.DisplayProgressBar("Switching Platform", $"Switching to {target}...", 0.5f);
            EditorUserBuildSettings.SwitchActiveBuildTarget(BuildPipeline.GetBuildTargetGroup(target), target);
            EditorUtility.ClearProgressBar();
        }
    }

    private bool ValidateInputs()
    {
        if (aotGlobalConfig == null)
        {
            EditorUtility.DisplayDialog("Error", "Please assign AOTGlobalConfig asset", "OK");
            return false;
        }

        if (selectedPlayMode == EPlayMode.OfflinePlayMode)
        {
            return true;
        }

        if (string.IsNullOrEmpty(serverFolderPath) || !Directory.Exists(serverFolderPath))
        {
            EditorUtility.DisplayDialog("Error", "Please select a valid server folder path", "OK");
            return false;
        }

        return true;
    }

    private void StartCompletePackageBuild()
    {
        try
        {
            Debug.Log("========== Complete Package Build Started (All Steps) ==========");

            // Step 1: 检查并切换平台，修改配置
            ExecuteStep1_SwitchPlatformAndConfig();

            // Step 2: 执行HybridCLR构建
            ExecuteStep2_HybridCLRBuild();

            // Step 3: 复制DLL文件
            ExecuteStep3_CopyDllFiles();

            // Step 4: 执行AssetBundle构建
            ExecuteStep4_AssetBundleBuild();

            // Step 5: 复制资源到服务器
            ExecuteStep5_CopyToServer(null);

            // Step 6: 还原配置
            ExecuteStep6_RestoreConfig();

            // Step 7: 执行游戏包构建
            ExecuteStep7_BuildGame();

            // Step 8: 打开输出文件夹
            ExecuteStep8_OpenOutputFolder();

            Debug.Log("========== Complete Package Build Completed Successfully ==========");
            EditorUtility.DisplayDialog("Success", "Complete package build completed successfully!", "OK");
        }
        catch (Exception ex)
        {
            EditorUtility.DisplayDialog("Build Error", $"Build process failed: {ex.Message}\n\nStack: {ex.StackTrace}", "OK");
            Debug.LogError($"Build process error: {ex}");
            // 尝试还原配置
            try
            {
                ExecuteStep6_RestoreConfig();
            }
            catch { }
        }
    }

    private void StartHotUpdateBuild()
    {
        try
        {
            Debug.Log("========== Hot Update Build Started (Step 1-6) ==========");

            // Step 1: 检查并切换平台，修改配置
            ExecuteStep1_SwitchPlatformAndConfig();

            // Step 2: 执行HybridCLR构建
            ExecuteStep2_HybridCLRBuild();

            // Step 3: 复制DLL文件
            ExecuteStep3_CopyDllFiles();

            // Step 4: 执行AssetBundle构建
            ExecuteStep4_AssetBundleBuild();

            // Step 5: 复制资源到服务器
            ExecuteStep5_CopyToServer(null);

            // Step 6: 还原配置
            ExecuteStep6_RestoreConfig();

            Debug.Log("========== Hot Update Build Completed Successfully ==========");
            EditorUtility.DisplayDialog("Success", "Hot update package build completed successfully!\nResources have been uploaded to server.", "OK");
        }
        catch (Exception ex)
        {
            EditorUtility.DisplayDialog("Build Error", $"Build process failed: {ex.Message}\n\nStack: {ex.StackTrace}", "OK");
            Debug.LogError($"Build process error: {ex}");
            // 尝试还原配置
            try
            {
                ExecuteStep6_RestoreConfig();
            }
            catch { }
        }
    }

    private void ExecuteStep1_SwitchPlatformAndConfig()
    {
        Debug.Log("Step 1: Switching platform and updating configuration...");

        // 切换平台到选定的平台
        if (EditorUserBuildSettings.activeBuildTarget != selectedPlatform)
        {
            SetTargetPlatform(selectedPlatform);
        }

        // 保存原始配置
        originalPlayMode = aotGlobalConfig.aotGlobalYooAssetConfig.playMode;
        originalHostServerURL = aotGlobalConfig.aotGlobalYooAssetConfig.hostServerURL;

        // 修改配置
        bool configChanged = false;

        if (aotGlobalConfig.aotGlobalYooAssetConfig.playMode != selectedPlayMode)
        {
            aotGlobalConfig.aotGlobalYooAssetConfig.playMode = selectedPlayMode;
            configChanged = true;
            Debug.Log($"Updated PlayMode from {originalPlayMode} to {selectedPlayMode}");
        }

        // 更新服务器URL
        if (selectedPlayMode != EPlayMode.OfflinePlayMode && !string.IsNullOrEmpty(serverFolderPath))
        {
            string platformName = GetPlatformName(selectedPlatform);
            string newHostServerURL = UpdateHostServerURL(originalHostServerURL, platformName);

            if (aotGlobalConfig.aotGlobalYooAssetConfig.hostServerURL != newHostServerURL)
            {
                aotGlobalConfig.aotGlobalYooAssetConfig.hostServerURL = newHostServerURL;
                configChanged = true;
                Debug.Log($"Updated HostServerURL to {newHostServerURL}");
            }
        }

        if (configChanged)
        {
            EditorUtility.SetDirty(aotGlobalConfig);
            AssetDatabase.SaveAssets();
            Debug.Log("Configuration saved");
        }
    }

    private void ExecuteStep2_HybridCLRBuild()
    {
        Debug.Log("Step 2: Executing HybridCLR Generate ALL...");

        // 执行 HybridCLR的Generate ALL menu
        try
        {
            EditorApplication.ExecuteMenuItem("HybridCLR/Generate/All");
            Debug.Log("HybridCLR Generate ALL executed");

            // 等待一段时间让HybridCLR完成构建
            System.Threading.Thread.Sleep(2000);
        }
        catch (Exception ex)
        {
            Debug.LogWarning($"Could not execute HybridCLR menu: {ex.Message}");
            Debug.Log("Make sure HybridCLR is installed and imported correctly");
        }
    }

    private void ExecuteStep3_CopyDllFiles()
    {
        Debug.Log("Step 3: Copying DLL files...");

        string projectPath = Path.GetDirectoryName(Application.dataPath);
        string hybridCLRDataPath = Path.Combine(projectPath, "HybridCLRData");
        string scriptDllFolder = Path.Combine(Application.dataPath, aotGlobalConfig.aotGlobalHybridClrConfig.hotScriptDllPath);

        // 清空ScriptDLL文件夹
        if (Directory.Exists(scriptDllFolder))
        {
            DirectoryInfo dirInfo = new DirectoryInfo(scriptDllFolder);
            foreach (FileInfo file in dirInfo.GetFiles())
            {
                file.Delete();
                Debug.Log($"Deleted: {file.Name}");
            }
        }
        else
        {
            Directory.CreateDirectory(scriptDllFolder);
        }

        string platformName = GetPlatformName(selectedPlatform);

        // 复制AOT DLL文件
        string aotDllSourcePath = Path.Combine(hybridCLRDataPath, "AssembliesPostIl2CppStrip", platformName);
        if (Directory.Exists(aotDllSourcePath) && aotGlobalConfig.aotGlobalHybridClrConfig.AOTScriptDllNames != null)
        {
            foreach (string dllName in aotGlobalConfig.aotGlobalHybridClrConfig.AOTScriptDllNames)
            {
                string sourceFile = Path.Combine(aotDllSourcePath, dllName);
                if (File.Exists(sourceFile))
                {
                    string fileName = Path.GetFileNameWithoutExtension(dllName);
                    string destFile = Path.Combine(scriptDllFolder, $"{fileName}.dll.bytes");
                    File.Copy(sourceFile, destFile, true);
                    Debug.Log($"Copied AOT DLL: {dllName} -> {fileName}.dll.bytes");
                }
                else
                {
                    Debug.LogWarning($"AOT DLL not found: {sourceFile}");
                }
            }
        }
        else
        {
            Debug.LogWarning($"AOT DLL source path not found: {aotDllSourcePath}");
        }

        // 复制HotUpdate DLL文件
        string hotUpdateDllSourcePath = Path.Combine(hybridCLRDataPath, "HotUpdateDlls", platformName);
        if (Directory.Exists(hotUpdateDllSourcePath))
        {
            string hotDllName = aotGlobalConfig.aotGlobalHybridClrConfig.hotScriptDllName;
            string sourceFile = Path.Combine(hotUpdateDllSourcePath, hotDllName);
            if (File.Exists(sourceFile))
            {
                string fileName = Path.GetFileNameWithoutExtension(hotDllName);
                string destFile = Path.Combine(scriptDllFolder, $"{fileName}.dll.bytes");
                File.Copy(sourceFile, destFile, true);
                Debug.Log($"Copied Hot Update DLL: {hotDllName} -> {fileName}.dll.bytes");
            }
            else
            {
                Debug.LogWarning($"Hot Update DLL not found: {sourceFile}");
            }
        }
        else
        {
            Debug.LogWarning($"Hot Update DLL source path not found: {hotUpdateDllSourcePath}");
        }

        AssetDatabase.Refresh();
        Debug.Log("DLL files copied successfully");
    }

    private void ExecuteStep4_AssetBundleBuild()
    {
        Debug.Log("Step 4: Executing AssetBundle build...");

        try
        {
            // 执行YooAsset菜单打开AssetBundle Builder窗口
            EditorApplication.ExecuteMenuItem("YooAsset/AssetBundle Builder");
            Debug.Log("AssetBundle Builder window opened");

            // 显示提示用户需要手动操作
            EditorUtility.DisplayDialog(
                "AssetBundle Build",
                "AssetBundle Builder window has been opened.\n\n" +
                "Please click the 'ClickBuild' button to start building AssetBundles.\n\n" +
                "The build process will wait for completion...",
                "OK");

            // 等待AssetBundle构建完成
            Debug.Log("Waiting for AssetBundle build to complete...");
            if (!BuildPackageUtility.WaitForAssetBundleBuild(300)) // 等待最多5分钟
            {
                throw new Exception("AssetBundle build timeout");
            }

            Debug.Log("AssetBundle build completed successfully");
        }
        catch (Exception ex)
        {
            Debug.LogError($"Could not execute YooAsset menu: {ex.Message}");
            throw new Exception($"Failed to complete AssetBundle build: {ex.Message}");
        }
    }

    private void ExecuteStep5_CopyToServer(BuildPackageState state = null)
    {
        Debug.Log("Step 5: Copying assets to server...");

        if (selectedPlayMode == EPlayMode.OfflinePlayMode && state == null)
        {
            Debug.Log("OfflinePlayMode detected, skipping server copy");
            return;
        }

        // 如果从保存的状态恢复，使用保存的服务器路径
        string currentServerPath = state != null ? state.ServerFolderPath : serverFolderPath;
        string currentPlayModeStr = state != null ? state.PlayModeValue : selectedPlayMode.ToString();
        EPlayMode currentPlayMode = (EPlayMode)Enum.Parse(typeof(EPlayMode), currentPlayModeStr);

        if (currentPlayMode == EPlayMode.OfflinePlayMode)
        {
            Debug.Log("OfflinePlayMode detected, skipping server copy");
            return;
        }

        string projectPath = Path.GetDirectoryName(Application.dataPath);
        string bundlesPath = Path.Combine(projectPath, "Bundles");
        string platformName = state != null ? state.PlatformName : GetPlatformName(selectedPlatform);
        string packageName = aotGlobalConfig.aotGlobalYooAssetConfig.packageName;

        // 查找最新的版本文件夹
        string platformBundlesPath = Path.Combine(bundlesPath, platformName, packageName);
        if (!Directory.Exists(platformBundlesPath))
        {
            Debug.LogError($"Platform bundles path not found: {platformBundlesPath}");
            return;
        }

        DirectoryInfo parentDir = new DirectoryInfo(platformBundlesPath);
        DirectoryInfo[] versionDirs = parentDir.GetDirectories();

        if (versionDirs.Length == 0)
        {
            Debug.LogError("No version directories found in bundles path");
            return;
        }

        // 获取最新的版本目录（按修改时间）
        System.Array.Sort(versionDirs, (a, b) => b.LastWriteTime.CompareTo(a.LastWriteTime));
        string latestVersionPath = versionDirs[0].FullName;

        Debug.Log($"Found latest version path: {latestVersionPath}");

        // 创建服务器资源文件夹
        string serverPlatformPath = Path.Combine(currentServerPath, platformName);
        if (!Directory.Exists(serverPlatformPath))
        {
            Directory.CreateDirectory(serverPlatformPath);
            Debug.Log($"Created server platform path: {serverPlatformPath}");
        }

        // 清空服务器资源文件夹
        DirectoryInfo serverDirInfo = new DirectoryInfo(serverPlatformPath);
        foreach (FileInfo file in serverDirInfo.GetFiles())
        {
            file.Delete();
        }
        foreach (DirectoryInfo dir in serverDirInfo.GetDirectories())
        {
            dir.Delete(true);
        }
        Debug.Log($"Cleared server folder: {serverPlatformPath}");

        // 复制文件到服务器
        CopyDirectory(latestVersionPath, serverPlatformPath);
        Debug.Log($"Copied bundles to server: {serverPlatformPath}");
    }

    private void ExecuteStep6_RestoreConfig()
    {
        Debug.Log("Step 6: Restoring configuration...");

        if (aotGlobalConfig.aotGlobalYooAssetConfig.playMode != originalPlayMode)
        {
            aotGlobalConfig.aotGlobalYooAssetConfig.playMode = originalPlayMode;
            Debug.Log($"Restored PlayMode to {originalPlayMode}");
        }

        if (aotGlobalConfig.aotGlobalYooAssetConfig.hostServerURL != originalHostServerURL)
        {
            aotGlobalConfig.aotGlobalYooAssetConfig.hostServerURL = originalHostServerURL;
            Debug.Log($"Restored HostServerURL to {originalHostServerURL}");
        }

        EditorUtility.SetDirty(aotGlobalConfig);
        AssetDatabase.SaveAssets();
        Debug.Log("Configuration restored and saved");
    }

    private void ExecuteStep7_BuildGame()
    {
        Debug.Log("Step 7: Building game package...");

        string platformName = GetPlatformName(selectedPlatform);
        string projectPath = Path.GetDirectoryName(Application.dataPath);
        string versionName = DateTime.Now.ToString("yyyy-MM-dd-HHmm");
        string outputPath = Path.Combine(projectPath, "..", "OutPut", platformName, versionName);

        // 创建输出目录
        if (!Directory.Exists(outputPath))
        {
            Directory.CreateDirectory(outputPath);
            Debug.Log($"Created output directory: {outputPath}");
        }

        // 设置构建选项
        BuildPlayerOptions buildPlayerOptions = new BuildPlayerOptions();
        buildPlayerOptions.scenes = GetEnabledScenes();
        buildPlayerOptions.locationPathName = GetOutputFilePath(outputPath, platformName);
        buildPlayerOptions.target = selectedPlatform;
        buildPlayerOptions.options = BuildOptions.None;

        Debug.Log($"Building to: {buildPlayerOptions.locationPathName}");

        BuildReport report = BuildPipeline.BuildPlayer(buildPlayerOptions);

        if (report.summary.result == BuildResult.Succeeded)
        {
            Debug.Log("Game build completed successfully");
        }
        else
        {
            throw new Exception($"Game build failed: {report.summary.result}");
        }
    }

    private void ExecuteStep8_OpenOutputFolder()
    {
        Debug.Log("Step 8: Opening output folder...");

        string platformName = GetPlatformName(selectedPlatform);
        string projectPath = Path.GetDirectoryName(Application.dataPath);
        string versionName = DateTime.Now.ToString("yyyy-MM-dd-HHmm");
        string outputPath = Path.Combine(projectPath, "..", "OutPut", platformName, versionName);

        if (Directory.Exists(outputPath))
        {
            EditorUtility.RevealInFinder(outputPath);
            Debug.Log($"Opened output folder: {outputPath}");
        }
        else
        {
            Debug.LogWarning($"Output folder not found: {outputPath}");
        }
    }

    private string GetPlatformName(BuildTarget target)
    {
        if (PlatformNames.TryGetValue(target, out string name))
        {
            return name;
        }
        return target.ToString();
    }

    private string UpdateHostServerURL(string originalURL, string platformName)
    {
        if (string.IsNullOrEmpty(originalURL))
        {
            Debug.LogWarning($"资源路径为空");
            return $"http://192.168.1.167:8084/XFramework/{platformName}";
        }

        // 替换最后一个斜杠后面的内容为平台名
        int lastSlashIndex = originalURL.LastIndexOf('/');
        if (lastSlashIndex >= 0)
        {
            return originalURL.Substring(0, lastSlashIndex + 1) + platformName;
        }

        return originalURL;
    }

    private string[] GetEnabledScenes()
    {
        List<string> scenes = new List<string>();
        foreach (EditorBuildSettingsScene scene in EditorBuildSettings.scenes)
        {
            if (scene.enabled)
            {
                scenes.Add(scene.path);
            }
        }
        return scenes.ToArray();
    }

    private string GetOutputFilePath(string outputPath, string platformName)
    {
        switch (selectedPlatform)
        {
            case BuildTarget.Android:
                return Path.Combine(outputPath, "game.apk");
            case BuildTarget.iOS:
                return outputPath;
            case BuildTarget.StandaloneWindows64:
                return Path.Combine(outputPath, "game.exe");
            case BuildTarget.StandaloneOSX:
                return Path.Combine(outputPath, "game.app", "Contents", "MacOS", "game");
            default:
                return Path.Combine(outputPath, "game");
        }
    }

    private void CopyDirectory(string sourceDir, string destDir)
    {
        DirectoryInfo source = new DirectoryInfo(sourceDir);
        DirectoryInfo[] directories = source.GetDirectories();

        foreach (FileInfo file in source.GetFiles())
        {
            string dest = Path.Combine(destDir, file.Name);
            file.CopyTo(dest, true);
        }

        foreach (DirectoryInfo dir in directories)
        {
            string dest = Path.Combine(destDir, dir.Name);
            if (!Directory.Exists(dest))
            {
                Directory.CreateDirectory(dest);
            }
            CopyDirectory(dir.FullName, dest);
        }
    }

    private void LoadAOTGlobalConfig()
    {
        if (aotGlobalConfig == null)
        {
            string[] guids = AssetDatabase.FindAssets("AOTGlobalConfig t:AOTGlobalConfig");
            if (guids.Length > 0)
            {
                string path = AssetDatabase.GUIDToAssetPath(guids[0]);
                aotGlobalConfig = AssetDatabase.LoadAssetAtPath<AOTGlobalConfig>(path);
            }
        }
    }
}
