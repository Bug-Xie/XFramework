using System;
using System.IO;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public partial class BuildPipelineEditor
{
    // 服务器资源根目录配置
    private const string SERVER_RES_ROOT = "C:/MyPart/Work/UnityHub/XFramework/Server/nginx-1.28.0/html/XFramework/Res";
    private const string PLATFORM = "Android";

    // 完整的服务器资源目录路径
    private static string ServerResDir => Path.Combine(SERVER_RES_ROOT, PLATFORM);
    // Bundles 源目录路径
    private static string BundlesDir => Path.Combine(BuildToolPanel.GetProjectRoot(), "Bundles", PLATFORM, "DefaultPackage");

    /// <summary>
    /// 全量同步：先清理再同步
    /// </summary>
    static void BuildFullSeverSync()
    {
        CleanSeverRes();
        SeverSyncRes();
    }

    /// <summary>
    /// 增量同步：只同步资源
    /// </summary>
    static void BuildSeverSync()
    {
        SeverSyncRes();
    }

    /// <summary>
    /// 清理服务器资源目录
    /// </summary>
    static void CleanSeverRes()
    {
        try
        {
            GameToolLogger.WriteLog($"========================================");
            GameToolLogger.WriteLog($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] 开始清理本地服务器资源目录");
            GameToolLogger.WriteLog($"平台: {PLATFORM}");
            GameToolLogger.WriteLog($"清理路径: {ServerResDir}");
            GameToolLogger.WriteLog($"========================================");

            // 检查目录是否存在
            if (Directory.Exists(ServerResDir))
            {
                GameToolLogger.WriteLog("正在清空目录...");

                // 删除目录下所有文件和子目录
                DirectoryInfo di = new DirectoryInfo(ServerResDir);
                foreach (FileInfo file in di.GetFiles("*", SearchOption.AllDirectories))
                {
                    file.Delete();
                }
                foreach (DirectoryInfo dir in di.GetDirectories())
                {
                    dir.Delete(true);
                }

                GameToolLogger.WriteLog("✅ 目录已完全清空");
            }
            else
            {
                GameToolLogger.WriteLog("⚠️ 目录不存在，正在创建...");
                Directory.CreateDirectory(ServerResDir);
                GameToolLogger.WriteLog("✅ 目录创建成功");
            }

            // 验证清理结果
            int fileCount = Directory.GetFiles(ServerResDir, "*", SearchOption.AllDirectories).Length;
            GameToolLogger.WriteLog($"========================================");
            GameToolLogger.WriteLog($"✅ 清理完成!");
            GameToolLogger.WriteLog($"剩余文件数量: {fileCount}");
            GameToolLogger.WriteLog($"========================================");
        }
        catch (Exception ex)
        {
            GameToolLogger.WriteLog($"========================================");
            GameToolLogger.WriteLog($"❌ 清理失败!");
            GameToolLogger.WriteLog($"错误信息: {ex.Message}");
            GameToolLogger.WriteLog($"堆栈跟踪: {ex.StackTrace}");
            GameToolLogger.WriteLog($"========================================");
            throw;
        }
    }

    /// <summary>
    /// 同步资源到服务器
    /// </summary>
    static void SeverSyncRes()
    {
        try
        {
            GameToolLogger.WriteLog($"========================================");
            GameToolLogger.WriteLog($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] 开始资源同步");
            GameToolLogger.WriteLog($"平台: {PLATFORM}");
            GameToolLogger.WriteLog($"源目录: {BundlesDir}");
            GameToolLogger.WriteLog($"目标目录: {ServerResDir}");
            GameToolLogger.WriteLog($"========================================");

            // 检查源目录是否存在
            if (!Directory.Exists(BundlesDir))
            {
                GameToolLogger.WriteLog($"❌ 错误: Bundles目录不存在: {BundlesDir}");
                GameToolLogger.WriteLog("请先执行资源包构建");
                throw new DirectoryNotFoundException($"Bundles目录不存在: {BundlesDir}");
            }

            // 检查目标目录是否存在，不存在则创建
            if (!Directory.Exists(ServerResDir))
            {
                GameToolLogger.WriteLog("⚠️ 服务器资源目录不存在，正在创建...");
                Directory.CreateDirectory(ServerResDir);
                GameToolLogger.WriteLog("✅ 目录创建成功");
            }

            GameToolLogger.WriteLog("当前工作目录: " + BundlesDir);

            // 查找版本文件夹（格式: X.Y.Z）
            GameToolLogger.WriteLog("");
            GameToolLogger.WriteLog("===== 扫描版本文件夹 =====");

            List<string> versionDirs = new List<string>();
            DirectoryInfo bundlesDirInfo = new DirectoryInfo(BundlesDir);
            foreach (DirectoryInfo dir in bundlesDirInfo.GetDirectories())
            {
                // 检查是否符合版本号格式 X.Y.Z
                if (System.Text.RegularExpressions.Regex.IsMatch(dir.Name, @"^\d+\.\d+\.\d+$"))
                {
                    versionDirs.Add(dir.Name);
                }
            }

            if (versionDirs.Count == 0)
            {
                GameToolLogger.WriteLog("❌ 错误: 未找到版本文件夹");
                GameToolLogger.WriteLog("Bundles目录应包含类似 1.0.0, 2.0.0 等版本文件夹");
                throw new Exception("未找到版本文件夹");
            }

            // 按版本号排序
            versionDirs.Sort((a, b) => new Version(a).CompareTo(new Version(b)));

            GameToolLogger.WriteLog("检测到的版本文件夹:");
            foreach (string version in versionDirs)
            {
                GameToolLogger.WriteLog($"  {version}");
            }
            GameToolLogger.WriteLog("==========================");

            // 获取最新版本和上一版本
            string currentVersion = versionDirs[versionDirs.Count - 1];
            string lastVersion = versionDirs.Count > 1 ? versionDirs[versionDirs.Count - 2] : null;

            GameToolLogger.WriteLog("");
            GameToolLogger.WriteLog($"当前版本: {currentVersion}");
            GameToolLogger.WriteLog($"上一版本: {(string.IsNullOrEmpty(lastVersion) ? "无" : lastVersion)}");
            GameToolLogger.WriteLog("");

            // 判断是全量更新还是增量更新
            if (string.IsNullOrEmpty(lastVersion))
            {
                // 全量更新
                FullSync(currentVersion);
            }
            else
            {
                // 增量更新
                IncrementalSync(currentVersion, lastVersion);
            }

            // 显示服务器目录文件列表
            int fileCount = Directory.GetFiles(ServerResDir, "*", SearchOption.AllDirectories).Length;
            long totalSize = GetDirectorySize(new DirectoryInfo(ServerResDir));

            GameToolLogger.WriteLog("");
            GameToolLogger.WriteLog("=========================================");
            GameToolLogger.WriteLog("服务器资源目录内容:");
            GameToolLogger.WriteLog($"总文件数: {fileCount}");
            GameToolLogger.WriteLog($"总大小: {FormatFileSize(totalSize)}");
            GameToolLogger.WriteLog("=========================================");
            GameToolLogger.WriteLog("✅ 资源同步完成!");
            GameToolLogger.WriteLog("=========================================");
        }
        catch (Exception ex)
        {
            GameToolLogger.WriteLog($"========================================");
            GameToolLogger.WriteLog($"❌ 资源同步失败!");
            GameToolLogger.WriteLog($"错误信息: {ex.Message}");
            GameToolLogger.WriteLog($"堆栈跟踪: {ex.StackTrace}");
            GameToolLogger.WriteLog($"========================================");
            throw;
        }
    }

    /// <summary>
    /// 全量同步
    /// </summary>
    private static void FullSync(string currentVersion)
    {
        GameToolLogger.WriteLog("===== 执行全量更新 =====");
        GameToolLogger.WriteLog("首次打包或仅有一个版本，将进行全量同步");

        string sourceDir = Path.Combine(BundlesDir, currentVersion);

        GameToolLogger.WriteLog("正在复制资源文件...");

        // 复制整个版本目录到服务器
        CopyDirectory(sourceDir, ServerResDir, true);

        GameToolLogger.WriteLog("✅ 全量更新完成");
    }

    /// <summary>
    /// 增量同步
    /// </summary>
    private static void IncrementalSync(string currentVersion, string lastVersion)
    {
        GameToolLogger.WriteLog("===== 执行增量更新 =====");
        GameToolLogger.WriteLog($"对比版本 {lastVersion} → {currentVersion}");

        string currentDir = Path.Combine(BundlesDir, currentVersion);
        string lastDir = Path.Combine(BundlesDir, lastVersion);

        GameToolLogger.WriteLog("正在分析差异文件...");

        // 收集差异文件
        List<string> diffFiles = new List<string>();

        // 获取当前版本所有文件
        DirectoryInfo currentDirInfo = new DirectoryInfo(currentDir);
        foreach (FileInfo file in currentDirInfo.GetFiles("*", SearchOption.AllDirectories))
        {
            string relativePath = file.FullName.Substring(currentDir.Length + 1);
            string lastFilePath = Path.Combine(lastDir, relativePath);

            // 检查是否为新增文件或修改文件
            if (!File.Exists(lastFilePath))
            {
                // 新增文件
                diffFiles.Add(relativePath);
            }
            else
            {
                // 比较文件是否修改（通过文件大小和修改时间）
                FileInfo lastFile = new FileInfo(lastFilePath);
                if (file.Length != lastFile.Length || file.LastWriteTime != lastFile.LastWriteTime)
                {
                    diffFiles.Add(relativePath);
                }
            }
        }

        GameToolLogger.WriteLog($"差异文件数量: {diffFiles.Count}");

        if (diffFiles.Count == 0)
        {
            GameToolLogger.WriteLog("⚠️ 没有检测到差异文件，跳过更新");
            return;
        }

        GameToolLogger.WriteLog("正在复制增量文件...");

        // 复制差异文件到服务器
        int copiedCount = 0;
        foreach (string relativePath in diffFiles)
        {
            string sourceFile = Path.Combine(currentDir, relativePath);
            string targetFile = Path.Combine(ServerResDir, relativePath);

            // 确保目标目录存在
            string targetDir = Path.GetDirectoryName(targetFile);
            if (!Directory.Exists(targetDir))
            {
                Directory.CreateDirectory(targetDir);
            }

            // 复制文件
            File.Copy(sourceFile, targetFile, true);
            copiedCount++;

            // 每复制100个文件显示一次进度
            if (copiedCount % 100 == 0)
            {
                GameToolLogger.WriteLog($"已复制: {copiedCount}/{diffFiles.Count}");
            }
        }

        GameToolLogger.WriteLog($"✅ 增量更新完成，共复制 {copiedCount} 个文件");
    }

    /// <summary>
    /// 递归复制目录
    /// </summary>
    private static void CopyDirectory(string sourceDir, string targetDir, bool overwrite)
    {
        DirectoryInfo dir = new DirectoryInfo(sourceDir);

        if (!dir.Exists)
        {
            throw new DirectoryNotFoundException($"源目录不存在: {sourceDir}");
        }

        // 创建目标目录
        if (!Directory.Exists(targetDir))
        {
            Directory.CreateDirectory(targetDir);
        }

        // 复制所有文件
        foreach (FileInfo file in dir.GetFiles())
        {
            string targetFilePath = Path.Combine(targetDir, file.Name);
            file.CopyTo(targetFilePath, overwrite);
        }

        // 递归复制子目录
        foreach (DirectoryInfo subDir in dir.GetDirectories())
        {
            string newTargetDir = Path.Combine(targetDir, subDir.Name);
            CopyDirectory(subDir.FullName, newTargetDir, overwrite);
        }
    }

    /// <summary>
    /// 获取目录大小
    /// </summary>
    private static long GetDirectorySize(DirectoryInfo dir)
    {
        long size = 0;

        // 累加所有文件大小
        foreach (FileInfo file in dir.GetFiles("*", SearchOption.AllDirectories))
        {
            size += file.Length;
        }

        return size;
    }

    /// <summary>
    /// 格式化文件大小
    /// </summary>
    private static string FormatFileSize(long bytes)
    {
        string[] sizes = { "B", "KB", "MB", "GB", "TB" };
        double len = bytes;
        int order = 0;

        while (len >= 1024 && order < sizes.Length - 1)
        {
            order++;
            len = len / 1024;
        }

        return $"{len:0.##} {sizes[order]}";
    }
}
