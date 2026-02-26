using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;

namespace DNHper
{
    public static partial class WinAPI
    {
        #region Process Management
        public static string CALLCMD(string InParameter)
        {
            using var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    WindowStyle = ProcessWindowStyle.Hidden,
                    FileName = "cmd.exe",
                    Arguments = InParameter,
                    CreateNoWindow = true,
                    UseShellExecute = false,
                    StandardOutputEncoding = Encoding.Default,
                    RedirectStandardOutput = true
                }
            };
            process.Start();
            var result = process.StandardOutput.ReadToEnd().Trim();
            process.WaitForExit();
            return result;
        }

        public static Process FindProcess(string ProcessFileName) =>
            FindProcesses(ProcessFileName).FirstOrDefault();

        public static List<Process> FindProcesses(string ProcessFileName)
        {
            try
            {
                var processNameToFind = Path.GetFileNameWithoutExtension(ProcessFileName);

                if (Path.IsPathRooted(ProcessFileName))
                {
                    // 完整路径：先按进程名获取，再按完整路径过滤
                    var allByName = Process.GetProcessesByName(processNameToFind);

                    var result = new List<Process>();
                    foreach (var p in allByName)
                    {
                        var actualPath = p.GetMainModuleFileName();
                        var pathsMatch = string.Equals(
                            actualPath,
                            ProcessFileName,
                            StringComparison.OrdinalIgnoreCase
                        );

                        if (pathsMatch)
                        {
                            result.Add(p);
                        }
                    }

                    return result;
                }
                else
                {
                    // 只有文件名：按进程名匹配（已去除.exe）
                    var processes = Process
                        .GetProcesses()
                        .Where(
                            p =>
                                p.ProcessName.Equals(
                                    processNameToFind,
                                    StringComparison.OrdinalIgnoreCase
                                )
                        );
                    var result = processes.ToList();
                    return result;
                }
            }
            catch (Exception ex)
            {
                NLogger.Error("[FindProcesses] 查找进程异常: {0}", ex.Message);
                return new List<Process>();
            }
        }

        public static void KillProcesses(string ProcessFileName) =>
            FindProcesses(ProcessFileName).ForEach(p => p.Kill());

        public static bool OpenProcess(
            string Path,
            string Args = "",
            bool runas = false,
            bool noWindow = false
        ) =>
            CheckValidExecutableFile(Path)
            && TryStartProcess(
                new ProcessStartInfo
                {
                    FileName = Path,
                    Arguments = Args,
                    CreateNoWindow = noWindow,
                    WorkingDirectory = System.IO.Path.GetDirectoryName(Path),
                    Verb = runas ? "runas" : ""
                }
            );

        public static bool OpenProcess(string Path, ProcessStartInfo startInfo) =>
            CheckValidExecutableFile(Path) && TryStartProcess(startInfo);

        public static bool OpenProcessIfNotOpend(string Path, ProcessStartInfo startInfo) =>
            FindProcess(Path) == null && OpenProcess(Path, startInfo);

        public static bool ProcessExists(string ProcessFileName) =>
            FindProcess(ProcessFileName) != null;

        public static bool CheckValidExecutableFile(string path) =>
            new[] { ".exe", ".bat", ".cmd", ".txt" }.Contains(System.IO.Path.GetExtension(path));

        private static bool TryStartProcess(ProcessStartInfo startInfo)
        {
            try
            {
                return Process.Start(startInfo) != null;
            }
            catch (Exception ex)
            {
                NLogger.Error("启动进程失败: {0}, 路径: {1}", ex.Message, startInfo.FileName);
                return false;
            }
        }
        #endregion
    }
}
