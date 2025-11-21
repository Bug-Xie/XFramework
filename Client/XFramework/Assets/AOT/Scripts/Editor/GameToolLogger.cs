using System;
using System.IO;
using System.Text;
using UnityEngine;

/// <summary>
/// 构建日志记录工具
/// </summary>
public static class GameToolLogger
{
    private static string _logFilePath;
    private static StreamWriter _writer;
    private static bool _isInitialized = false;

    /// <summary>
    /// 初始化日志系统
    /// </summary>
    /// <param name="logFileName">日志文件名（不需要路径）</param>
    public static void Initialize(string logFileName = "build_log.txt")
    {
        try
        {
            // 确定日志文件路径 - 从BuildToolPanel获取可配置路径
            string logDir = BuildToolPanel.GetBuildLogsDir();
            if (!Directory.Exists(logDir))
            {
                Directory.CreateDirectory(logDir);
            }
            
            _logFilePath = Path.Combine(logDir, logFileName);
            
            // 创建或追加到日志文件
            _writer = new StreamWriter(_logFilePath, false, Encoding.UTF8);
            _isInitialized = true;
            
            WriteLog("=== 构建日志系统初始化 ===");
            WriteLog($"日志文件: {_logFilePath}");
            WriteLog($"初始化时间: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
            WriteLog("=========================");
        }
        catch (Exception e)
        {
            Debug.LogError($"日志系统初始化失败: {e.Message}");
        }
    }

    /// <summary>
    /// 写入日志
    /// </summary>
    public static void WriteLog(string message, LogType logType = LogType.Info)
    {
        if (!_isInitialized)
        {
            Initialize(); // 自动初始化
        }

        try
        {
            string timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
            string logEntry = $"[{timestamp}] [{logType}] {message}";
            
            // 写入文件
            _writer.WriteLine(logEntry);
            _writer.Flush(); // 立即写入，避免缓冲
            
            // 同时在Unity控制台输出
            switch (logType)
            {
                case LogType.Error:
                    Debug.LogError(logEntry);
                    break;
                case LogType.Warning:
                    Debug.LogWarning(logEntry);
                    break;
                default:
                    Debug.Log(logEntry);
                    break;
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"写入日志失败: {e.Message}");
        }
    }

  
    /// <summary>
    /// 关闭日志系统
    /// </summary>
    public static void Shutdown()
    {
        if (_writer != null)
        {
            WriteLog("=== 构建日志系统关闭 ===");
            _writer.Close();
            _writer.Dispose();
            _writer = null;
        }
        _isInitialized = false;
    }
    
}

/// <summary>
/// 日志类型
/// </summary>
public enum LogType
{
    Info,
    Warning,
    Error
}