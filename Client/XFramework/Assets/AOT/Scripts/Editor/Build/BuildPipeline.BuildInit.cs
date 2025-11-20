using UnityEngine;
using System.IO;
using YooAsset.Editor;


public partial class BuildPipelineEditor
    {
        /// <summary>
        /// 构建前的准备工作：
        /// 1. 清理输出目录
        /// 2. 生成HybridCLR所需的DLL
        /// 3. 拷贝AOT和热更DLL到指定位置
        /// </summary>
        public static void BuildInit(bool buildAPK)
        {
            Debug.Log("开始构建环境准备...");

            // 清理AOT DLL目录
            string aotDllDir = BuildToolPanel.GetAOTDLLDir();
            if (Directory.Exists(aotDllDir))
            {
                Directory.Delete(aotDllDir, true);
                Debug.Log($"已清理AOT DLL目录: {aotDllDir}");
            }

            // 清理JIT DLL目录
            string jitDllDir = BuildToolPanel.GetJITDllDir();
            if (Directory.Exists(jitDllDir))
            {
                Directory.Delete(jitDllDir, true);
                Debug.Log($"已清理JIT DLL目录: {jitDllDir}");
            }

            // 清理内置资源目录（StreamingAssets/DefaultPackage）
            string streamingAssetsDir = AssetBundleBuilderHelper.GetStreamingAssetsRoot();
            if (Directory.Exists(streamingAssetsDir))
            {
                Directory.Delete(streamingAssetsDir, true);
                Debug.Log($"已清理StreamingAssets资源目录: {streamingAssetsDir}");
            }
        }
    }
