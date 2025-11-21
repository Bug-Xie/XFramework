using System;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

public partial class BuildPipelineEditor
{
    /// <summary>
    /// 计算下一个版本号
    /// </summary>
    /// <param name="currentVersion">当前版本号</param>
    /// <param name="isFullBuild">true=主版本号+1，false=修订号+1</param>
    public static string GetNextVersion(string currentVersion, bool isFullBuild)
    {
        var match = Regex.Match(currentVersion, @"^(\d+)\.(\d+)\.(\d+)$");
        if (!match.Success)
            throw new Exception("版本号格式错误，应为 x.x.x");

        int major = int.Parse(match.Groups[1].Value);
        int minor = int.Parse(match.Groups[2].Value);
        int patch = int.Parse(match.Groups[3].Value);

        if (isFullBuild)
        {
            major++;
            minor = 0; // 全量包时次版本号归零
            patch = 0;
        }
        else
        {
            if (patch >= 99) // 假设修订号最大为99
            {
                patch = 0;
                minor++;
            }
            else
            {
                patch++;
            }
        }

        return $"{major}.{minor}.{patch}";
    }

    /// <summary>
    /// 设置编译符号（宏定义），用于区分不同构建模式
    /// </summary>
    public static void SetScriptingDefineSymbol(string symbol)
    {
        var buildTargetGroup = EditorUserBuildSettings.selectedBuildTargetGroup;
        var symbols = PlayerSettings.GetScriptingDefineSymbolsForGroup(buildTargetGroup)
            .Split(';')
            .ToList();

        // 移除已有的相关符号，避免重复
        symbols.Remove(BuildToolPanel.OFFLINE_MODE_SYMBOL);
        symbols.Remove(BuildToolPanel.ASSETBUNDLE_MODE_SYMBOL);

        // 添加目标符号
        if (!symbols.Contains(symbol))
        {
            symbols.Add(symbol);
        }

        PlayerSettings.SetScriptingDefineSymbolsForGroup(buildTargetGroup, string.Join(";", symbols));
        GameToolLogger.WriteLog($"已设置编译符号: {symbol}");
    }

    /// <summary>
    /// 管理EnableLog宏定义
    /// </summary>
    /// <param name="enableLog">true为添加EnableLog宏，false为移除EnableLog宏</param>
    public static void SetEnableLogSymbol(bool enableLog)
    {
        var buildTargetGroup = EditorUserBuildSettings.selectedBuildTargetGroup;
        var symbols = PlayerSettings.GetScriptingDefineSymbolsForGroup(buildTargetGroup)
            .Split(';')
            .ToList();

        const string ENABLE_LOG_SYMBOL = "EnableLog";

        if (enableLog)
        {
            // 添加EnableLog宏
            if (!symbols.Contains(ENABLE_LOG_SYMBOL))
            {
                symbols.Add(ENABLE_LOG_SYMBOL);
                GameToolLogger.WriteLog("已添加EnableLog宏定义");
            }
            else
            {
                GameToolLogger.WriteLog("EnableLog宏定义已存在");
            }
        }
        else
        {
            // 移除EnableLog宏
            if (symbols.Remove(ENABLE_LOG_SYMBOL))
            {
                GameToolLogger.WriteLog("已移除EnableLog宏定义");
            }
            else
            {
                GameToolLogger.WriteLog("EnableLog宏定义不存在，无需移除");
            }
        }

        PlayerSettings.SetScriptingDefineSymbolsForGroup(buildTargetGroup, string.Join(";", symbols));
    }
}
