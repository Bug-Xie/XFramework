using System;
using System.Linq;
using UnityEditor;
using BuildReport = UnityEditor.Build.Reporting.BuildReport;
using UnityEngine;
using System.IO;


    public partial class BuildPipelineEditor
    {
        /// <summary>
        /// 构建APK包
        /// </summary>
        /// <param name="apkName">APK名称前缀</param>
        public static void BuildPlayer(String apkName)
        {
            GameToolLogger.WriteLog("开始APK构建...");
            GameToolLogger.WriteLog($"构建类型: 全量包");

            // 从 PlayerSettings 读取版本号
            string currentVersion = PlayerSettings.bundleVersion;

            // 确保资源刷新
            AssetDatabase.Refresh();

            // 生成APK名称
            string buildType = apkName;
            // 使用可配置的输出目录
            string outputDir = BuildToolPanel.GetApkOutputDir();
            if (!Directory.Exists(outputDir))
            {
                Directory.CreateDirectory(outputDir);
                GameToolLogger.WriteLog($"创建APK输出目录: {outputDir}");
            }
            string outputPath = Path.Combine(outputDir, $"{buildType}_{currentVersion}.apk");
            GameToolLogger.WriteLog($"APK输出路径: {outputPath}");

            // 配置构建选项
            var options = new BuildPlayerOptions
            {
                scenes = EditorBuildSettings.scenes.Select(s => s.path).ToArray(),
                locationPathName = outputPath,
                target = BuildTarget.Android,
                options = BuildOptions.None
            };

            // 执行构建
            BuildReport report = UnityEditor.BuildPipeline.BuildPlayer(options);
            if (report.summary.result == UnityEditor.Build.Reporting.BuildResult.Succeeded)
            {
                GameToolLogger.WriteLog($"✅ APK构建成功: {outputPath}");

                // 获取实际文件大小
                FileInfo fileInfo = new FileInfo(outputPath);
                long fileSizeBytes = fileInfo.Length;
                double fileSizeMB = fileSizeBytes / (1024.0 * 1024.0);
                double fileSizeKB = fileSizeBytes / 1024.0;
                GameToolLogger.WriteLog($"实际文件大小: {fileSizeMB:F2} MB ({fileSizeKB:F0} KB)");
            }
            else
            {
                throw new Exception($"APK构建失败: {report.summary.result}");
            }
        }
    }
