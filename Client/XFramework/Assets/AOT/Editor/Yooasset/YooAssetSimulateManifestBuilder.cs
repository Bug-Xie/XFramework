using UnityEngine;
using UnityEditor;
using System.IO;
using YooAsset.Editor;

public class YooAssetSimulateManifestBuilder : EditorWindow
{
    private static string _targetPath = "Bundles/Android/DefaultPackage/Simulate";
    
    [MenuItem("Tools/生成模拟资源清单")]
    public static void GenerateSimulateManifest()
    {
        // // 获取项目根目录
        // string projectRoot = Path.GetDirectoryName(Application.dataPath);
        // string fullTargetPath = Path.Combine(projectRoot, _targetPath);
        //
        // try
        // {
        //     // 清理目标目录
        //     if (Directory.Exists(fullTargetPath))
        //     {
        //         Directory.Delete(fullTargetPath, true);
        //         Debug.Log($"已清理目录: {fullTargetPath}");
        //     }
        //     
        //     // 确保目录存在
        //     Directory.CreateDirectory(fullTargetPath);
        //     
        //     // 生成模拟资源清单
        //     string packageName = "DefaultPackage";
        //     var simulateResult = EditorSimulateModeHelper.SimulateBuild(packageName);
        //     
        //     Debug.Log($"模拟资源清单生成完成！");
        //     Debug.Log($"输出路径: {fullTargetPath}");
        //     
        //     // 打开资源管理器显示生成的目录
        //     EditorUtility.RevealInFinder(fullTargetPath);
        // }
        // catch (System.Exception e)
        // {
        //     Debug.LogError($"生成模拟资源清单失败: {e.Message}");
        //     EditorUtility.DisplayDialog("错误", $"生成失败: {e.Message}", "确定");
        // }
    }
}