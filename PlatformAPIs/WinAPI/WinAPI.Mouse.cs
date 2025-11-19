using System;
using System.Runtime.InteropServices;

namespace DNHper
{
    public static partial class WinAPI
    {
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
    }
}
