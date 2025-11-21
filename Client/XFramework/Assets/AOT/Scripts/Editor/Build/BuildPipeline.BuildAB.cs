using System;
using System.Linq;
using UnityEditor;
using YooAsset.Editor;
using UnityEngine;
using System.IO;
using YooAsset;


    public partial class BuildPipelineEditor
    {
        /// <summary>
        /// 构建全量资源包
        /// </summary>
        /// <param name="autoIncrementVersion">是否自动递增版本号（开发模式传false，正式发布传true）</param>
        public static void BuildFullAB(bool autoIncrementVersion = true)
        {
            // 从 PlayerSettings 读取当前版本
            string currentVersion = PlayerSettings.bundleVersion;
            string newVersion = currentVersion;

            if (autoIncrementVersion)
            {
                newVersion = GetNextVersion(currentVersion, true);
                GameToolLogger.WriteLog($"版本号递增: {currentVersion} → {newVersion}");
            }
            else
            {
                GameToolLogger.WriteLog($"开发模式：保持版本号 {currentVersion} 不变");
            }

            var buildParams = new ScriptableBuildParameters
            {
                BuildOutputRoot = AssetBundleBuilderHelper.GetDefaultBuildOutputRoot(), //Yooasset 默认输出路径
                BuildinFileRoot =AssetBundleBuilderHelper.GetStreamingAssetsRoot(), //Yooasset 默认复制Streamming路径
                BuildPipeline = nameof(ScriptableBuildPipeline), //默认构建管线
                BuildBundleType = (int)EBuildBundleType.AssetBundle, //构建格式
                BuildTarget = EditorUserBuildSettings.activeBuildTarget, //构建平台
                PackageName = "DefaultPackage", //包名
                PackageVersion = newVersion, // 次版本号升级
                EnableSharePackRule = false, //是否共享
                VerifyBuildingResult = true, //验证结果
                FileNameStyle = EFileNameStyle.HashName, //资源包样式
                BuildinFileCopyOption = EBuildinFileCopyOption.ClearAndCopyAll, // 清理streamming并全部复制
                BuildinFileCopyParams = null, ////需要复制到stremming的标签
                CompressOption = ECompressOption.LZ4, //压缩格式
                ClearBuildCacheFiles = true, //打包输出路径是否清除以前的
                EncryptionServices = new EncryptionNone(), //资源包加密服务
                ManifestProcessServices = new ManifestProcessNone(), //资源清单加密服务
                ManifestRestoreServices = new ManifestRestoreNone(), //资源清单解密服务
                BuiltinShadersBundleName = string.Empty, //是否内置着色器资源包名称
                DisableWriteTypeTree = false, // 全量包不禁用TypeTree写入
            };
            GameToolLogger.WriteLog("准备构建");
            ExecuteBuildAB(buildParams, "全量资源包");

            // 只有在自动递增模式下才更新版本号
            if (autoIncrementVersion)
            {
                PlayerSettings.bundleVersion = newVersion;
                PlayerSettings.Android.bundleVersionCode++;
                GameToolLogger.WriteLog($"已更新版本号: {newVersion}, Android构建号: {PlayerSettings.Android.bundleVersionCode}");
            }
        }

        /// <summary>
        /// 构建热更新资源包
        /// </summary>
        /// <param name="autoIncrementVersion">是否自动递增版本号（开发模式传false，正式发布传true）</param>
        public static void BuildIncrementalAB(bool autoIncrementVersion = true)
        {
            // 从 PlayerSettings 读取当前版本
            string currentVersion = PlayerSettings.bundleVersion;
            string newVersion = currentVersion;

            if (autoIncrementVersion)
            {
                newVersion = GetNextVersion(currentVersion, false);
                GameToolLogger.WriteLog($"版本号递增: {currentVersion} → {newVersion}");
            }
            else
            {
                GameToolLogger.WriteLog($"开发模式：保持版本号 {currentVersion} 不变");
            }

            var buildParams = new ScriptableBuildParameters
            {
                BuildOutputRoot = AssetBundleBuilderHelper.GetDefaultBuildOutputRoot(), //Yooasset 默认输出路径
                BuildinFileRoot =AssetBundleBuilderHelper.GetStreamingAssetsRoot(), //Yooasset 默认复制Streamming路径
                BuildPipeline = nameof(ScriptableBuildPipeline), //默认构建管线
                BuildBundleType = (int)EBuildBundleType.AssetBundle, //构建格式
                BuildTarget = EditorUserBuildSettings.activeBuildTarget, //构建平台
                PackageName = "DefaultPackage", //包名
                PackageVersion = newVersion, // 次版本号升级
                EnableSharePackRule = true, //是否共享
                VerifyBuildingResult = true, //验证结果
                FileNameStyle = EFileNameStyle.HashName, //资源包样式
                BuildinFileCopyOption = EBuildinFileCopyOption.None, // 清理streamming并全部复制
                BuildinFileCopyParams =  null, ////需要复制到stremming的标签
                CompressOption = ECompressOption.LZ4, //压缩格式
                ClearBuildCacheFiles = true, //打包输出路径是否清除以前的
                EncryptionServices = new EncryptionNone(), //资源包加密服务
                ManifestProcessServices = new ManifestProcessNone(), //资源清单加密服务
                ManifestRestoreServices = new ManifestRestoreNone(), //资源清单解密服务
                BuiltinShadersBundleName = string.Empty, //是否内置着色器资源包名称
                DisableWriteTypeTree = false, // 全量包不禁用TypeTree写入
            };
            ExecuteBuildAB(buildParams, "热更新资源包");

            // 只有在自动递增模式下才更新版本号
            if (autoIncrementVersion)
            {
                PlayerSettings.bundleVersion = newVersion;
                PlayerSettings.Android.bundleVersionCode++;
                GameToolLogger.WriteLog($"已更新版本号: {newVersion}, Android构建号: {PlayerSettings.Android.bundleVersionCode}");
            }
        }


        /// <summary>
        /// 执行资源包构建流程
        /// </summary>
        private static void ExecuteBuildAB(ScriptableBuildParameters buildParams, string buildName)
        {
            GameToolLogger.WriteLog($"开始执行 {buildName} 构建...");
            GameToolLogger.WriteLog($"构建参数: 目标平台={buildParams.BuildTarget}, 版本={buildParams.PackageVersion}");

            var pipeline = new ScriptableBuildPipeline();
            var result = pipeline.Run(buildParams, false);

            GameToolLogger.WriteLog($"YooAsset构建完成: {(result.Success ? "成功 ✅" : "失败 ❌")}");

            if (result.Success)
            {
                GameToolLogger.WriteLog($"✅ {buildName}构建成功 | 版本: {buildParams.PackageVersion}");

                // 资源验证和日志
                AssetDatabase.Refresh();
                string targetDir = buildParams.BuildinFileRoot;

                if (Directory.Exists(targetDir))
                {
                    var files = Directory.GetFiles(targetDir, "*", SearchOption.AllDirectories);
                    GameToolLogger.WriteLog($"构建成功! 资源文件数量: {files.Length}");

                    if (files.Length > 0)
                    {
                        GameToolLogger.WriteLog("资源文件示例:");
                        foreach (var file in files.Take(3))
                        {
                            GameToolLogger.WriteLog($"- {file.Replace(Application.dataPath, "Assets")}");
                        }
                    }
                }
            }
            else
            {
                GameToolLogger.WriteLog($"❌ {buildName}构建失败，请检查错误日志", LogType.Error);
                throw new Exception($"{buildName} 构建失败，已中断后续流程！");
            }
        }
        
        
        
        
    }
    
    
    
