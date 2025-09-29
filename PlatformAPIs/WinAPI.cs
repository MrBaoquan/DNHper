using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace DNHper
{
    public static class WinAPI
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
                    ? Process
                        .GetProcessesByName(Path.GetFileNameWithoutExtension(ProcessFileName))
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

        public static bool ProcessExists(string ProcessFileName) => FindProcess(ProcessFileName) != null;

        public static bool CheckValidExecutableFile(string path) =>
            new[] { ".exe", ".bat", ".cmd", ".txt" }.Contains(System.IO.Path.GetExtension(path));

        private static bool TryStartProcess(ProcessStartInfo startInfo)
        {
            try
            {
                return Process.Start(startInfo) != null;
            }
            catch
            {
                return false;
            }
        }
        #endregion

        #region Window Operations
        // 基础 API 封装
        public static IntPtr CurrentWindow() => Process.GetCurrentProcess().MainWindowHandle;

        public static bool IsWindowTopMost(IntPtr hWnd) =>
            (User32.GetWindowLong(hWnd, (int)SetWindowLongIndex.GWL_EXSTYLE) & 0x80000) == 0x80000;

        public static bool IsIconic(IntPtr hWnd) => User32.IsIconic(hWnd);

        public static bool IsZoomed(IntPtr hWnd) => User32.IsZoomed(hWnd);

        public static bool ShowWindow(IntPtr hwnd, int nCmdShow) => User32.ShowWindow(hwnd, nCmdShow);

        public static IntPtr GetForegroundWindow() => User32.GetForegroundWindow();

        public static bool SetForegroundWindow(IntPtr hWnd) => User32.SetForegroundWindow(hWnd);

        public static IntPtr SetFocus(IntPtr hWnd) => User32.SetFocus(hWnd);

        public static IntPtr FindWindow(string lpClassName, string lpWindowName) => User32.FindWindow(lpClassName, lpWindowName);

        public static uint GetLastError() => Kernel32.GetLastError();

        public static bool IsHungAppWindow(IntPtr hWnd) => User32.IsHungAppWindow(hWnd);

        public static int GetWindowLong(IntPtr hWnd, int nIndex) => User32.GetWindowLong(hWnd, nIndex);

        public static int SetWindowLong(IntPtr hWnd, int nIndex, uint dwNewLong) => User32.SetWindowLong(hWnd, nIndex, dwNewLong);

        public static uint DwmExtendFrameIntoClientArea(IntPtr hWnd, ref MARGINS margins) =>
            Dwmapi.DwmExtendFrameIntoClientArea(hWnd, ref margins);

        public static int SetWindowPos(IntPtr hWnd, int hWndInsertAfter, int x, int y, int Width, int Height, SetWindowPosFlags flags) =>
            User32.SetWindowPos(hWnd, hWndInsertAfter, x, y, Width, Height, flags);

        public static int SendMessage(IntPtr hWnd, int Msg, int wParam, StringBuilder lParam) =>
            User32.SendMessage(hWnd, Msg, wParam, lParam);

        public static IntPtr PostMessage(IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam) =>
            User32.PostMessage(hWnd, Msg, wParam, lParam);

        public static bool IsWindowVisible(IntPtr hWnd) => hWnd != IntPtr.Zero && User32.IsWindowVisible(hWnd);

        // 扩展窗口控制功能
        public static bool MaximizeWindow() => MaximizeWindow(CurrentWindow());

        public static bool MaximizeWindow(IntPtr hWnd) => hWnd != IntPtr.Zero && ShowWindow(hWnd, (int)CMDShow.SW_SHOWMAXIMIZED);

        public static bool MinimizeWindow() => MinimizeWindow(CurrentWindow());

        public static bool MinimizeWindow(IntPtr hWnd) => hWnd != IntPtr.Zero && ShowWindow(hWnd, (int)CMDShow.SW_SHOWMINIMIZED);

        public static bool RestoreWindow() => RestoreWindow(CurrentWindow());

        public static bool RestoreWindow(IntPtr hWnd) => hWnd != IntPtr.Zero && ShowWindow(hWnd, (int)CMDShow.SW_RESTORE);

        public static bool ShowWindowNormal() => ShowWindowNormal(CurrentWindow());

        public static bool ShowWindowNormal(IntPtr hWnd) => hWnd != IntPtr.Zero && ShowWindow(hWnd, (int)CMDShow.SW_SHOWNORMAL);

        public static bool HideWindow() => HideWindow(CurrentWindow());

        public static bool HideWindow(IntPtr hWnd) => hWnd != IntPtr.Zero && ShowWindow(hWnd, (int)CMDShow.SW_HIDE);

        public static bool ShowWindowNoActivate(IntPtr hWnd) => hWnd != IntPtr.Zero && ShowWindow(hWnd, (int)CMDShow.SW_SHOW);

        public static WindowState GetWindowState(IntPtr hWnd)
        {
            if (hWnd == IntPtr.Zero)
                return WindowState.Invalid;
            if (!IsWindowVisible(hWnd))
                return WindowState.Hidden;
            if (IsIconic(hWnd))
                return WindowState.Minimized;
            if (IsZoomed(hWnd))
                return WindowState.Maximized;
            return WindowState.Normal;
        }

        public static string GetWindowTitle(IntPtr hWnd)
        {
            if (hWnd == IntPtr.Zero)
                return string.Empty;
            try
            {
                int length = User32.GetWindowTextLength(hWnd);
                if (length == 0)
                    return string.Empty;
                var sb = new StringBuilder(length + 1);
                User32.GetWindowText(hWnd, sb, sb.Capacity);
                return sb.ToString();
            }
            catch
            {
                return string.Empty;
            }
        }

        public static bool ToggleWindowState(IntPtr hWnd)
        {
            if (hWnd == IntPtr.Zero)
                return false;
            return GetWindowState(hWnd) switch
            {
                WindowState.Minimized => RestoreWindow(hWnd),
                WindowState.Maximized => RestoreWindow(hWnd),
                WindowState.Normal => MinimizeWindow(hWnd),
                WindowState.Hidden => ShowWindowNormal(hWnd),
                _ => false
            };
        }

        public static bool SmartShowWindow(IntPtr hWnd)
        {
            if (hWnd == IntPtr.Zero)
                return false;
            return GetWindowState(hWnd) switch
            {
                WindowState.Hidden => ShowWindowNormal(hWnd) && SetForegroundWindow(hWnd),
                WindowState.Minimized => RestoreWindow(hWnd) && SetForegroundWindow(hWnd),
                WindowState.Normal or WindowState.Maximized => SetForegroundWindow(hWnd),
                _ => false
            };
        }

        public static bool ExecuteWindowAction(IntPtr hWnd, WindowAction action)
        {
            if (hWnd == IntPtr.Zero)
                return false;
            return action switch
            {
                WindowAction.Maximize => MaximizeWindow(hWnd),
                WindowAction.Minimize => MinimizeWindow(hWnd),
                WindowAction.Restore => RestoreWindow(hWnd),
                WindowAction.Show => ShowWindowNormal(hWnd),
                WindowAction.Hide => HideWindow(hWnd),
                WindowAction.Toggle => ToggleWindowState(hWnd),
                WindowAction.Activate => SmartShowWindow(hWnd),
                _ => false
            };
        }

        public static int BatchWindowControl(IntPtr[] hWnds, WindowAction action) =>
            hWnds?.Count(hWnd => ExecuteWindowAction(hWnd, action)) ?? 0;

        public static int ControlWindowsByProcessName(string processName, WindowAction action)
        {
            if (string.IsNullOrEmpty(processName))
                return 0;
            var hWnds = FindProcesses(processName).Where(p => p.MainWindowHandle != IntPtr.Zero).Select(p => p.MainWindowHandle).ToArray();
            return BatchWindowControl(hWnds, action);
        }

        public static int ControlWindowsByTitle(string windowTitle, WindowAction action, bool exactMatch = false)
        {
            if (string.IsNullOrEmpty(windowTitle))
                return 0;
            var matchingWindows = Process
                .GetProcesses()
                .Where(p => p.MainWindowHandle != IntPtr.Zero)
                .Select(p => new { Handle = p.MainWindowHandle, Title = GetWindowTitle(p.MainWindowHandle) })
                .Where(
                    w =>
                        exactMatch
                            ? w.Title.Equals(windowTitle, StringComparison.OrdinalIgnoreCase)
                            : w.Title.Contains(windowTitle, StringComparison.OrdinalIgnoreCase)
                )
                .Select(w => w.Handle)
                .ToArray();
            return BatchWindowControl(matchingWindows, action);
        }

        public static WindowInfo GetWindowInfo(IntPtr hWnd)
        {
            if (hWnd == IntPtr.Zero)
                return null;
            try
            {
                return new WindowInfo
                {
                    Handle = hWnd,
                    Title = GetWindowTitle(hWnd),
                    State = GetWindowState(hWnd),
                    IsVisible = IsWindowVisible(hWnd),
                    IsTopMost = IsWindowTopMost(hWnd),
                    IsHung = IsHungAppWindow(hWnd)
                };
            }
            catch
            {
                return null;
            }
        }

        public static List<WindowInfo> GetAllVisibleWindows()
        {
            return Process
                .GetProcesses()
                .Where(p => p.MainWindowHandle != IntPtr.Zero)
                .Select(p => new { Process = p, Info = GetWindowInfo(p.MainWindowHandle) })
                .Where(x => x.Info?.IsVisible == true)
                .Select(x =>
                {
                    x.Info.ProcessName = x.Process.ProcessName;
                    x.Info.ProcessId = x.Process.Id;
                    return x.Info;
                })
                .ToList();
        }
        #endregion

        #region Mouse Operations
        public static void SetCursorPos(int x, int y) => User32.SetCursorPos(x, y);

        public static bool ShowCursor(bool bShow) => User32.ShowCursor(bShow);

        public static void ClickLeftMouseButton()
        {
            var inputs = new[] { InputHelper.CreateMouseInput(MOUSEEVENTF_LEFTDOWN), InputHelper.CreateMouseInput(MOUSEEVENTF_LEFTUP) };
            if (User32.SendInput((uint)inputs.Length, inputs, Marshal.SizeOf<INPUT>()) == 0)
                Console.WriteLine("鼠标点击发送失败");
        }

        public static void mouse_event(int dwFlags, int dx, int dy, int cButtons, int dwExtraInfo) =>
            User32.mouse_event(dwFlags, dx, dy, cButtons, dwExtraInfo);
        #endregion

        #region Monitor/Display Operations
        public static MonitorInfo GetPrimaryMonitorResolution()
        {
            var primaryMonitor = User32.MonitorFromPoint(new POINT(0, 0), MonitorOptions.MONITOR_DEFAULTTOPRIMARY);
            var monitorInfo = new MONITORINFOEX { cbSize = Marshal.SizeOf<MONITORINFOEX>() };

            if (User32.GetMonitorInfo(primaryMonitor, ref monitorInfo))
            {
                var rect = monitorInfo.rcMonitor;
                return new MonitorInfo
                {
                    DeviceName = monitorInfo.szDevice,
                    Width = rect.right - rect.left,
                    Height = rect.bottom - rect.top,
                    Left = rect.left,
                    Top = rect.top,
                    IsPrimary = true
                };
            }
            return null;
        }

        public static List<MonitorInfo> GetAllMonitorsResolution()
        {
            var monitors = new List<MonitorInfo>();
            User32.EnumDisplayMonitors(
                IntPtr.Zero,
                IntPtr.Zero,
                (IntPtr hMonitor, IntPtr hdcMonitor, ref RECT lprcMonitor, IntPtr dwData) =>
                {
                    var monitorInfo = new MONITORINFOEX { cbSize = Marshal.SizeOf<MONITORINFOEX>() };
                    if (User32.GetMonitorInfo(hMonitor, ref monitorInfo))
                    {
                        var rect = monitorInfo.rcMonitor;
                        monitors.Add(
                            new MonitorInfo
                            {
                                DeviceName = monitorInfo.szDevice,
                                Width = rect.right - rect.left,
                                Height = rect.bottom - rect.top,
                                Left = rect.left,
                                Top = rect.top,
                                IsPrimary = (monitorInfo.dwFlags & MONITORINFOF_PRIMARY) != 0
                            }
                        );
                    }
                    return true;
                },
                IntPtr.Zero
            );
            return monitors;
        }

        public static MonitorInfo GetMonitorFromPoint(int x, int y) =>
            GetMonitorInfo(User32.MonitorFromPoint(new POINT(x, y), MonitorOptions.MONITOR_DEFAULTTONEAREST));

        public static MonitorInfo GetMonitorFromWindow(IntPtr hWnd) =>
            GetMonitorInfo(User32.MonitorFromWindow(hWnd, MonitorOptions.MONITOR_DEFAULTTONEAREST));

        private static MonitorInfo GetMonitorInfo(IntPtr hMonitor)
        {
            var monitorInfo = new MONITORINFOEX { cbSize = Marshal.SizeOf<MONITORINFOEX>() };
            if (User32.GetMonitorInfo(hMonitor, ref monitorInfo))
            {
                var rect = monitorInfo.rcMonitor;
                return new MonitorInfo
                {
                    DeviceName = monitorInfo.szDevice,
                    Width = rect.right - rect.left,
                    Height = rect.bottom - rect.top,
                    Left = rect.left,
                    Top = rect.top,
                    IsPrimary = (monitorInfo.dwFlags & MONITORINFOF_PRIMARY) != 0
                };
            }
            return null;
        }

        public static int GetMonitorCount() => User32.GetSystemMetrics(SystemMetric.SM_CMONITORS);

        public static (int Width, int Height) GetVirtualScreenSize() =>
            (User32.GetSystemMetrics(SystemMetric.SM_CXVIRTUALSCREEN), User32.GetSystemMetrics(SystemMetric.SM_CYVIRTUALSCREEN));

        public static ExtendedDesktopInfo GetExtendedDesktopResolution()
        {
            var monitors = GetAllMonitorsResolution();
            if (monitors?.Count == 0)
                return null;

            int minX = monitors.Min(m => m.Left),
                minY = monitors.Min(m => m.Top);
            int maxX = monitors.Max(m => m.Left + m.Width),
                maxY = monitors.Max(m => m.Top + m.Height);

            return new ExtendedDesktopInfo
            {
                TotalWidth = maxX - minX,
                TotalHeight = maxY - minY,
                LeftBound = minX,
                TopBound = minY,
                RightBound = maxX,
                BottomBound = maxY,
                MonitorCount = monitors.Count,
                Monitors = monitors,
                VirtualScreenSize = GetVirtualScreenSize()
            };
        }

        public static (int TotalWidth, int TotalHeight) GetExtendedDesktopSize()
        {
            var info = GetExtendedDesktopResolution();
            return info != null ? (info.TotalWidth, info.TotalHeight) : (0, 0);
        }

        public static bool IsPointInExtendedDesktop(int x, int y)
        {
            var info = GetExtendedDesktopResolution();
            return info != null && x >= info.LeftBound && x < info.RightBound && y >= info.TopBound && y < info.BottomBound;
        }

        public static (int CenterX, int CenterY) GetExtendedDesktopCenter()
        {
            var info = GetExtendedDesktopResolution();
            return info != null ? (info.LeftBound + info.TotalWidth / 2, info.TopBound + info.TotalHeight / 2) : (0, 0);
        }
        #endregion

        #region HotKey Management
        public static bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk) =>
            User32.RegisterHotKey(hWnd, id, fsModifiers, vk);

        public static bool UnregisterHotKey(IntPtr hWnd, int id) => User32.UnregisterHotKey(hWnd, id);
        #endregion

        #region System Operations
        public static bool ShutdownBlockReasonCreate(IntPtr hWnd, string pwszReason) => User32.ShutdownBlockReasonCreate(hWnd, pwszReason);

        public static bool ShutdownBlockReasonDestroy(IntPtr hWnd) => User32.ShutdownBlockReasonDestroy(hWnd);
        #endregion

        #region Audio Operations
        private static IMMDeviceEnumerator _deviceEnumerator;
        private static IAudioEndpointVolume _audioEndpointVolume;
        private static bool _audioInitialized = false;

        /// <summary>
        /// 初始化音频系统
        /// </summary>
        private static bool InitializeAudio()
        {
            if (_audioInitialized && _audioEndpointVolume != null)
                return true;

            try
            {
                // 初始化 COM
                Ole32.CoInitialize(IntPtr.Zero);

                // 创建设备枚举器
                var clsid = AudioConstants.CLSID_MMDeviceEnumerator;
                var iid = AudioConstants.IID_IMMDeviceEnumerator;

                int hr = Ole32.CoCreateInstance(ref clsid, IntPtr.Zero, AudioConstants.CLSCTX_ALL, ref iid, out IntPtr deviceEnumeratorPtr);

                if (hr != 0 || deviceEnumeratorPtr == IntPtr.Zero)
                    return false;

                _deviceEnumerator = Marshal.GetObjectForIUnknown(deviceEnumeratorPtr) as IMMDeviceEnumerator;
                Marshal.Release(deviceEnumeratorPtr);

                if (_deviceEnumerator == null)
                    return false;

                // 获取默认音频设备
                hr = _deviceEnumerator.GetDefaultAudioEndpoint(DataFlow.Render, Role.Multimedia, out IMMDevice device);
                if (hr != 0 || device == null)
                    return false;

                // 激活音量控制接口
                var audioEndpointVolumeIid = AudioConstants.IID_IAudioEndpointVolume;
                hr = device.Activate(ref audioEndpointVolumeIid, AudioConstants.CLSCTX_ALL, IntPtr.Zero, out IntPtr audioEndpointVolumePtr);

                if (hr != 0 || audioEndpointVolumePtr == IntPtr.Zero)
                    return false;

                _audioEndpointVolume = Marshal.GetObjectForIUnknown(audioEndpointVolumePtr) as IAudioEndpointVolume;
                Marshal.Release(audioEndpointVolumePtr);

                _audioInitialized = _audioEndpointVolume != null;
                return _audioInitialized;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"音频初始化失败: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// 清理音频资源
        /// </summary>
        public static void CleanupAudio()
        {
            try
            {
                if (_audioEndpointVolume != null)
                {
                    Marshal.ReleaseComObject(_audioEndpointVolume);
                    _audioEndpointVolume = null;
                }

                if (_deviceEnumerator != null)
                {
                    Marshal.ReleaseComObject(_deviceEnumerator);
                    _deviceEnumerator = null;
                }

                _audioInitialized = false;
                Ole32.CoUninitialize();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"音频清理失败: {ex.Message}");
            }
        }

        /// <summary>
        /// 设置系统主音量 (0.0 - 1.0)
        /// </summary>
        /// <param name="volume">音量级别，范围 0.0 到 1.0</param>
        /// <returns>设置是否成功</returns>
        public static bool SetMasterVolume(float volume)
        {
            if (!InitializeAudio())
                return false;

            try
            {
                // 确保音量在有效范围内
                volume = Math.Max(0.0f, Math.Min(1.0f, volume));

                var guid = AudioConstants.GUID_NULL;
                int hr = _audioEndpointVolume.SetMasterVolumeLevelScalar(volume, ref guid);
                return hr == 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"设置音量失败: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// 获取系统主音量 (0.0 - 1.0)
        /// </summary>
        /// <returns>当前音量级别，失败时返回 -1</returns>
        public static float GetMasterVolume()
        {
            if (!InitializeAudio())
                return -1;

            try
            {
                int hr = _audioEndpointVolume.GetMasterVolumeLevelScalar(out float volume);
                return hr == 0 ? volume : -1;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"获取音量失败: {ex.Message}");
                return -1;
            }
        }

        /// <summary>
        /// 设置系统主音量 (0 - 100)
        /// </summary>
        /// <param name="volumePercent">音量百分比，范围 0 到 100</param>
        /// <returns>设置是否成功</returns>
        public static bool SetMasterVolumePercent(int volumePercent)
        {
            volumePercent = Math.Max(0, Math.Min(100, volumePercent));
            return SetMasterVolume(volumePercent / 100.0f);
        }

        /// <summary>
        /// 获取系统主音量 (0 - 100)
        /// </summary>
        /// <returns>当前音量百分比，失败时返回 -1</returns>
        public static int GetMasterVolumePercent()
        {
            float volume = GetMasterVolume();
            return volume >= 0 ? (int)Math.Round(volume * 100) : -1;
        }

        /// <summary>
        /// 设置静音状态
        /// </summary>
        /// <param name="mute">是否静音</param>
        /// <returns>设置是否成功</returns>
        public static bool SetMute(bool mute)
        {
            if (!InitializeAudio())
                return false;

            try
            {
                var guid = AudioConstants.GUID_NULL;
                int hr = _audioEndpointVolume.SetMute(mute, ref guid);
                return hr == 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"设置静音失败: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// 获取静音状态
        /// </summary>
        /// <returns>是否静音，失败时返回 null</returns>
        public static bool? GetMute()
        {
            if (!InitializeAudio())
                return null;

            try
            {
                int hr = _audioEndpointVolume.GetMute(out bool mute);
                return hr == 0 ? mute : null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"获取静音状态失败: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// 切换静音状态
        /// </summary>
        /// <returns>切换后的静音状态，失败时返回 null</returns>
        public static bool? ToggleMute()
        {
            bool? currentMute = GetMute();
            if (currentMute == null)
                return null;

            bool newMute = !currentMute.Value;
            return SetMute(newMute) ? newMute : null;
        }

        /// <summary>
        /// 音量步进增加
        /// </summary>
        /// <returns>操作是否成功</returns>
        public static bool VolumeStepUp()
        {
            if (!InitializeAudio())
                return false;

            try
            {
                var guid = AudioConstants.GUID_NULL;
                int hr = _audioEndpointVolume.VolumeStepUp(ref guid);
                return hr == 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"音量步进增加失败: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// 音量步进减少
        /// </summary>
        /// <returns>操作是否成功</returns>
        public static bool VolumeStepDown()
        {
            if (!InitializeAudio())
                return false;

            try
            {
                var guid = AudioConstants.GUID_NULL;
                int hr = _audioEndpointVolume.VolumeStepDown(ref guid);
                return hr == 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"音量步进减少失败: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// 获取音量步进信息
        /// </summary>
        /// <returns>当前步进和总步进数，失败时返回 null</returns>
        public static (uint currentStep, uint totalSteps)? GetVolumeStepInfo()
        {
            if (!InitializeAudio())
                return null;

            try
            {
                int hr = _audioEndpointVolume.GetVolumeStepInfo(out uint currentStep, out uint totalSteps);
                return hr == 0 ? (currentStep, totalSteps) : null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"获取音量步进信息失败: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// 获取音量范围信息 (分贝)
        /// </summary>
        /// <returns>最小、最大和增量分贝值，失败时返回 null</returns>
        public static (float minDB, float maxDB, float incrementDB)? GetVolumeRange()
        {
            if (!InitializeAudio())
                return null;

            try
            {
                int hr = _audioEndpointVolume.GetVolumeRange(out float minDB, out float maxDB, out float incrementDB);
                return hr == 0 ? (minDB, maxDB, incrementDB) : null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"获取音量范围失败: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// 获取系统音频信息
        /// </summary>
        /// <returns>音频系统信息</returns>
        public static AudioSystemInfo GetAudioSystemInfo()
        {
            var info = new AudioSystemInfo
            {
                IsInitialized = _audioInitialized,
                Volume = GetMasterVolume(),
                VolumePercent = GetMasterVolumePercent(),
                IsMuted = GetMute(),
                VolumeStepInfo = GetVolumeStepInfo(),
                VolumeRange = GetVolumeRange()
            };

            return info;
        }

        /// <summary>
        /// 智能音量控制
        /// </summary>
        /// <param name="action">音量操作</param>
        /// <param name="value">操作值（用于设置音量时）</param>
        /// <returns>操作是否成功</returns>
        public static bool SmartVolumeControl(VolumeAction action, float value = 0)
        {
            return action switch
            {
                VolumeAction.SetVolume => SetMasterVolume(value),
                VolumeAction.SetVolumePercent => SetMasterVolumePercent((int)value),
                VolumeAction.Mute => SetMute(true),
                VolumeAction.Unmute => SetMute(false),
                VolumeAction.ToggleMute => ToggleMute() != null,
                VolumeAction.VolumeUp => VolumeStepUp(),
                VolumeAction.VolumeDown => VolumeStepDown(),
                _ => false
            };
        }
        #endregion

        #region Constants
        public const int MOUSEEVENTF_LEFTDOWN = 0x2,
            MOUSEEVENTF_LEFTUP = 0x4,
            MOUSEEVENTF_MIDDLEDOWN = 0x20,
            MOUSEEVENTF_MIDDLEUP = 0x40,
            MOUSEEVENTF_MOVE = 0x1,
            MOUSEEVENTF_RIGHTDOWN = 0x8,
            MOUSEEVENTF_RIGHTUP = 0x10;
        private const uint MONITORINFOF_PRIMARY = 0x00000001;
        #endregion
    }

    // 窗口信息类
    public class WindowInfo
    {
        public IntPtr Handle { get; set; }
        public string Title { get; set; }
        public WindowState State { get; set; }
        public bool IsVisible { get; set; }
        public bool IsTopMost { get; set; }
        public bool IsHung { get; set; }
        public string ProcessName { get; set; }
        public int ProcessId { get; set; }

        public override string ToString() =>
            $"[{ProcessName}:{ProcessId}] {Title} - {State}" + (IsTopMost ? " [TopMost]" : "") + (IsHung ? " [Hung]" : "");
    }

    // 显示器信息类
    public class MonitorInfo
    {
        public string DeviceName { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public int Left { get; set; }
        public int Top { get; set; }
        public bool IsPrimary { get; set; }

        public override string ToString() => $"{DeviceName}: {Width}x{Height} at ({Left},{Top}){(IsPrimary ? " [Primary]" : "")}";
    }

    // 扩展桌面信息类
    public class ExtendedDesktopInfo
    {
        public int TotalWidth { get; set; }
        public int TotalHeight { get; set; }
        public int LeftBound { get; set; }
        public int TopBound { get; set; }
        public int RightBound { get; set; }
        public int BottomBound { get; set; }
        public int MonitorCount { get; set; }
        public List<MonitorInfo> Monitors { get; set; }
        public (int Width, int Height) VirtualScreenSize { get; set; }

        public (int X, int Y) Center => (LeftBound + TotalWidth / 2, TopBound + TotalHeight / 2);
        public long TotalArea => (long)TotalWidth * TotalHeight;
        public long ActualMonitorArea => Monitors?.Sum(m => (long)m.Width * m.Height) ?? 0;
        public double LayoutEfficiency => TotalArea > 0 ? (double)ActualMonitorArea / TotalArea : 0;

        public override string ToString() =>
            $"扩展桌面: {TotalWidth}x{TotalHeight} 范围: ({LeftBound},{TopBound}) 到 ({RightBound},{BottomBound}) 显示器数量: {MonitorCount} 布局效率: {LayoutEfficiency:P2}";

        public string GetDetailedInfo()
        {
            var sb = new StringBuilder();
            sb.AppendLine($"扩展桌面详细信息:");
            sb.AppendLine($"  总分辨率: {TotalWidth} x {TotalHeight}");
            sb.AppendLine($"  边界范围: ({LeftBound}, {TopBound}) 到 ({RightBound}, {BottomBound})");
            sb.AppendLine($"  中心点: {Center.X}, {Center.Y}");
            sb.AppendLine($"  总面积: {TotalArea:N0} 像素");
            sb.AppendLine($"  显示器数量: {MonitorCount}");
            sb.AppendLine($"  实际显示面积: {ActualMonitorArea:N0} 像素");
            sb.AppendLine($"  布局效率: {LayoutEfficiency:P2}");
            sb.AppendLine($"  系统虚拟屏幕: {VirtualScreenSize.Width} x {VirtualScreenSize.Height}");

            if (Monitors?.Count > 0)
            {
                sb.AppendLine("  各显示器信息:");
                for (int i = 0; i < Monitors.Count; i++)
                    sb.AppendLine($"    [{i + 1}] {Monitors[i]}");
            }
            return sb.ToString();
        }
    }

    /// <summary>
    /// 音频系统信息类
    /// </summary>
    public class AudioSystemInfo
    {
        public bool IsInitialized { get; set; }
        public float Volume { get; set; }
        public int VolumePercent { get; set; }
        public bool? IsMuted { get; set; }
        public (uint currentStep, uint totalSteps)? VolumeStepInfo { get; set; }
        public (float minDB, float maxDB, float incrementDB)? VolumeRange { get; set; }

        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.AppendLine("音频系统信息:");
            sb.AppendLine($"  初始化状态: {(IsInitialized ? "已初始化" : "未初始化")}");
            sb.AppendLine($"  当前音量: {Volume:F2} ({VolumePercent}%)");
            sb.AppendLine($"  静音状态: {(IsMuted?.ToString() ?? "未知")}");

            if (VolumeStepInfo.HasValue)
            {
                sb.AppendLine($"  音量步进: {VolumeStepInfo.Value.currentStep}/{VolumeStepInfo.Value.totalSteps}");
            }

            if (VolumeRange.HasValue)
            {
                var range = VolumeRange.Value;
                sb.AppendLine($"  音量范围: {range.minDB:F1}dB 到 {range.maxDB:F1}dB (增量: {range.incrementDB:F1}dB)");
            }

            return sb.ToString();
        }
    }

    public static class ProcessExtensions
    {
        public static string GetMainModuleFileName(this Process process)
        {
            try
            {
                return process.MainModule?.FileName ?? string.Empty;
            }
            catch
            {
                return string.Empty;
            }
        }
    }
}
