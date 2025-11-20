using System.IO;
using System.Linq;
using UnityEngine;
using UnityEditor;

/// <summary>
/// 构建工具面板 - 继承自BaseToolPanel
/// </summary>
public class BuildToolPanel : BaseToolPanel
{
    private static string projectRoot => Directory.GetParent(Application.dataPath).FullName;
    public override string PanelName => "打包构建";
    public override string PanelIcon => "📦";
    public override string Description => "Unity项目构建管理工具，支持离线包、热更新包等多种构建方式";

    // 面板状态
    private bool _showBuildSettings = true;
    private bool _showOfflineBuilds = true;
    private bool _showHotfixBuilds = true;

    // 构建统计
    private BuildStatistics _buildStats = new BuildStatistics();

    // 构建配置 - 直接存储在类中
// 获取项目根目录
    private static string ProjectRoot = Directory.GetParent(Application.dataPath).FullName;

// 构建配置
    public static string AotDllDir;
    public static string JitDllDir ;
    public static string VersionFilePath;
    public static string OfflineModeSymbol;
    public static string AssetBundleSymbol;
    public static string AotDllsString;
    public static string JitDllsString;

    // 新增的路径配置
    public static string GitBashPath;
    public static string BuildCleanScriptPath;
    public static string SeverSyncScriptPath;
    public static string LogPath;
    public static string BuildLogsDir;

    static BuildToolPanel()
    {
    ProjectRoot = Directory.GetParent(Application.dataPath).FullName;

// 构建配置
     AotDllDir = Path.Combine(Application.dataPath, "JIT", "PakageAsset", "AOTDLL");
     JitDllDir = Path.Combine(Application.dataPath, "JIT", "PakageAsset", "JITDLL");
     VersionFilePath = Path.Combine(ProjectRoot, "SaveAsset", "BuildEditor", "In", "Buildversion.txt");
     OfflineModeSymbol = "RESOURCE_OFFLINE";
     AssetBundleSymbol = "RESOURCE_ASSETBUNDLE";
     AotDllsString = "System.Core.dll,System.dll,mscorlib.dll";
     JitDllsString = "HotUpdate.dll";

    // 新增的路径配置
     GitBashPath = @"C:\Program Files\Git\bin\bash.exe";
     BuildCleanScriptPath = Path.Combine(ProjectRoot, "SaveAsset", "BuildEditor", "In", "BuildCleanSeverRes.sh");
    SeverSyncScriptPath = Path.Combine(ProjectRoot, "SaveAsset", "BuildEditor", "In", "SeverSyncRes.sh");
     LogPath = Path.Combine(ProjectRoot, "SaveAsset", "BuildEditor", "Out", "sync_log.txt");
     BuildLogsDir = Path.Combine(ProjectRoot, "SaveAsset", "BuildEditor", "Out");
    }

    public override void OnGUI()
    {
        // 构建设置（移到最上面）
        _showBuildSettings = DrawFoldoutGroup("⚙️ 构建设置", _showBuildSettings, DrawBuildSettings);

        GUILayout.Space(10);

        // 构建状态概览
        DrawBuildStatusOverview();

        GUILayout.Space(10);

        // 离线包构建
        _showOfflineBuilds = DrawFoldoutGroup("💿 离线包构建", _showOfflineBuilds, DrawOfflineBuilds);

        // 热更新包构建
        _showHotfixBuilds = DrawFoldoutGroup("🔥 热更新包构建", _showHotfixBuilds, DrawHotfixBuilds);
    }

    /// <summary>
    /// 绘制构建状态概览
    /// </summary>
    private void DrawBuildStatusOverview()
    {
        EditorGUILayout.BeginVertical("box");
        GUILayout.Label("📊 构建状态概览", EditorStyles.boldLabel);

        EditorGUILayout.BeginHorizontal();

        // 左侧信息
        EditorGUILayout.BeginVertical();
        try
        {
            GUILayout.Label($"当前平台：{EditorUserBuildSettings.activeBuildTarget}", EditorStyles.miniLabel);
            GUILayout.Label($"构建模式：{(EditorUserBuildSettings.development ? "开发模式" : "发布模式")}", EditorStyles.miniLabel);
            GUILayout.Label($"最后构建：{_buildStats.LastBuildTime}", EditorStyles.miniLabel);
        }
        catch (System.Exception e)
        {
            GUILayout.Label($"状态获取失败: {e.Message}", EditorStyles.miniLabel);
        }
        EditorGUILayout.EndVertical();

        // 右侧按钮
        EditorGUILayout.BeginVertical();
        if (GUILayout.Button("📁 打开构建目录", GUILayout.Width(120)))
        {
            OpenBuildDirectory();
        }
        if (GUILayout.Button("📦 打开AB包目录", GUILayout.Width(120)))
        {
            OpenABPackagesDirectory();
        }
        EditorGUILayout.EndVertical();

        EditorGUILayout.EndHorizontal();
        EditorGUILayout.EndVertical();
    }

    /// <summary>
    /// 绘制构建设置
    /// </summary>
    private void DrawBuildSettings()
    {
        EditorGUILayout.BeginVertical("box");

        // 路径设置
        GUILayout.Label("📁 路径设置", EditorStyles.boldLabel);

        // AOT/JIT DLL目录
        DrawPathField("AOT DLL目录:", ref AotDllDir, true);
        DrawPathField("JIT DLL目录:", ref JitDllDir, true);
        DrawPathField("版本文件路径:", ref VersionFilePath, false);

        GUILayout.Space(5);

        // 新增的工具路径
        GUILayout.Label("🔧 工具路径", EditorStyles.boldLabel);
        DrawPathField("Git Bash路径:", ref GitBashPath, false);
        DrawPathField("清理脚本路径:", ref BuildCleanScriptPath, false);
        DrawPathField("同步脚本路径:", ref SeverSyncScriptPath, false);
        DrawPathField("日志文件路径:", ref LogPath, false);
        DrawPathField("构建日志目录:", ref BuildLogsDir, true);

        GUILayout.Space(10);

        // 编译符号设置
        GUILayout.Label("🔧 编译符号", EditorStyles.boldLabel);

        EditorGUILayout.BeginHorizontal();
        GUILayout.Label("离线模式符号:", GUILayout.Width(100));
        OfflineModeSymbol = GUILayout.TextField(OfflineModeSymbol, EditorStyles.textField, GUILayout.ExpandWidth(true));
        GUILayout.Space(30); // 与选择按钮宽度对齐
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();
        GUILayout.Label("资源包符号:", GUILayout.Width(100));
        AssetBundleSymbol = GUILayout.TextField(AssetBundleSymbol, EditorStyles.textField, GUILayout.ExpandWidth(true));
        GUILayout.Space(30); // 与选择按钮宽度对齐
        EditorGUILayout.EndHorizontal();

        GUILayout.Space(5);

        EditorGUILayout.BeginHorizontal();
        GUILayout.Label("当前符号:", GUILayout.Width(100));
        var symbols = PlayerSettings.GetScriptingDefineSymbolsForGroup(EditorUserBuildSettings.selectedBuildTargetGroup);
        GUILayout.Label(string.IsNullOrEmpty(symbols) ? "无" : symbols, EditorStyles.helpBox, GUILayout.ExpandWidth(true));
        GUILayout.Space(30); // 与选择按钮宽度对齐
        EditorGUILayout.EndHorizontal();

        GUILayout.Space(10);

        // DLL列表设置
        GUILayout.Label("📚 DLL列表", EditorStyles.boldLabel);

        EditorGUILayout.BeginHorizontal();
        GUILayout.Label("AOT DLL列表:", GUILayout.Width(100));
        AotDllsString = GUILayout.TextField(AotDllsString, EditorStyles.textField, GUILayout.ExpandWidth(true));
        GUILayout.Space(30); // 与选择按钮宽度对齐
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();
        GUILayout.Label("JIT DLL列表:", GUILayout.Width(100));
        JitDllsString = GUILayout.TextField(JitDllsString, EditorStyles.textField, GUILayout.ExpandWidth(true));
        GUILayout.Space(30); // 与选择按钮宽度对齐
        EditorGUILayout.EndHorizontal();

        GUILayout.Space(10);

        // 操作按钮
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("🔄 重置为默认值", GUILayout.Width(120)))
        {
            ResetToDefaults();
        }
        if (GUILayout.Button("📁 查看版本文件", GUILayout.Width(120)))
        {
            OpenVersionFile();
        }
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.EndVertical();
    }

    /// <summary>
    /// 绘制路径字段（带文件夹选择器）
    /// </summary>
    private void DrawPathField(string label, ref string path, bool isFolder)
    {
        EditorGUILayout.BeginHorizontal();
        GUILayout.Label(label, GUILayout.Width(100));

        // 显示当前路径 - 使用固定宽度以与输入框对齐
        string displayPath = string.IsNullOrEmpty(path) ? "未设置" : path;
        GUILayout.Label(displayPath, EditorStyles.helpBox, GUILayout.ExpandWidth(true));

        // 选择按钮 - 与输入框右边对齐
        if (GUILayout.Button("📂", GUILayout.Width(30)))
        {
            string selectedPath = "";
            if (isFolder)
            {
                // 选择文件夹
                selectedPath = EditorUtility.OpenFolderPanel($"选择{label}", path, "");
            }
            else
            {
                // 选择文件
                string directory = string.IsNullOrEmpty(path) ? Application.dataPath : System.IO.Path.GetDirectoryName(path);
                string extension = System.IO.Path.GetExtension(path);
                selectedPath = EditorUtility.OpenFilePanel($"选择{label}", directory, extension.TrimStart('.'));
            }

            if (!string.IsNullOrEmpty(selectedPath))
            {
                path = selectedPath;
            }
        }

        EditorGUILayout.EndHorizontal();
    }

    /// <summary>
    /// 绘制离线包构建
    /// </summary>
    private void DrawOfflineBuilds()
    {
        DrawButtonGroup(
            "完整离线包",
            "",
            new ButtonInfo("📱 构建全量包(离线)", () => {
                BuildPipelineEditor.BuildOfflineAPK();
                _buildStats.RecordBuild("离线全量包");
            }, null, true, 35)
        );
    }

    /// <summary>
    /// 绘制热更新包构建
    /// </summary>
    private void DrawHotfixBuilds()
    {
        DrawButtonGroup(
            "基础包构建",
            "",
            new ButtonInfo("📦 构建全量包APK(热更)", () => {
                BuildPipelineEditor.BuildFullPackageAPK();
                _buildStats.RecordBuild("热更全量包");
            }, null, true, 35),

            new ButtonInfo("🗃️ 构建空包APK(热更)", () => {
                BuildPipelineEditor.BuildNulllPackageAPK();
                _buildStats.RecordBuild("热更空包");
            }, null, true, 35)
        );

        GUILayout.Space(8);

        DrawButtonGroup(
            "增量更新包",
            "",
            new ButtonInfo("🔄 构建增量包", () => {
                BuildPipelineEditor.BuildIncrementalPackageNoAPK();
                _buildStats.RecordBuild("增量包");
            }, null, true, 35)
        );
    }

    #region 私有方法

    private void OpenBuildDirectory()
    {
        string buildPath = System.IO.Path.GetFullPath("Build");
        if (System.IO.Directory.Exists(buildPath))
        {
            EditorUtility.RevealInFinder(buildPath);
        }
        else
        {
            EditorUtility.DisplayDialog("提示", "构建目录不存在，请先执行构建操作", "确定");
        }
    }

    private void OpenABPackagesDirectory()
    {
        string abPath = System.IO.Path.Combine(Application.dataPath, "../AssetBundles");
        if (System.IO.Directory.Exists(abPath))
        {
            EditorUtility.RevealInFinder(abPath);
        }
        else
        {
            // 尝试其他可能的AB包路径
            string[] possiblePaths = {
                System.IO.Path.Combine(Application.dataPath, "../Bundles"),
                System.IO.Path.Combine(Application.dataPath, "../StreamingAssets"),
                System.IO.Path.Combine(Application.streamingAssetsPath, "")
            };

            foreach (string path in possiblePaths)
            {
                if (System.IO.Directory.Exists(path))
                {
                    EditorUtility.RevealInFinder(path);
                    return;
                }
            }

            EditorUtility.DisplayDialog("提示", "AB包目录不存在，可能的路径:\n- AssetBundles\n- Bundles\n- StreamingAssets\n\n请先执行资源包构建操作", "确定");
        }
    }

    private void OpenVersionFile()
    {
        try
        {
            if (System.IO.File.Exists(VersionFilePath))
            {
                UnityEditorInternal.InternalEditorUtility.OpenFileAtLineExternal(VersionFilePath, 1);
            }
            else
            {
                EditorUtility.DisplayDialog("提示", "版本文件不存在，请先生成版本号", "确定");
            }
        }
        catch (System.Exception e)
        {
            EditorUtility.DisplayDialog("错误", $"打开版本文件失败: {e.Message}", "确定");
        }
    }

    // 重置为默认值
    private void ResetToDefaults()
    {
        if (EditorUtility.DisplayDialog("重置确认", "将重置所有构建设置为默认值，是否继续？", "确认", "取消"))
        {
           
            // 重置新增的路径配置
            AotDllDir = Path.Combine(Application.dataPath, "JIT", "PakageAsset", "AOTDLL");
            JitDllDir = Path.Combine(Application.dataPath, "JIT", "PakageAsset", "JITDLL");
            VersionFilePath = Path.Combine(ProjectRoot, "SaveAsset", "BuildEditor", "In", "Buildversion.txt");
            OfflineModeSymbol = "RESOURCE_OFFLINE";
            AssetBundleSymbol = "RESOURCE_ASSETBUNDLE";
            AotDllsString = "System.Core.dll,System.dll,mscorlib.dll";
            JitDllsString = "HotUpdate.dll";

            GitBashPath = @"C:\Program Files\Git\bin\bash.exe";
            BuildCleanScriptPath = Path.Combine(ProjectRoot, "SaveAsset", "BuildEditor", "In", "BuildCleanSeverRes.sh");
            SeverSyncScriptPath = Path.Combine(ProjectRoot, "SaveAsset", "BuildEditor", "In", "SeverSyncRes.sh");
            LogPath = Path.Combine(ProjectRoot, "SaveAsset", "BuildEditor", "Out", "sync_log.txt");
            BuildLogsDir = Path.Combine(ProjectRoot, "SaveAsset", "BuildEditor", "Out");
            
            EditorUtility.DisplayDialog("完成", "构建设置已重置为默认值", "确定");
        }
    }

    #endregion

    #region BuildHelper兼容方法 

    // 兼容原有API，直接在BuildToolPanel中提供
    public static string GetAOTDLLDir() => AotDllDir;
    public static string GetJITDllDir() => JitDllDir;
    public static string VersionFilePath_Static => VersionFilePath;

    public const string OFFLINE_MODE_SYMBOL = "RESOURCE_OFFLINE"; // 保留常量用于兼容性
    public const string ASSETBUNDLE_MODE_SYMBOL = "RESOURCE_ASSETBUNDLE"; // 保留常量用于兼容性

    public static System.Collections.Generic.List<string> GetAotDLLNames()
    {
        return AotDllsString.Split(',').Where(s => !string.IsNullOrEmpty(s.Trim())).Select(s => s.Trim()).ToList();
    }

    public static System.Collections.Generic.List<string> GetJITDLLNames()
    {
        return JitDllsString.Split(',').Where(s => !string.IsNullOrEmpty(s.Trim())).Select(s => s.Trim()).ToList();
    }

    // 动态获取符号
    public static string GetOfflineModeSymbol() => OfflineModeSymbol;
    public static string GetAssetBundleModeSymbol() => AssetBundleSymbol;

    // 新增的路径访问方法
    public static string GetGitBashPath() => GitBashPath;
    public static string GetBuildCleanScriptPath() => BuildCleanScriptPath;
    public static string GetSeverSyncScriptPath() => SeverSyncScriptPath;
    public static string GetLogPath() => LogPath;
    public static string GetBuildLogsDir() => BuildLogsDir;

    #endregion

    /// <summary>
    /// 构建统计信息
    /// </summary>
    private class BuildStatistics
    {
        public string LastBuildTime { get; private set; } = "暂无记录";
        public int TotalBuilds { get; private set; } = 0;

        public void RecordBuild(string buildType)
        {
            LastBuildTime = $"{System.DateTime.Now:MM-dd HH:mm} ({buildType})";
            TotalBuilds++;
        }
    }
}