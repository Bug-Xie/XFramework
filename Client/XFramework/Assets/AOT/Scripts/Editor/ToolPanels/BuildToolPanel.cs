using System.Collections.Generic;
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

    // 构建配置 - 直接存储在类中
// 获取项目根目录
    public static string ProjectRoot = Directory.GetParent(Application.dataPath).FullName;

// 构建配置
    public static string AotDllDir;
    public static string JitDllDir;
    public static string AotDllsString;
    public static string JitDllsString;
    public static bool EnableLog;

    // 新增的路径配置
    public static string BuildLogsDir;
    public static string ApkOutputDir;

    static BuildToolPanel()
    {
        ProjectRoot = Directory.GetParent(Application.dataPath).FullName;
        Defaults();
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
            GUILayout.Label($"构建模式：{(EnableLog ? "开发模式" : "发布模式")}", EditorStyles.miniLabel);
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

        if (GUILayout.Button("📝 打开日志目录", GUILayout.Width(120)))
        {
            OpenBuildLogsDirectory();
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
        DrawPathField("AOTDLL目录:", ref AotDllDir, true);
        DrawPathField("JITDLL目录:", ref JitDllDir, true);
        DrawPathField("APK输出目录:", ref ApkOutputDir, true);
        DrawPathField("构建日志目录:", ref BuildLogsDir, true);

        GUILayout.Space(10);

        // 编译符号设置
        GUILayout.Label("🔧 编译符号", EditorStyles.boldLabel);
  
        
        GUILayout.Space(5);
        EditorGUILayout.BeginHorizontal();
        GUILayout.Label("当前符号:", GUILayout.Width(100));
        var symbols =
            PlayerSettings.GetScriptingDefineSymbolsForGroup(EditorUserBuildSettings.selectedBuildTargetGroup);
        GUILayout.Label(string.IsNullOrEmpty(symbols) ? "无" : symbols, EditorStyles.helpBox,
            GUILayout.ExpandWidth(true));
        GUILayout.Space(30); // 与选择按钮宽度对齐
        EditorGUILayout.EndHorizontal();

        GUILayout.Space(5);

        // Enable Log 切换
        EditorGUILayout.BeginHorizontal();
        GUILayout.Label("启用日志:", GUILayout.Width(86));
        EditorGUI.BeginChangeCheck();
        // 使用ToggleLeft让点击区域更大，包含文字部分
        EnableLog = EditorGUILayout.ToggleLeft(EnableLog ? "✅ 已启用" : "❌ 已禁用", EnableLog, GUILayout.ExpandWidth(true));
        if (EditorGUI.EndChangeCheck())
        {
            GUI.changed = true;
        }

        GUILayout.Space(30);
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
                string directory = string.IsNullOrEmpty(path)
                    ? Application.dataPath
                    : System.IO.Path.GetDirectoryName(path);
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
            new ButtonInfo("📱 构建全量包(离线)", () =>
            {
                EditorApplication.delayCall += () =>
                {
                    BuildPipelineEditor.BuildOfflineAPK();
                };
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
            new ButtonInfo("📦 构建全量包APK(热更)", () =>
            {
                EditorApplication.delayCall += () =>
                {
                    BuildPipelineEditor.BuildFullPackageAPK();
                };
            }, null, true, 35),
            new ButtonInfo("🗃️ 构建空包APK(热更)", () =>
            {
                EditorApplication.delayCall += () =>
                {
                    BuildPipelineEditor.BuildNulllPackageAPK();
                };
            }, null, true, 35)
        );

        GUILayout.Space(8);

        DrawButtonGroup(
            "增量更新包",
            "",
            new ButtonInfo("🔄 构建增量包", () =>
            {
                EditorApplication.delayCall += () =>
                {
                    BuildPipelineEditor.BuildIncrementalPackageNoAPK();
                };
            }, null, true, 35)
        );
    }

    #region 私有方法

    /// <summary>
    /// 直接打开目录（进入目录内部，而非选中目录）
    /// </summary>
    private void OpenDirectoryDirectly(string path)
    {
        string fullPath = System.IO.Path.GetFullPath(path);
        // 使用系统命令直接打开目录
        System.Diagnostics.Process.Start("explorer.exe", fullPath);
    }

    private void OpenBuildDirectory()
    {
        string buildPath = System.IO.Path.GetFullPath(ApkOutputDir);
        if (System.IO.Directory.Exists(buildPath))
        {
            OpenDirectoryDirectly(buildPath);
        }
        else
        {
            if (EditorUtility.DisplayDialog("提示", $"构建目录不存在:\n{buildPath}\n\n是否创建该目录？", "创建", "取消"))
            {
                try
                {
                    System.IO.Directory.CreateDirectory(buildPath);
                    OpenDirectoryDirectly(buildPath);
                }
                catch (System.Exception e)
                {
                    EditorUtility.DisplayDialog("错误", $"创建构建目录失败: {e.Message}", "确定");
                }
            }
        }
    }

    private void OpenABPackagesDirectory()
    {
        string abPath = System.IO.Path.GetFullPath(System.IO.Path.Combine(Application.dataPath, "../Bundles"));
        if (System.IO.Directory.Exists(abPath))
        {
            OpenDirectoryDirectly(abPath);
        }
        else
        {
            // 尝试其他可能的AB包路径
            string[] possiblePaths =
            {
                System.IO.Path.Combine(Application.dataPath, "../AssetBundles"),
                System.IO.Path.Combine(Application.dataPath, "../StreamingAssets"),
                System.IO.Path.Combine(Application.streamingAssetsPath, "")
            };

            foreach (string path in possiblePaths)
            {
                if (System.IO.Directory.Exists(path))
                {
                    OpenDirectoryDirectly(path);
                    return;
                }
            }

            EditorUtility.DisplayDialog("提示",
                "AB包目录不存在，可能的路径:\n- Bundles\n- AssetBundles\n- StreamingAssets\n\n请先执行资源包构建操作", "确定");
        }
    }

    private void OpenBuildLogsDirectory()
    {
        string logsPath =BuildLogsDir;
        if (System.IO.Directory.Exists(logsPath))
        {
            OpenDirectoryDirectly(logsPath);
        }
        else
        {
            // 如果目录不存在，询问是否创建
            if (EditorUtility.DisplayDialog("提示", $"日志目录不存在:\n{logsPath}\n\n是否创建该目录？", "创建", "取消"))
            {
                try
                {
                    System.IO.Directory.CreateDirectory(logsPath);
                    OpenDirectoryDirectly(logsPath);
                }
                catch (System.Exception e)
                {
                    EditorUtility.DisplayDialog("错误", $"创建日志目录失败: {e.Message}", "确定");
                }
            }
        }
    }

    // 重置为默认值
    private void ResetToDefaults()
    {
        if (EditorUtility.DisplayDialog("重置确认", "将重置所有构建设置为默认值，是否继续？", "确认", "取消"))
        {
            Defaults();
            EditorUtility.DisplayDialog("完成", "构建设置已重置为默认值", "确定");
        }
    }

    private static void Defaults()
    {
        // 重置新增的路径配置
        AotDllDir = Path.Combine(Application.dataPath, "JIT", "PakageAsset", "AOTDLL");
        JitDllDir = Path.Combine(Application.dataPath, "JIT", "PakageAsset", "JITDLL");

        AotDllsString = "System.Core.dll,System.dll,mscorlib.dll";
        JitDllsString = "HotUpdate.dll";
        EnableLog = true; // 重置为默认不启用日志

        BuildLogsDir = Path.Combine(ProjectRoot, "SaveAsset", "Out", "BuildEditor");
        ApkOutputDir = Path.Combine(ProjectRoot, "SaveAsset", "Out", "BuildPlayer");
    }
    
    public static System.Collections.Generic.List<string> GetAotDLLNames()
    {
        return AotDllsString.Split(',').Where(s => !string.IsNullOrEmpty(s.Trim())).Select(s => s.Trim()).ToList();
    }

    public static System.Collections.Generic.List<string> GetJITDLLNames()
    {
        return JitDllsString.Split(',').Where(s => !string.IsNullOrEmpty(s.Trim())).Select(s => s.Trim()).ToList();
    }

    #endregion
}