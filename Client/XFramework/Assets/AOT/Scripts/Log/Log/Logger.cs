using System;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

/// <summary>
/// 统一的日志管理器
/// 提供简洁的API和线程安全的日志记录
/// 替代原有的Debuger类
/// </summary>
public static class Logger
{
    private static readonly object _lockObject = new object();
    private static readonly List<ILogHandler> _handlers = new List<ILogHandler>();
    private static bool _initialized = false;

    /// <summary>
    /// 初始化日志系统
    /// </summary>
    public static void Initialize()
    {
        lock (_lockObject)
        {
            if (_initialized)
                return;

            LogConfig.Validate();

            // 添加默认处理器
            if (LogConfig.UseUnityDebug)
                _handlers.Add(new UnityLogHandler());

            if (LogConfig.UseConsoleOutput)
                _handlers.Add(new ConsoleLogHandler());

            if (LogConfig.SaveToFile)
                _handlers.Add(new FileLogHandler());

            _initialized = true;
        }
    }

    /// <summary>
    /// 添加自定义日志处理器
    /// </summary>
    public static void AddHandler(ILogHandler handler)
    {
        if (handler == null)
            return;

        lock (_lockObject)
        {
            if (!_handlers.Contains(handler))
                _handlers.Add(handler);
        }
    }

    /// <summary>
    /// 移除日志处理器
    /// </summary>
    public static void RemoveHandler(ILogHandler handler)
    {
        if (handler == null)
            return;

        lock (_lockObject)
        {
            _handlers.Remove(handler);
            (handler as IDisposable)?.Dispose();
        }
    }

    /// <summary>
    /// 清除所有处理器
    /// </summary>
    public static void ClearHandlers()
    {
        lock (_lockObject)
        {
            foreach (var handler in _handlers)
            {
                (handler as IDisposable)?.Dispose();
            }
            _handlers.Clear();
            _initialized = false;
        }
    }

    #region Info Logging

    [Conditional("EnableLog")]
    public static void Info(object message)
    {
        if (ShouldLog(LogLevel.Info))
            InternalLog(LogLevel.Info, "", message?.ToString() ?? "null");
    }

    [Conditional("EnableLog")]
    public static void Info(string tag, object message)
    {
        if (ShouldLog(LogLevel.Info))
            InternalLog(LogLevel.Info, tag, message?.ToString() ?? "null");
    }

    [Conditional("EnableLog")]
    public static void Info(string tag, string format, params object[] args)
    {
        if (ShouldLog(LogLevel.Info))
            InternalLog(LogLevel.Info, tag, FormatMessage(format, args));
    }

    [Conditional("EnableLog")]
    public static void InfoNetSend(string message)
    {
        if (ShouldLog(LogLevel.Info))
            InternalLog(LogLevel.Info, "[Net-Send]", message);
    }

    [Conditional("EnableLog")]
    public static void InfoNetReceive(string message)
    {
        if (ShouldLog(LogLevel.Info))
            InternalLog(LogLevel.Info, "[Net-Recv]", message);
    }

    #endregion

    #region Warn Logging

    [Conditional("EnableLog")]
    public static void Warn(object message)
    {
        if (ShouldLog(LogLevel.Warn))
            InternalLog(LogLevel.Warn, "", message?.ToString() ?? "null");
    }

    [Conditional("EnableLog")]
    public static void Warn(string tag, object message)
    {
        if (ShouldLog(LogLevel.Warn))
            InternalLog(LogLevel.Warn, tag, message?.ToString() ?? "null");
    }

    [Conditional("EnableLog")]
    public static void Warn(string tag, string format, params object[] args)
    {
        if (ShouldLog(LogLevel.Warn))
            InternalLog(LogLevel.Warn, tag, FormatMessage(format, args));
    }

    #endregion

    #region Error Logging

    [Conditional("EnableLog")]
    public static void Error(object message)
    {
        if (ShouldLog(LogLevel.Error))
            InternalLog(LogLevel.Error, "", message?.ToString() ?? "null");
    }

    [Conditional("EnableLog")]
    public static void Error(string tag, object message)
    {
        if (ShouldLog(LogLevel.Error))
            InternalLog(LogLevel.Error, tag, message?.ToString() ?? "null");
    }

    [Conditional("EnableLog")]
    public static void Error(string tag, string format, params object[] args)
    {
        if (ShouldLog(LogLevel.Error))
            InternalLog(LogLevel.Error, tag, FormatMessage(format, args));
    }

    [Conditional("EnableLog")]
    public static void Error(object message, Exception exception)
    {
        if (ShouldLog(LogLevel.Error))
        {
            InternalLog(LogLevel.Error, "Exception", message?.ToString() ?? exception?.Message ?? "Unknown error");
            if (exception != null)
            {
                string exceptionInfo = $"{exception.GetType().Name}: {exception.Message}\n{exception.StackTrace}";
                InternalLogStackTrace(exceptionInfo);
            }
        }
    }

    #endregion

    /// <summary>
    /// 刷新所有日志处理器的缓冲区
    /// </summary>
    public static void Flush()
    {
        lock (_lockObject)
        {
            foreach (var handler in _handlers)
            {
                handler.Flush();
            }
        }
    }

    /// <summary>
    /// 释放日志资源
    /// </summary>
    public static void Shutdown()
    {
        Flush();
        ClearHandlers();
    }

    #region Private Methods

    private static bool ShouldLog(LogLevel level)
    {
        return LogConfig.CurrentLevel <= level;
    }

    private static void InternalLog(LogLevel level, string tag, string message)
    {
        if (!_initialized)
            Initialize();

        lock (_lockObject)
        {
            foreach (var handler in _handlers)
            {
                handler.Handle(level, tag, message);
            }

            // 处理堆栈信息
            if (LogConfig.ShowStackTrace && level == LogLevel.Error)
            {
                string stackTrace = StackTraceProvider.GetEnhancedStackTrace(LogConfig.StackTraceDepth);
                foreach (var handler in _handlers)
                {
                    handler.HandleStackTrace(stackTrace);
                }
            }
        }
    }

    private static void InternalLogStackTrace(string stackTrace)
    {
        lock (_lockObject)
        {
            foreach (var handler in _handlers)
            {
                handler.HandleStackTrace(stackTrace);
            }
        }
    }

    private static string FormatMessage(string format, object[] args)
    {
        if (args == null || args.Length == 0)
            return format;

        try
        {
            return string.Format(format, args);
        }
        catch (Exception ex)
        {
            return $"{format} [Format Error: {ex.Message}]";
        }
    }

    #endregion
}
