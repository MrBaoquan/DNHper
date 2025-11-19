using System;
using System.Diagnostics;
using System.Text;

namespace DNHper
{
    public static partial class WinAPI
    {
        #region Window Operations - Basic API
        public static IntPtr CurrentWindow() => Process.GetCurrentProcess().MainWindowHandle;
        public static bool IsWindowTopMost(IntPtr hWnd) => (User32.GetWindowLong(hWnd, (int)SetWindowLongIndex.GWL_EXSTYLE) & 0x80000) == 0x80000;
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
        public static uint DwmExtendFrameIntoClientArea(IntPtr hWnd, ref MARGINS margins) => Dwmapi.DwmExtendFrameIntoClientArea(hWnd, ref margins);
        public static int SetWindowPos(IntPtr hWnd, int hWndInsertAfter, int x, int y, int Width, int Height, SetWindowPosFlags flags) =>
            User32.SetWindowPos(hWnd, hWndInsertAfter, x, y, Width, Height, flags);
        public static int SendMessage(IntPtr hWnd, int Msg, int wParam, StringBuilder lParam) => User32.SendMessage(hWnd, Msg, wParam, lParam);
        public static IntPtr PostMessage(IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam) => User32.PostMessage(hWnd, Msg, wParam, lParam);
        public static bool IsWindowVisible(IntPtr hWnd) => hWnd != IntPtr.Zero && User32.IsWindowVisible(hWnd);
        #endregion
    }
}