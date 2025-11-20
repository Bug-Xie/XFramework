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
        public static void BuildFullAB()
        {
            string currentVersion = GetVersion("apk");
            string newVersion = GetNextVersion(currentVersion, true);

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
            Debug.Log($"准备构建");
            ExecuteBuildAB(buildParams, "全量资源包");
            SetVersion("apk", newVersion);
        }

        /// <summary>
        /// 构建热更新资源包
        /// </summary>
        public static void BuildIncrementalAB()
        {
            // 读取并递增hotfix版本号
            string currentVersion = GetVersion("apk");
            string newVersion = GetNextVersion(currentVersion, false);

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
            SetVersion("apk", newVersion);
        }


        /// <summary>
        /// 执行资源包构建流程
        /// </summary>
        private static void ExecuteBuildAB(ScriptableBuildParameters buildParams, string buildName)
        {
            Debug.Log($"开始执行 {buildName} 构建...");
            Debug.Log($"构建参数: 目标平台={buildParams.BuildTarget}, 版本={buildParams.PackageVersion}");

            var pipeline = new ScriptableBuildPipeline();
            var result = pipeline.Run(buildParams, false);

            Debug.Log($"YooAsset构建完成: {(result.Success ? "成功 ✅" : "失败 ❌")}");

            if (result.Success)
            {
                Debug.Log($"✅ {buildName}构建成功 | 版本: {buildParams.PackageVersion}");

                // 资源验证和日志
                AssetDatabase.Refresh();
                string targetDir = buildParams.BuildinFileRoot;

                if (Directory.Exists(targetDir))
                {
                    var files = Directory.GetFiles(targetDir, "*", SearchOption.AllDirectories);
                    Debug.Log($"构建成功! 资源文件数量: {files.Length}");

                    if (files.Length > 0)
                    {
                        Debug.Log("资源文件示例:");
                        foreach (var file in files.Take(3))
                        {
                            Debug.Log($"- {file.Replace(Application.dataPath, "Assets")}");
                        }
                    }
                }
            }
            else
            {
                Debug.LogError($"❌ {buildName}构建失败，请检查错误日志");
                throw new Exception($"{buildName} 构建失败，已中断后续流程！");
            }
        }
        
        
        
        
    }
    
    
    
