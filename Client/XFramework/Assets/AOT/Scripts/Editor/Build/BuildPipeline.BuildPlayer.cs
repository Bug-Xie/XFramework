using System;
using System.Linq;
using UnityEditor;
using BuildReport = UnityEditor.Build.Reporting.BuildReport;
using UnityEngine;
using System.IO;

namespace AOT.Scripts.Editor.Build
{
    public partial class BuildPipelineEditor
    {
        /// <summary>
        /// 构建APK包
        /// </summary>
        /// <param name="includeAllResources">true=全量包，false=核心包</param>
        private static void BuildPlayer(bool includeAllResources,String apkName)
        {
            Debug.Log("开始APK构建...");
            Debug.Log($"构建类型: {(includeAllResources ? "全量包" : "核心包")}");
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
                Debug.Log($"文件大小: {report.summary.totalSize / (1024 * 1024)} MB");
            }
            else
            {
                throw new Exception($"APK构建失败: {report.summary.result}");
            }
        }
    }
}