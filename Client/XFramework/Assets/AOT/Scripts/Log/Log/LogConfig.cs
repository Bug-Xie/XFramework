using System;

/// <summary>
/// 日志配置管理
/// 集中管理所有日志设置
/// </summary>
public class LogConfig
{
    /// <summary>
    /// 当前日志级别
    /// </summary>
    public static LogLevel CurrentLevel = LogLevel.Info;

    /// <summary>
    /// 是否使用Unity Debug输出
    /// </summary>
    public static bool UseUnityDebug = true;

    /// <summary>
    /// 是否在控制台输出
    /// </summary>
    public static bool UseConsoleOutput = true;

    /// <summary>
    /// 是否保存到文件
    /// </summary>
    public static bool SaveToFile = false;

    /// <summary>
    /// 是否显示时间戳
    /// </summary>
    public static bool ShowTimestamp = true;

    /// <summary>
    /// 是否显示堆栈信息
    /// </summary>
    public static bool ShowStackTrace = true;

    /// <summary>
    /// 堆栈深度（0 = 完整）
    /// </summary>
    public static int StackTraceDepth = 5;

    /// <summary>
    /// 是否显示源代码链接
    /// </summary>
    public static bool ShowSourceLink = true;

    /// <summary>
    /// 日志文件保存目录
    /// </summary>
    public static string LogFileDirectory = "";

    /// <summary>
    /// 单个日志文件最大大小（字节）
    /// 0表示无限制
    /// </summary>
    public static long MaxLogFileSize = 10 * 1024 * 1024; // 10MB

    /// <summary>
    /// 最多保留的日志文件数
    /// </summary>
    public static int MaxLogFileCount = 10;

    /// <summary>
    /// 时间戳格式
    /// </summary>
    public static string TimestampFormat = "yyyy-MM-dd HH:mm:ss.fff";

    /// <summary>
    /// 验证配置的有效性
    /// </summary>
    public static void Validate()
    {
        if (StackTraceDepth < 0)
            StackTraceDepth = 0;

        if (MaxLogFileCount < 1)
            MaxLogFileCount = 1;
    }
}
