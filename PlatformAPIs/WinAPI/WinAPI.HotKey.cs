using System;

namespace DNHper
{
    public static partial class WinAPI
    {
        #region HotKey & System
        public static bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk) => User32.RegisterHotKey(hWnd, id, fsModifiers, vk);
        public static bool UnregisterHotKey(IntPtr hWnd, int id) => User32.UnregisterHotKey(hWnd, id);
        public static bool ShutdownBlockReasonCreate(IntPtr hWnd, string pwszReason) => User32.ShutdownBlockReasonCreate(hWnd, pwszReason);
        public static bool ShutdownBlockReasonDestroy(IntPtr hWnd) => User32.ShutdownBlockReasonDestroy(hWnd);
        #endregion
    }
}