using System.IO;
using UnityEditor;
using UnityEngine;

/// <summary>
/// AssetBundle构建完成后的自动处理脚本
/// 监听AssetBundle构建完成，自动执行后续步骤
/// </summary>
public class AssetBundleBuildPostProcessor : AssetPostprocessor
{
    private static string LastBuildPlatform = "";

    public override int GetPostprocessOrder()
    {
        return 0;
    }

    public static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths, bool didDomainReload)
    {
        // 检查是否是从Bundles文件夹导入的资源（表示AssetBundle构建完成）
        foreach (string asset in importedAssets)
        {
            if (asset.Contains("Bundles/"))
            {
                Debug.Log("AssetBundle build detected, processing...");
                break;
            }
        }
    }
}

/// <summary>
/// 管理整个构建流程的工具类
/// </summary>
public static class BuildPackageUtility
{
    /// <summary>
    /// 保存构建状态以便在菜单操作后继续
    /// </summary>
    private static string BuildStateFile
    {
        get { return Path.Combine(Path.GetDirectoryName(Application.dataPath), ".buildstate"); }
    }

    /// <summary>
    /// 保存构建状态
    /// </summary>
    public static void SaveBuildState(BuildPackageState state)
    {
        string json = JsonUtility.ToJson(state);
        File.WriteAllText(BuildStateFile, json);
        Debug.Log($"Build state saved: {state}");
    }

    /// <summary>
    /// 加载构建状态
    /// </summary>
    public static bool TryLoadBuildState(out BuildPackageState state)
    {
        if (File.Exists(BuildStateFile))
        {
            try
            {
                string json = File.ReadAllText(BuildStateFile);
                state = JsonUtility.FromJson<BuildPackageState>(json);
                Debug.Log($"Build state loaded: {state}");
                return true;
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"Failed to load build state: {ex.Message}");
            }
        }

        state = default;
        return false;
    }

    /// <summary>
    /// 清除构建状态
    /// </summary>
    public static void ClearBuildState()
    {
        if (File.Exists(BuildStateFile))
        {
            File.Delete(BuildStateFile);
            Debug.Log("Build state cleared");
        }
    }

    /// <summary>
    /// 等待AssetBundle构建完成
    /// </summary>
    public static bool WaitForAssetBundleBuild(int timeoutSeconds = 300)
    {
        int elapsedTime = 0;
        int checkInterval = 1000; // 1秒检查一次

        while (elapsedTime < timeoutSeconds * 1000)
        {
            // 检查是否还有构建进程在运行
            if (!EditorApplication.isCompiling && !EditorApplication.isUpdating)
            {
                System.Threading.Thread.Sleep(500); // 再等0.5秒确保完全完成
                return true;
            }

            System.Threading.Thread.Sleep(checkInterval);
            elapsedTime += checkInterval;

            if (elapsedTime % 10000 == 0) // 每10秒输出一次
            {
                Debug.Log($"Waiting for AssetBundle build... ({elapsedTime / 1000}s)");
            }
        }

        Debug.LogError("AssetBundle build timeout");
        return false;
    }
}

/// <summary>
/// 构建状态数据结构
/// </summary>
[System.Serializable]
public class BuildPackageState
{
    public string PlayModeValue;
    public string HostServerURL;
    public string ServerFolderPath;
    public string PlatformName;

    public override string ToString()
    {
        return $"PlayMode={PlayModeValue}, Platform={PlatformName}, ServerPath={ServerFolderPath}";
    }
}
