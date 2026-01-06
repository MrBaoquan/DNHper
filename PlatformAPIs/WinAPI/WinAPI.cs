using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace DNHper
{
    public static partial class WinAPI
    {
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

        #region Audio Static Fields
        private static IMMDeviceEnumerator _deviceEnumerator;
        private static IAudioEndpointVolume _audioEndpointVolume;
        private static bool _audioInitialized = false;
        #endregion
    }

    #region Data Classes
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
            $"[{ProcessName}:{ProcessId}] {Title} - {State}"
            + (IsTopMost ? " [TopMost]" : "")
            + (IsHung ? " [Hung]" : "");
    }

    public class MonitorInfo
    {
        public string DeviceName { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public int Left { get; set; }
        public int Top { get; set; }
        public bool IsPrimary { get; set; }

        public override string ToString() =>
            $"{DeviceName}: {Width}x{Height} at ({Left},{Top}){(IsPrimary ? " [Primary]" : "")}";
    }

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
                sb.AppendLine(
                    $"  音量步进: {VolumeStepInfo.Value.currentStep}/{VolumeStepInfo.Value.totalSteps}"
                );
            if (VolumeRange.HasValue)
            {
                var range = VolumeRange.Value;
                sb.AppendLine(
                    $"  音量范围: {range.minDB:F1}dB 到 {range.maxDB:F1}dB (增量: {range.incrementDB:F1}dB)"
                );
            }
            return sb.ToString();
        }
    }

    public static class ProcessExtensions
    {
        [DllImport("Kernel32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool QueryFullProcessImageName(
            [In] IntPtr hProcess,
            [In] uint dwFlags,
            [Out] StringBuilder lpExeName,
            [In, Out] ref uint lpdwSize
        );

        public static string GetMainModuleFileName(this Process process)
        {
            try
            {
                // 优先使用QueryFullProcessImageName，支持跨位数访问
                var fileNameBuilder = new StringBuilder(1024);
                uint bufferLength = (uint)fileNameBuilder.Capacity + 1;
                if (QueryFullProcessImageName(process.Handle, 0, fileNameBuilder, ref bufferLength))
                {
                    return fileNameBuilder.ToString();
                }

                // 降级到MainModule（可能在跨位数时失败）
                return process.MainModule?.FileName ?? string.Empty;
            }
            catch
            {
                return string.Empty;
            }
        }
    }
    #endregion
}
