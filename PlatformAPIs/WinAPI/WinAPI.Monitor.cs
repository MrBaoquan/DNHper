using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;

namespace DNHper
{
    public static partial class WinAPI
    {
        #region Monitor Operations
        public static MonitorInfo GetPrimaryMonitorResolution() =>
            GetMonitorInfo(User32.MonitorFromPoint(new POINT(0, 0), MonitorOptions.MONITOR_DEFAULTTOPRIMARY));

        public static List<MonitorInfo> GetAllMonitorsResolution()
        {
            var monitors = new List<MonitorInfo>();
            User32.EnumDisplayMonitors(
                IntPtr.Zero,
                IntPtr.Zero,
                (IntPtr hMonitor, IntPtr hdcMonitor, ref RECT lprcMonitor, IntPtr dwData) =>
                {
                    var info = GetMonitorInfo(hMonitor);
                    if (info != null)
                        monitors.Add(info);
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
            if (!User32.GetMonitorInfo(hMonitor, ref monitorInfo))
                return null;
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
    }
}
