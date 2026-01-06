using System;
using System.Runtime.InteropServices;
using System.Text;
using System.IO;

namespace DNHper
{
    public static partial class WinAPI
    {
        /// <summary>
        /// 快捷方式操作相关接口
        /// </summary>
        public static class Shortcut
        {
            [ComImport]
            [Guid("00021401-0000-0000-C000-000000000046")]
            private class ShellLink { }

            [ComImport]
            [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
            [Guid("000214F9-0000-0000-C000-000000000046")]
            private interface IShellLinkW
            {
                void GetPath(
                    [Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder pszFile,
                    int cchMaxPath,
                    IntPtr pfd,
                    int fFlags
                );
                void GetIDList(out IntPtr ppidl);
                void SetIDList(IntPtr pidl);
                void GetDescription(
                    [Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder pszName,
                    int cchMaxName
                );
                void SetDescription([MarshalAs(UnmanagedType.LPWStr)] string pszName);
                void GetWorkingDirectory(
                    [Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder pszDir,
                    int cchMaxPath
                );
                void SetWorkingDirectory([MarshalAs(UnmanagedType.LPWStr)] string pszDir);
                void GetArguments(
                    [Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder pszArgs,
                    int cchMaxPath
                );
                void SetArguments([MarshalAs(UnmanagedType.LPWStr)] string pszArgs);
                void GetHotkey(out short pwHotkey);
                void SetHotkey(short wHotkey);
                void GetShowCmd(out int piShowCmd);
                void SetShowCmd(int iShowCmd);
                void GetIconLocation(
                    [Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder pszIconPath,
                    int cchIconPath,
                    out int piIcon
                );
                void SetIconLocation(
                    [MarshalAs(UnmanagedType.LPWStr)] string pszIconPath,
                    int iIcon
                );
                void SetRelativePath(
                    [MarshalAs(UnmanagedType.LPWStr)] string pszPathRel,
                    int dwReserved
                );
                void Resolve(IntPtr hwnd, int fFlags);
                void SetPath([MarshalAs(UnmanagedType.LPWStr)] string pszFile);
            }

            [ComImport]
            [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
            [Guid("0000010b-0000-0000-C000-000000000046")]
            private interface IPersistFile
            {
                void GetClassID(out Guid pClassID);

                [PreserveSig]
                int IsDirty();
                void Load([In, MarshalAs(UnmanagedType.LPWStr)] string pszFileName, uint dwMode);
                void Save(
                    [In, MarshalAs(UnmanagedType.LPWStr)] string pszFileName,
                    [In, MarshalAs(UnmanagedType.Bool)] bool fRemember
                );
                void SaveCompleted([In, MarshalAs(UnmanagedType.LPWStr)] string pszFileName);
                void GetCurFile([In, MarshalAs(UnmanagedType.LPWStr)] string ppszFileName);
            }

            /// <summary>
            /// 标准化路径为 Windows 格式（反斜杠）
            /// </summary>
            private static string NormalizePath(string path)
            {
                if (string.IsNullOrEmpty(path))
                    return path;

                // 替换正斜杠为反斜杠
                path = path.Replace('/', '\\');

                // 获取绝对路径
                try
                {
                    path = Path.GetFullPath(path);
                }
                catch
                {
                    // 如果获取失败，保持原样
                }

                return path;
            }

            /// <summary>
            /// 创建快捷方式
            /// </summary>
            /// <param name="shortcutPath">快捷方式的完整路径（.lnk文件）</param>
            /// <param name="targetPath">目标文件或程序的路径</param>
            /// <param name="arguments">启动参数（可选）</param>
            /// <param name="workingDirectory">工作目录（可选）</param>
            /// <param name="description">描述信息（可选）</param>
            /// <param name="iconPath">图标路径（可选）</param>
            /// <param name="iconIndex">图标索引（默认为0）</param>
            /// <param name="useRelativePath">是否使用相对路径</param>
            /// <returns>是否创建成功</returns>
            public static bool CreateShortcut(
                string shortcutPath,
                string targetPath,
                string arguments = "",
                string workingDirectory = "",
                string description = "",
                string iconPath = "",
                int iconIndex = 0,
                bool useRelativePath = false // 新增参数
            )
            {
                try
                {
                    // 标准化所有路径为 Windows 格式
                    shortcutPath = NormalizePath(shortcutPath);
                    targetPath = NormalizePath(targetPath);

                    IShellLinkW link = (IShellLinkW)new ShellLink();

                    // 设置目标路径
                    link.SetPath(targetPath);

                    // 设置参数
                    if (!string.IsNullOrEmpty(arguments))
                    {
                        link.SetArguments(arguments);
                    }

                    // 设置工作目录
                    string workDir = workingDirectory;
                    if (string.IsNullOrEmpty(workDir) && !string.IsNullOrEmpty(targetPath))
                    {
                        if (Directory.Exists(targetPath))
                        {
                            workDir = targetPath;
                        }
                        else
                        {
                            workDir = Path.GetDirectoryName(targetPath);
                        }
                    }

                    if (!string.IsNullOrEmpty(workDir))
                    {
                        workDir = NormalizePath(workDir);
                        link.SetWorkingDirectory(workDir);
                    }

                    // 设置描述
                    if (!string.IsNullOrEmpty(description))
                    {
                        link.SetDescription(description);
                    }

                    // 设置图标
                    if (!string.IsNullOrEmpty(iconPath))
                    {
                        iconPath = NormalizePath(iconPath);
                        if (File.Exists(iconPath) || Directory.Exists(iconPath))
                        {
                            link.SetIconLocation(iconPath, iconIndex);
                        }
                    }

                    // 设置相对路径
                    if (useRelativePath)
                    {
                        string? shortcutDir = Path.GetDirectoryName(shortcutPath);
                        if (!string.IsNullOrEmpty(shortcutDir))
                        {
                            string? relativePath = GetRelativePath(shortcutDir, targetPath);
                            if (!string.IsNullOrEmpty(relativePath))
                            {
                                link.SetRelativePath(relativePath, 0);
                            }
                        }
                    }

                    // 确保快捷方式目录存在
                    string shortcutDir2 = Path.GetDirectoryName(shortcutPath);
                    if (!Directory.Exists(shortcutDir2))
                    {
                        Directory.CreateDirectory(shortcutDir2);
                    }

                    // 保存快捷方式
                    IPersistFile file = (IPersistFile)link;
                    file.Save(shortcutPath, false);

                    Marshal.ReleaseComObject(file);
                    Marshal.ReleaseComObject(link);

                    return File.Exists(shortcutPath);
                }
                catch (Exception)
                {
                    return false;
                }
            }

            /// <summary>
            /// 计算相对路径
            /// </summary>
            private static string? GetRelativePath(string fromPath, string toPath)
            {
                if (string.IsNullOrEmpty(fromPath) || string.IsNullOrEmpty(toPath))
                    return null;

                try
                {
                    Uri fromUri = new Uri(fromPath.TrimEnd('\\') + "\\");
                    Uri toUri = new Uri(toPath);

                    Uri relativeUri = fromUri.MakeRelativeUri(toUri);
                    string relativePath = Uri.UnescapeDataString(relativeUri.ToString());

                    // 将正斜杠替换为反斜杠
                    relativePath = relativePath.Replace('/', '\\');

                    return relativePath;
                }
                catch
                {
                    return null;
                }
            }

            /// <summary>
            /// 创建桌面快捷方式
            /// </summary>
            /// <param name="shortcutName">快捷方式名称（不含.lnk扩展名）</param>
            /// <param name="targetPath">目标文件路径</param>
            /// <param name="arguments">启动参数（可选）</param>
            /// <param name="description">描述信息（可选）</param>
            /// <param name="iconPath">图标路径（可选）</param>
            /// <returns>是否创建成功</returns>
            public static bool CreateDesktopShortcut(
                string shortcutName,
                string targetPath,
                string arguments = "",
                string description = "",
                string iconPath = ""
            )
            {
                string desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
                string shortcutPath = Path.Combine(desktopPath, $"{shortcutName}.lnk");

                return CreateShortcut(
                    shortcutPath,
                    targetPath,
                    arguments,
                    "",
                    description,
                    iconPath
                );
            }

            /// <summary>
            /// 创建开始菜单快捷方式
            /// </summary>
            /// <param name="shortcutName">快捷方式名称（不含.lnk扩展名）</param>
            /// <param name="targetPath">目标文件路径</param>
            /// <param name="folderName">开始菜单文件夹名称（可选）</param>
            /// <param name="arguments">启动参数（可选）</param>
            /// <param name="description">描述信息（可选）</param>
            /// <param name="iconPath">图标路径（可选）</param>
            /// <returns>是否创建成功</returns>
            public static bool CreateStartMenuShortcut(
                string shortcutName,
                string targetPath,
                string folderName = "",
                string arguments = "",
                string description = "",
                string iconPath = ""
            )
            {
                string startMenuPath = Environment.GetFolderPath(
                    Environment.SpecialFolder.Programs
                );

                if (!string.IsNullOrEmpty(folderName))
                {
                    startMenuPath = Path.Combine(startMenuPath, folderName);
                    if (!Directory.Exists(startMenuPath))
                    {
                        try
                        {
                            Directory.CreateDirectory(startMenuPath);
                        }
                        catch
                        {
                            return false;
                        }
                    }
                }

                string shortcutPath = Path.Combine(startMenuPath, $"{shortcutName}.lnk");

                return CreateShortcut(
                    shortcutPath,
                    targetPath,
                    arguments,
                    "",
                    description,
                    iconPath
                );
            }

            /// <summary>
            /// 删除快捷方式
            /// </summary>
            /// <param name="shortcutPath">快捷方式的完整路径</param>
            /// <returns>是否删除成功</returns>
            public static bool DeleteShortcut(string shortcutPath)
            {
                try
                {
                    shortcutPath = NormalizePath(shortcutPath);
                    if (File.Exists(shortcutPath))
                    {
                        File.Delete(shortcutPath);
                        return true;
                    }
                    return false;
                }
                catch
                {
                    return false;
                }
            }

            /// <summary>
            /// 检查快捷方式是否存在
            /// </summary>
            /// <param name="shortcutPath">快捷方式的完整路径</param>
            /// <returns>是否存在</returns>
            public static bool ShortcutExists(string shortcutPath)
            {
                shortcutPath = NormalizePath(shortcutPath);
                return File.Exists(shortcutPath);
            }

            /// <summary>
            /// 获取快捷方式目标路径
            /// </summary>
            /// <param name="shortcutPath">快捷方式的完整路径</param>
            /// <returns>目标路径，失败返回null</returns>
            public static string? GetShortcutTarget(string shortcutPath)
            {
                try
                {
                    shortcutPath = NormalizePath(shortcutPath);

                    if (!File.Exists(shortcutPath))
                    {
                        return null;
                    }

                    IShellLinkW link = (IShellLinkW)new ShellLink();
                    IPersistFile file = (IPersistFile)link;

                    file.Load(shortcutPath, 0);

                    StringBuilder path = new StringBuilder(260);
                    link.GetPath(path, path.Capacity, IntPtr.Zero, 0);

                    string targetPath = path.ToString();

                    Marshal.ReleaseComObject(file);
                    Marshal.ReleaseComObject(link);

                    return targetPath;
                }
                catch (Exception)
                {
                    return null;
                }
            }
        }
    }
}
