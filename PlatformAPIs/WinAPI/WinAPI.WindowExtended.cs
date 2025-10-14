using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace DNHper
{
    public static partial class WinAPI
    {
        #region Window Operations - Extended
        public static bool MaximizeWindow(IntPtr hWnd = default) =>
            ShowWindow(hWnd == default ? CurrentWindow() : hWnd, (int)CMDShow.SW_SHOWMAXIMIZED);

        public static bool MinimizeWindow(IntPtr hWnd = default) =>
            ShowWindow(hWnd == default ? CurrentWindow() : hWnd, (int)CMDShow.SW_SHOWMINIMIZED);

        public static bool RestoreWindow(IntPtr hWnd = default) =>
            ShowWindow(hWnd == default ? CurrentWindow() : hWnd, (int)CMDShow.SW_RESTORE);

        public static bool ShowWindowNormal(IntPtr hWnd = default) =>
            ShowWindow(hWnd == default ? CurrentWindow() : hWnd, (int)CMDShow.SW_SHOWNORMAL);

        public static bool HideWindow(IntPtr hWnd = default) => ShowWindow(hWnd == default ? CurrentWindow() : hWnd, (int)CMDShow.SW_HIDE);

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

        public static bool ToggleWindowState(IntPtr hWnd) =>
            hWnd != IntPtr.Zero
            && GetWindowState(hWnd) switch
            {
                WindowState.Minimized or WindowState.Maximized => RestoreWindow(hWnd),
                WindowState.Normal => MinimizeWindow(hWnd),
                WindowState.Hidden => ShowWindowNormal(hWnd),
                _ => false
            };

        public static bool SmartShowWindow(IntPtr hWnd) =>
            hWnd != IntPtr.Zero
            && GetWindowState(hWnd) switch
            {
                WindowState.Hidden => ShowWindowNormal(hWnd) && SetForegroundWindow(hWnd),
                WindowState.Minimized => RestoreWindow(hWnd) && SetForegroundWindow(hWnd),
                WindowState.Normal or WindowState.Maximized => SetForegroundWindow(hWnd),
                _ => false
            };

        public static bool ExecuteWindowAction(IntPtr hWnd, WindowAction action) =>
            hWnd != IntPtr.Zero
            && action switch
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

        public static int BatchWindowControl(IntPtr[] hWnds, WindowAction action) =>
            hWnds?.Count(hWnd => ExecuteWindowAction(hWnd, action)) ?? 0;

        public static int ControlWindowsByProcessName(string processName, WindowAction action) =>
            string.IsNullOrEmpty(processName)
                ? 0
                : BatchWindowControl(
                    FindProcesses(processName).Where(p => p.MainWindowHandle != IntPtr.Zero).Select(p => p.MainWindowHandle).ToArray(),
                    action
                );

        public static int ControlWindowsByTitle(string windowTitle, WindowAction action, bool exactMatch = false) =>
            string.IsNullOrEmpty(windowTitle)
                ? 0
                : BatchWindowControl(
                    Process
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
                        .ToArray(),
                    action
                );

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

        public static List<WindowInfo> GetAllVisibleWindows() =>
            Process
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
        #endregion
    }
}
