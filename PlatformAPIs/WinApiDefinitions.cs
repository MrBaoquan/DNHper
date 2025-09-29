using System;
using System.Runtime.InteropServices;
using System.Text;

namespace DNHper
{
    #region API Imports
    internal static class User32
    {
        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool IsIconic(IntPtr hWnd);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool IsZoomed(IntPtr hWnd);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool ShowWindow(IntPtr hwnd, int nCmdShow);

        [DllImport("user32.dll")]
        public static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll")]
        public static extern bool SetForegroundWindow(IntPtr hWnd);

        [DllImport("user32.dll")]
        public static extern IntPtr SetFocus(IntPtr hWnd);

        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        public static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool IsHungAppWindow(IntPtr hWnd);

        [DllImport("user32.dll", EntryPoint = "mouse_event")]
        public static extern void mouse_event(int dwFlags, int dx, int dy, int cButtons, int dwExtraInfo);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern uint SendInput(uint nInputs, INPUT[] pInputs, int cbSize);

        [DllImport("user32.dll", EntryPoint = "GetWindowLongW", CharSet = CharSet.Unicode)]
        public static extern int GetWindowLong(IntPtr hWnd, int nIndex);

        [DllImport("user32.dll", EntryPoint = "SetWindowLongW", CharSet = CharSet.Unicode)]
        public static extern int SetWindowLong(IntPtr hWnd, int nIndex, uint dwNewLong);

        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        public static extern int SetWindowPos(
            IntPtr hWnd,
            int hWndInsertAfter,
            int x,
            int y,
            int Width,
            int Height,
            SetWindowPosFlags flags
        );

        [DllImport("User32")]
        public extern static void SetCursorPos(int x, int y);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern int SendMessage(IntPtr hWnd, int Msg, int wParam, StringBuilder lParam);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern IntPtr PostMessage(IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll", EntryPoint = "ShowCursor")]
        public extern static bool ShowCursor(bool bShow);

        [DllImport("user32.dll")]
        public static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);

        [DllImport("user32.dll")]
        public static extern bool UnregisterHotKey(IntPtr hWnd, int id);

        [DllImport("user32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        public extern static bool ShutdownBlockReasonCreate([In] IntPtr hWnd, [In] string pwszReason);

        [DllImport("user32.dll", SetLastError = true)]
        public extern static bool ShutdownBlockReasonDestroy([In] IntPtr hWnd);

        // 显示器相关API
        [DllImport("user32.dll")]
        public static extern bool EnumDisplayMonitors(IntPtr hdc, IntPtr lprcClip, MonitorEnumDelegate lpfnEnum, IntPtr dwData);

        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        public static extern bool GetMonitorInfo(IntPtr hMonitor, ref MONITORINFOEX lpmi);

        [DllImport("user32.dll")]
        public static extern IntPtr MonitorFromPoint(POINT pt, MonitorOptions dwFlags);

        [DllImport("user32.dll")]
        public static extern IntPtr MonitorFromWindow(IntPtr hwnd, MonitorOptions dwFlags);

        [DllImport("user32.dll")]
        public static extern int GetSystemMetrics(SystemMetric smIndex);

        // 新增的窗口控制相关API
        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool IsWindowVisible(IntPtr hWnd);

        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        public static extern int GetWindowText(IntPtr hWnd, StringBuilder lpString, int nMaxCount);

        [DllImport("user32.dll")]
        public static extern int GetWindowTextLength(IntPtr hWnd);

        public delegate bool MonitorEnumDelegate(IntPtr hMonitor, IntPtr hdcMonitor, ref RECT lprcMonitor, IntPtr dwData);
    }

    internal static class Kernel32
    {
        [DllImport("kernel32.dll")]
        public static extern uint GetLastError();
    }

    internal static class Dwmapi
    {
        [DllImport("Dwmapi.dll")]
        public static extern uint DwmExtendFrameIntoClientArea(IntPtr hWnd, ref MARGINS margins);
    }

    internal static class Ole32
    {
        [DllImport("ole32.dll")]
        public static extern int CoInitialize(IntPtr pvReserved);

        [DllImport("ole32.dll")]
        public static extern void CoUninitialize();

        [DllImport("ole32.dll")]
        public static extern int CoCreateInstance(ref Guid rclsid, IntPtr pUnkOuter, uint dwClsContext, ref Guid riid, out IntPtr ppv);
    }
    #endregion

    #region Structures
    [StructLayout(LayoutKind.Sequential)]
    struct INPUT
    {
        public uint type;
        public MOUSEINPUT mi;
    }

    [StructLayout(LayoutKind.Sequential)]
    struct MOUSEINPUT
    {
        public int dx,
            dy;
        public uint mouseData,
            dwFlags,
            time;
        public IntPtr dwExtraInfo;
    }

    public struct MARGINS
    {
        public int cxLeftWidth,
            cxRightWidth,
            cyTopHeight,
            cyBottomHeight;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct POINT
    {
        public int x,
            y;

        public POINT(int x, int y)
        {
            this.x = x;
            this.y = y;
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct RECT
    {
        public int left,
            top,
            right,
            bottom;
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    public struct MONITORINFOEX
    {
        public int cbSize;
        public RECT rcMonitor;
        public RECT rcWork;
        public uint dwFlags;

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
        public string szDevice;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct PROPERTYKEY
    {
        public Guid fmtid;
        public uint pid;
    }

    [StructLayout(LayoutKind.Explicit)]
    public struct PROPVARIANT
    {
        [FieldOffset(0)]
        public ushort vt;

        [FieldOffset(8)]
        public IntPtr pwszVal;

        [FieldOffset(8)]
        public uint uintVal;
    }

    internal static class InputHelper
    {
        const int INPUT_MOUSE = 0;

        public static INPUT CreateMouseInput(int flags) =>
            new INPUT
            {
                type = INPUT_MOUSE,
                mi = new MOUSEINPUT { dwFlags = (uint)flags }
            };
    }
    #endregion

    #region Audio Interfaces
    [ComImport]
    [Guid("BCDE0395-E52F-467C-8E3D-C4579291692E")]
    internal class MMDeviceEnumeratorComObject { }

    [ComImport]
    [Guid("A95664D2-9614-4F35-A746-DE8DB63617E6")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    internal interface IMMDeviceEnumerator
    {
        int EnumAudioEndpoints(DataFlow dataFlow, DeviceState dwStateMask, out IMMDeviceCollection ppDevices);
        int GetDefaultAudioEndpoint(DataFlow dataFlow, Role role, out IMMDevice ppEndpoint);
        int GetDevice(string pwstrId, out IMMDevice ppDevice);
        int RegisterEndpointNotificationCallback(IMMNotificationClient pClient);
        int UnregisterEndpointNotificationCallback(IMMNotificationClient pClient);
    }

    [ComImport]
    [Guid("0BD7A1BE-7A1A-44DB-8397-CC5392387B5E")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    internal interface IMMDeviceCollection
    {
        int GetCount(out uint pcDevices);
        int Item(uint nDevice, out IMMDevice ppDevice);
    }

    [ComImport]
    [Guid("D666063F-1587-4E43-81F1-B948E807363F")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    internal interface IMMDevice
    {
        int Activate(ref Guid iid, uint dwClsCtx, IntPtr pActivationParams, out IntPtr ppInterface);
        int OpenPropertyStore(uint stgmAccess, out IPropertyStore ppProperties);
        int GetId(out string ppstrId);
        int GetState(out DeviceState pdwState);
    }

    [ComImport]
    [Guid("886D8EEB-8CF2-4446-8D02-CDBA1DBDCF99")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    internal interface IPropertyStore
    {
        int GetCount(out uint cProps);
        int GetAt(uint iProp, out PROPERTYKEY pkey);
        int GetValue(ref PROPERTYKEY key, out PROPVARIANT pv);
        int SetValue(ref PROPERTYKEY key, ref PROPVARIANT propvar);
        int Commit();
    }

    [ComImport]
    [Guid("5CDF2C82-841E-4546-9722-0CF74078229A")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    internal interface IAudioEndpointVolume
    {
        int RegisterControlChangeNotify(IAudioEndpointVolumeCallback pNotify);
        int UnregisterControlChangeNotify(IAudioEndpointVolumeCallback pNotify);
        int GetChannelCount(out uint pnChannelCount);
        int SetMasterVolumeLevel(float fLevelDB, ref Guid pguidEventContext);
        int SetMasterVolumeLevelScalar(float fLevel, ref Guid pguidEventContext);
        int GetMasterVolumeLevel(out float pfLevelDB);
        int GetMasterVolumeLevelScalar(out float pfLevel);
        int SetChannelVolumeLevel(uint nChannel, float fLevelDB, ref Guid pguidEventContext);
        int SetChannelVolumeLevelScalar(uint nChannel, float fLevel, ref Guid pguidEventContext);
        int GetChannelVolumeLevel(uint nChannel, out float pfLevelDB);
        int GetChannelVolumeLevelScalar(uint nChannel, out float pfLevel);
        int SetMute(bool bMute, ref Guid pguidEventContext);
        int GetMute(out bool pbMute);
        int GetVolumeStepInfo(out uint pnStep, out uint pnStepCount);
        int VolumeStepUp(ref Guid pguidEventContext);
        int VolumeStepDown(ref Guid pguidEventContext);
        int QueryHardwareSupport(out uint pdwHardwareSupportMask);
        int GetVolumeRange(out float pflVolumeMindB, out float pflVolumeMaxdB, out float pflVolumeIncrementdB);
    }

    [ComImport]
    [Guid("657804FA-D6AD-4496-8A60-352752AF4F89")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    internal interface IAudioEndpointVolumeCallback
    {
        int OnNotify(IntPtr pNotifyData);
    }

    [ComImport]
    [Guid("7991EEC9-7E89-4D85-8390-6C703CEC60C0")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    internal interface IMMNotificationClient
    {
        int OnDeviceStateChanged(string pwstrDeviceId, DeviceState dwNewState);
        int OnDeviceAdded(string pwstrDeviceId);
        int OnDeviceRemoved(string pwstrDeviceId);
        int OnDefaultDeviceChanged(DataFlow flow, Role role, string pwstrDefaultDeviceId);
        int OnPropertyValueChanged(string pwstrDeviceId, PROPERTYKEY key);
    }
    #endregion

    #region Enums
    public enum CMDShow
    {
        SW_HIDE = 0,
        SW_SHOWNORMAL = 1,
        SW_NORMAL = SW_SHOWNORMAL,
        SW_SHOWMINIMIZED = 2,
        SW_SHOWMAXIMIZED = 3,
        SW_SHOW = 5,
        SW_RESTORE = 9
    }

    public enum HWndInsertAfter
    {
        HWND_TOPMOST = -1,
        HWND_NOTOPMOST = -2,
        HWND_BOTTOM = 1,
        HWND_TOP = 0
    }

    public enum UFullScreenMode
    {
        ExclusiveFullScreen = 0,
        FullScreenWindow = 1,
        MaximizedWindow = 2,
        Windowed = 3,
        MinimizedWindow = 11
    }

    public enum MonitorOptions : uint
    {
        MONITOR_DEFAULTTONULL = 0x00000000,
        MONITOR_DEFAULTTOPRIMARY = 0x00000001,
        MONITOR_DEFAULTTONEAREST = 0x00000002
    }

    public enum SystemMetric : int
    {
        SM_CMONITORS = 80, // 显示器数量
        SM_CXVIRTUALSCREEN = 78, // 虚拟桌面宽度
        SM_CYVIRTUALSCREEN = 79 // 虚拟桌面高度
    }

    [Flags]
    public enum SetWindowPosFlags : uint
    {
        SWP_ASYNCWINDOWPOS = 0x4000,
        SWP_DEFERERASE = 0x2000,
        SWP_DRAWFRAME = 0x0020,
        SWP_FRAMECHANGED = 0x0020,
        SWP_HIDEWINDOW = 0x0080,
        SWP_NOACTIVATE = 0x0010,
        SWP_NOCOPYBITS = 0x0100,
        SWP_NOMOVE = 0x0002,
        SWP_NOOWNERZORDER = 0x0200,
        SWP_NOREDRAW = 0x0008,
        SWP_NOREPOSITION = 0x0200,
        SWP_NOSENDCHANGING = 0x0400,
        SWP_NOSIZE = 0x0001,
        SWP_NOZORDER = 0x0004,
        SWP_SHOWWINDOW = 0x0040
    }

    [Flags]
    public enum GWL_STYLE : long
    {
        WS_BORDER = 0x00800000L,
        WS_CAPTION = 0x00C00000L,
        WS_CHILD = 0x40000000L,
        WS_CHILDWINDOW = 0x40000000L,
        WS_CLIPCHILDREN = 0x02000000L,
        WS_CLIPSIBLINGS = 0x04000000L,
        WS_DISABLED = 0x08000000L,
        WS_DLGFRAME = 0x00400000L,
        WS_GROUP = 0x00020000L,
        WS_HSCROLL = 0x00100000L,
        WS_ICONIC = 0x20000000L,
        WS_MAXIMIZE = 0x01000000L,
        WS_MAXIMIZEBOX = 0x00010000L,
        WS_MINIMIZE = 0x20000000L,
        WS_MINIMIZEBOX = 0x00020000L,
        WS_OVERLAPPED = 0x00000000L,
        WS_OVERLAPPEDWINDOW = WS_OVERLAPPED | WS_CAPTION | WS_SYSMENU | WS_THICKFRAME | WS_MINIMIZEBOX | WS_MAXIMIZEBOX,
        WS_POPUP = 0x80000000L,
        WS_POPUPWINDOW = WS_POPUP | WS_BORDER | WS_SYSMENU,
        WS_SIZEBOX = 0x00040000L,
        WS_SYSMENU = 0x00080000L,
        WS_TABSTOP = 0x00010000L,
        WS_THICKFRAME = 0x00040000L,
        WS_TILED = 0x00000000L,
        WS_TILEDWINDOW = WS_OVERLAPPED | WS_CAPTION | WS_SYSMENU | WS_THICKFRAME | WS_MINIMIZEBOX | WS_MAXIMIZEBOX,
        WS_VISIBLE = 0x10000000L,
        WS_VSCROLL = 0x00200000L
    }

    [Flags]
    public enum GWL_EXSTYLE : long
    {
        WS_EX_ACCEPTFILES = 0x00000010L,
        WS_EX_APPWINDOW = 0x00040000L,
        WS_EX_CLIENTEDGE = 0x00000200L,
        WS_EX_COMPOSITED = 0x02000000L,
        WS_EX_CONTEXTHELP = 0x00000400L,
        WS_EX_CONTROLPARENT = 0x00010000L,
        WS_EX_DLGMODALFRAME = 0x00000001L,
        WS_EX_LAYERED = 0x00080000,
        WS_EX_LAYOUTRTL = 0x00400000L,
        WS_EX_LEFT = 0x00000000L,
        WS_EX_LEFTSCROLLBAR = 0x00004000L,
        WS_EX_LTRREADING = 0x00000000L,
        WS_EX_MDICHILD = 0x00000040L,
        WS_EX_NOACTIVATE = 0x08000000L,
        WS_EX_NOINHERITLAYOUT = 0x00100000L,
        WS_EX_NOPARENTNOTIFY = 0x00000004L,
        WS_EX_NOREDIRECTIONBITMAP = 0x00200000L,
        WS_EX_OVERLAPPEDWINDOW = WS_EX_WINDOWEDGE | WS_EX_CLIENTEDGE,
        WS_EX_PALETTEWINDOW = WS_EX_WINDOWEDGE | WS_EX_TOOLWINDOW | WS_EX_TOPMOST,
        WS_EX_RIGHT = 0x00001000L,
        WS_EX_RIGHTSCROLLBAR = 0x00000000L,
        WS_EX_RTLREADING = 0x00002000L,
        WS_EX_STATICEDGE = 0x00020000L,
        WS_EX_TOOLWINDOW = 0x00000080L,
        WS_EX_TOPMOST = 0x00000008L,
        WS_EX_TRANSPARENT = 0x00000020L,
        WS_EX_WINDOWEDGE = 0x00000100L
    }

    [Flags]
    public enum SetWindowLongIndex : int
    {
        GWL_EXSTYLE = -20,
        GWL_HINSTANCE = -6,
        GWL_ID = -12,
        GWL_STYLE = -16,
        GWL_USERDATA = -21,
        GWL_WNDPROC = -4
    }

    [Flags]
    public enum KeyModifiers
    {
        None = 0,
        Alt = 1,
        Ctrl = 2,
        Shift = 4,
        WindowsKey = 8
    }

    /// <summary>
    /// 窗口状态枚举
    /// </summary>
    public enum WindowState
    {
        /// <summary>
        /// 无效窗口
        /// </summary>
        Invalid,

        /// <summary>
        /// 正常状态
        /// </summary>
        Normal,

        /// <summary>
        /// 最小化状态
        /// </summary>
        Minimized,

        /// <summary>
        /// 最大化状态
        /// </summary>
        Maximized,

        /// <summary>
        /// 隐藏状态
        /// </summary>
        Hidden
    }

    /// <summary>
    /// 窗口操作枚举
    /// </summary>
    public enum WindowAction
    {
        /// <summary>
        /// 最大化
        /// </summary>
        Maximize,

        /// <summary>
        /// 最小化
        /// </summary>
        Minimize,

        /// <summary>
        /// 还原
        /// </summary>
        Restore,

        /// <summary>
        /// 显示
        /// </summary>
        Show,

        /// <summary>
        /// 隐藏
        /// </summary>
        Hide,

        /// <summary>
        /// 切换状态
        /// </summary>
        Toggle,

        /// <summary>
        /// 激活并置前
        /// </summary>
        Activate
    }

    public enum DataFlow
    {
        Render,
        Capture,
        All
    }

    public enum Role
    {
        Console,
        Multimedia,
        Communications
    }

    public enum DeviceState : uint
    {
        Active = 0x00000001,
        Disabled = 0x00000002,
        NotPresent = 0x00000004,
        Unplugged = 0x00000008,
        All = 0x0000000F
    }

    public enum AudioDeviceType
    {
        Playback,
        Recording
    }

    /// <summary>
    /// 音量操作枚举
    /// </summary>
    public enum VolumeAction
    {
        /// <summary>
        /// 设置音量 (0.0-1.0)
        /// </summary>
        SetVolume,

        /// <summary>
        /// 设置音量百分比 (0-100)
        /// </summary>
        SetVolumePercent,

        /// <summary>
        /// 静音
        /// </summary>
        Mute,

        /// <summary>
        /// 取消静音
        /// </summary>
        Unmute,

        /// <summary>
        /// 切换静音状态
        /// </summary>
        ToggleMute,

        /// <summary>
        /// 音量增加
        /// </summary>
        VolumeUp,

        /// <summary>
        /// 音量减少
        /// </summary>
        VolumeDown
    }
    #endregion

    #region Audio Constants
    public static class AudioConstants
    {
        public static readonly Guid IID_IAudioEndpointVolume = new Guid("5CDF2C82-841E-4546-9722-0CF74078229A");
        public static readonly Guid CLSID_MMDeviceEnumerator = new Guid("BCDE0395-E52F-467C-8E3D-C4579291692E");
        public static readonly Guid IID_IMMDeviceEnumerator = new Guid("A95664D2-9614-4F35-A746-DE8DB63617E6");
        public static readonly Guid GUID_NULL = Guid.Empty;
        public const uint CLSCTX_ALL = 23;
    }
    #endregion
}
