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
        /// <param name="includeAllResources">true=全量包，false=核心包</param>
        public static void BuildPlayer(String apkName)
        {
            Debug.Log("开始APK构建...");
            Debug.Log($"构建类型: 全量包");
            // 读取apk版本号
            string currentVersion = GetVersion("apk");
            // 确保资源刷新
            AssetDatabase.Refresh();

            // 生成APK名称
            string buildType = apkName;
            string outputPath = $"Build/Android/{buildType}_{currentVersion}.apk";

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
                Debug.Log($"✅ APK构建成功: {outputPath}");
    
                // 获取实际文件大小
                FileInfo fileInfo = new FileInfo(outputPath);
                long fileSizeBytes = fileInfo.Length;
                double fileSizeMB = fileSizeBytes / (1024.0 * 1024.0);
                double fileSizeKB = fileSizeBytes / 1024.0;
                Debug.Log($"实际文件大小: {fileSizeMB:F2} MB ({fileSizeKB:F0} KB)");
            }
            else
            {
                throw new Exception($"APK构建失败: {report.summary.result}");
            }
        }
    }
