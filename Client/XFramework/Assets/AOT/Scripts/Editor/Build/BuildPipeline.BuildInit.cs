using UnityEngine;
using System.IO;

namespace AOT.Scripts.Editor.Build
{
    public partial class BuildPipelineEditor
    {
        /// <summary>
        /// 构建前的准备工作：
        /// 1. 清理输出目录
        /// 2. 生成HybridCLR所需的DLL
        /// 3. 拷贝AOT和热更DLL到指定位置
        /// </summary>
        private static void BuildInit(bool buildAPK)
        {
            Debug.Log("开始构建环境准备...");
        
            // 1. 清理构建缓存目录
            string buildOutputPath = BuildHelper.GetDLLOutputPath();
            if (buildAPK && Directory.Exists(buildOutputPath))
            {
                Directory.Delete(buildOutputPath, true);
                Debug.Log($"已清理构建缓存: {buildOutputPath}");
            }

            // 2. 清理热更DLL目录（Assets/HotCodeDll）
            string hotUpdateDllDir = BuildHelper.GetJITDllDir();
            if (Directory.Exists(hotUpdateDllDir))
            {
                Directory.Delete(hotUpdateDllDir, true);
                Debug.Log($"已清理热更DLL目录: {hotUpdateDllDir}");
            }

            // 3. 清理内置资源目录（StreamingAssets/DefaultPackage）
            string streamingAssetsDir = BuildHelper.GetAOTDLLDir();
            if (Directory.Exists(streamingAssetsDir))
            {
                Directory.Delete(streamingAssetsDir, true);
                Debug.Log($"已清理StreamingAssets资源目录: {streamingAssetsDir}");
            }
        }
    }
}