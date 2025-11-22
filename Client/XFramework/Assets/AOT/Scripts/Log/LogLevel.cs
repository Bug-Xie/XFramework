using System;

/// <summary>
/// 日志级别定义
/// </summary>
public enum LogLevel : byte
{
    /// <summary>
    /// 信息级别 - 一般信息
    /// </summary>
    Info = 0,

    /// <summary>
    /// 警告级别 - 需要注意的问题
    /// </summary>
    Warn = 1,

    /// <summary>
    /// 错误级别 - 严重错误
    /// </summary>
    Error = 2
}
