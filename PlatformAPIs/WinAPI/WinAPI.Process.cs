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

        public static Process FindProcess(string ProcessFileName) => FindProcesses(ProcessFileName).FirstOrDefault();

        public static List<Process> FindProcesses(string ProcessFileName)
        {
            try
            {
                var processes = Path.IsPathRooted(ProcessFileName)
                    ? Process.GetProcessesByName(Path.GetFileNameWithoutExtension(ProcessFileName))
                        .Where(p => p.GetMainModuleFileName() == ProcessFileName)
                    : Process.GetProcesses().Where(p => p.ProcessName.Equals(ProcessFileName, StringComparison.OrdinalIgnoreCase));
                return processes.ToList();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return new List<Process>();
            }
        }

        public static void KillProcesses(string ProcessFileName) => FindProcesses(ProcessFileName).ForEach(p => p.Kill());

        public static bool OpenProcess(string Path, string Args = "", bool runas = false, bool noWindow = false) =>
            CheckValidExecutableFile(Path) && TryStartProcess(new ProcessStartInfo
            {
                FileName = Path,
                Arguments = Args,
                CreateNoWindow = noWindow,
                WorkingDirectory = System.IO.Path.GetDirectoryName(Path),
                Verb = runas ? "runas" : ""
            });

        public static bool OpenProcess(string Path, ProcessStartInfo startInfo) => 
            CheckValidExecutableFile(Path) && TryStartProcess(startInfo);

        public static bool OpenProcessIfNotOpend(string Path, ProcessStartInfo startInfo) => 
            FindProcess(Path) == null && OpenProcess(Path, startInfo);

        public static bool ProcessExists(string ProcessFileName) => FindProcess(ProcessFileName) != null;

        public static bool CheckValidExecutableFile(string path) => 
            new[] { ".exe", ".bat", ".cmd", ".txt" }.Contains(System.IO.Path.GetExtension(path));

        private static bool TryStartProcess(ProcessStartInfo startInfo)
        {
            try { return Process.Start(startInfo) != null; }
            catch { return false; }
        }
        #endregion
    }
}