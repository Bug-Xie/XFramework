using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LogModule : MonoBehaviour
{
    // 配置日志系统
    [SerializeField] private LogLevel CurrentLevel = LogLevel.Info;           // 设置日志级别
    [SerializeField] private bool UseUnityDebug = true;                   // 使用Unity Debug输出
    [SerializeField] private bool UseConsoleOutput = true;                // 同时输出到控制台
    [SerializeField] private bool  SaveToFile = true;                      // 保存到文件
    [SerializeField] private bool ShowTimestamp = true;                   // 显示时间戳
    [SerializeField] private bool ShowStackTrace = true;                  // 显示堆栈信息
    [SerializeField] private int StackTraceDepth = 5;                    // 堆栈深度
    [SerializeField] private bool ShowSourceLink = true;                  // 显示源代码链接
    [SerializeField] private long MaxLogFileSize = 10 * 1024 * 1024;      // 单个文件10MB
    [SerializeField] private int MaxLogFileCount = 10;                   // 保留10个日志文件
    void Awake()
    {
        // 配置日志系统
        LogConfig.CurrentLevel =CurrentLevel;           // 设置日志级别
        LogConfig.UseUnityDebug = UseUnityDebug;                   // 使用Unity Debug输出
        LogConfig.UseConsoleOutput = UseConsoleOutput;                // 同时输出到控制台
        LogConfig.SaveToFile = SaveToFile;                      // 保存到文件
        LogConfig.ShowTimestamp = ShowTimestamp;                   // 显示时间戳
        LogConfig.ShowStackTrace = ShowStackTrace;                  // 显示堆栈信息
        LogConfig.StackTraceDepth = StackTraceDepth;                    // 堆栈深度
        LogConfig.ShowSourceLink = ShowSourceLink;                  // 显示源代码链接
        LogConfig.MaxLogFileSize =MaxLogFileSize;      // 单个文件10MB
        LogConfig.MaxLogFileCount = MaxLogFileCount;                   // 保留10个日志文件
        Logger.Initialize();
    }

    // Update is called once per frame
    private void OnApplicationQuit()
    {
        Logger.Shutdown();
    }
}
