using System;
using System.Text;

/// <summary>
/// 控制台日志处理器
/// 负责将日志输出到标准控制台
/// </summary>
public class ConsoleLogHandler : ILogHandler
{
    private readonly StringBuilder _formatBuffer = new StringBuilder(512);

    public void Handle(LogLevel level, string tag, string message, UnityEngine.Object context = null)
    {
        _formatBuffer.Clear();

        if (LogConfig.ShowTimestamp)
        {
            _formatBuffer.Append("[");
            _formatBuffer.Append(DateTime.Now.ToString(LogConfig.TimestampFormat));
            _formatBuffer.Append("] ");
        }

        _formatBuffer.Append("[");
        _formatBuffer.Append(GetLevelTag(level));
        _formatBuffer.Append("]");

        if (!string.IsNullOrEmpty(tag))
        {
            _formatBuffer.Append(" [");
            _formatBuffer.Append(tag);
            _formatBuffer.Append("]");
        }

        _formatBuffer.Append(" ");
        _formatBuffer.Append(message);

        Console.WriteLine(_formatBuffer.ToString());
    }

    public void HandleStackTrace(string stackTrace)
    {
        if (LogConfig.ShowStackTrace && !string.IsNullOrEmpty(stackTrace))
        {
            Console.WriteLine(stackTrace);
        }
    }

    public void Flush()
    {
        // 控制台不需要刷新
    }

    public void Dispose()
    {
        _formatBuffer.Clear();
    }

    private string GetLevelTag(LogLevel level)
    {
        return level switch
        {
            LogLevel.Info => "INFO",
            LogLevel.Warn => "WARN",
            LogLevel.Error => "ERROR",
            _ => "UNKNOWN"
        };
    }
}
