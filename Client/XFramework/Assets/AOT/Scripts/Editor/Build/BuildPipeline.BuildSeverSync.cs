using System;
using UnityEngine;
using System.IO;


    public partial class BuildPipelineEditor
    {
        static void BuildFullSeverSync()
        {
            CleanSeverRes();
            SeverSyncRes();
        }
        
        static void BuildSeverSync()
        {
            SeverSyncRes();
        }

        static void SeverSyncRes()
        {
            string gitBashPath = @"C:\MyPart\Work\Git\Git\bin\bash.exe";
            string scriptPath = Application.dataPath + "Assets/AOT/Scripts/Editor/SeverSyncRes.sh";
            Debug.Log("脚本路径: " + scriptPath);

            var process = new System.Diagnostics.Process
            {
                StartInfo = new System.Diagnostics.ProcessStartInfo
                {
                    FileName = gitBashPath,
                    Arguments = $"--login -i \"{scriptPath}\"",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                }
            };
            process.Start();
            string output = process.StandardOutput.ReadToEnd();
            string error = process.StandardError.ReadToEnd();
            process.WaitForExit();

            string logContent = $"脚本执行完成，退出代码: {process.ExitCode}\n" +
                                $"标准输出:\n{output}\n" +
                                $"错误输出:\n{error}\n";

            // 保存到项目根目录下的 sync_log.txt
            string logPath = Path.Combine(Application.dataPath, "Assets/AOT/Scripts/Editor/sync_log.txt");
            File.WriteAllText(logPath, logContent);

            Console.WriteLine(logContent);


            Console.WriteLine($"脚本执行完成，退出代码: {process.ExitCode}");
            Console.WriteLine("标准输出:\n" + output);
            Console.WriteLine("错误输出:\n" + error);

            Debug.Log("资源同步命令已执行");
        }

        static void CleanSeverRes()
        {
            // 获取脚本路径 Assets
            string gitBashPath = @"C:\Program Files\Git\bin\bash.exe";
            string scriptPath = Application.dataPath + "Assets/AOT/Scripts/Editor/BuildCleanSeverRes.sh";
            Debug.Log("脚本路径: " + scriptPath);

            var process = new System.Diagnostics.Process
            {
                StartInfo = new System.Diagnostics.ProcessStartInfo
                {
                    FileName = gitBashPath,
                    Arguments = $"--login -i \"{scriptPath}\"",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                }
            };
            process.Start();
            string output = process.StandardOutput.ReadToEnd();
            string error = process.StandardError.ReadToEnd();
            process.WaitForExit();

            Console.WriteLine($"脚本执行完成，退出代码: {process.ExitCode}");
            Console.WriteLine("标准输出:\n" + output);
            Console.WriteLine("错误输出:\n" + error);
            Debug.Log("远程服务器清理命令已执行");
        }
    
}