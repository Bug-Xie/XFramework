using System;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;
using System.IO;
using System.Collections.Generic;

public partial class BuildPipelineEditor
{
    /// <summary>
    /// 读取所有版本号（如apk=1.2.3, hotfix=1.2.7）
    /// </summary>
    private static Dictionary<string, string> ReadAllVersions()
    {
        var dict = new Dictionary<string, string>();
        if (!File.Exists(BuildHelper.VersionFilePath))
            return dict;
        foreach (var line in File.ReadAllLines(BuildHelper.VersionFilePath))
        {
            var parts = line.Split('=');
            if (parts.Length == 2)
                dict[parts[0].Trim()] = parts[1].Trim();
        }

        return dict;
    }

    private static string GetVersion(string key)
    {
        var dict = ReadAllVersions();
        return dict.ContainsKey(key) ? dict[key] : "1.0.0";
    }

    /// <summary>
    /// 写入所有版本号到version.txt
    /// </summary>
    private static void WriteAllVersions(Dictionary<string, string> dict)
    {
        var lines = dict.Select(kv => $"{kv.Key}={kv.Value}");
        File.WriteAllLines(BuildHelper.VersionFilePath, lines);
    }

    /// <summary>
    /// 设置指定类型的版本号并写入文件
    /// </summary>
    private static void SetVersion(string key, string version)
    {
        var dict = ReadAllVersions();
        dict[key] = version;
        WriteAllVersions(dict);
    }

    /// <summary>
    /// 计算下一个版本号
    /// </summary>
    /// <param name="currentVersion">当前版本号</param>
    /// <param name="isFullBuild">true=次版本号+1，false=修订号+1</param>
    private static string GetNextVersion(string currentVersion, bool isFullBuild)
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
            if (patch >= 99) // 假设修订号最大为999
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
    private static void SetScriptingDefineSymbol(string symbol)
    {
        var buildTargetGroup = EditorUserBuildSettings.selectedBuildTargetGroup;
        var symbols = PlayerSettings.GetScriptingDefineSymbolsForGroup(buildTargetGroup)
            .Split(';')
            .ToList();

        // 移除已有的相关符号，避免重复
        symbols.Remove(BuildHelper.OFFLINE_MODE_SYMBOL);
        symbols.Remove(BuildHelper.ASSETBUNDLE_MODE_SYMBOL);

        // 添加目标符号
        if (!symbols.Contains(symbol))
        {
            symbols.Add(symbol);
        }

        PlayerSettings.SetScriptingDefineSymbolsForGroup(buildTargetGroup, string.Join(";", symbols));
        Debug.Log($"已设置编译符号: {symbol}");
    }
}