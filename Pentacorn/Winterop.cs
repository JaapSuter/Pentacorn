using System;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using Microsoft.Win32;
using System.Drawing;

namespace Pentacorn
{
    static class Winterop
    {
        public static void SetLibraryPaths(string rootDir)
        {
            var todoRequiresSillyExplicitLoad = Path.Combine(rootDir, @"Misc\bpmDetect.dll");
            var ret = LoadLibrary(todoRequiresSillyExplicitLoad);
            if (ret == IntPtr.Zero)
                throw new Exception("Unable to load: " + todoRequiresSillyExplicitLoad);

            var dirs = Directory.EnumerateDirectories(Global.LibDir, "*", SearchOption.AllDirectories);
            var ok = SetDllDirectory("")
                && SetDllDirectory(Global.ExeDir)
                && SetDllDirectory(rootDir)
                && Enumerable.All(dirs, SetDllDirectory);
            if (!ok)
                throw new Exception("Error, unable to set DLL search path.");

            AppDomain.CurrentDomain.AssemblyResolve += new ResolveEventHandler((s, a) =>
            {
                var references = Assembly.GetExecutingAssembly().GetReferencedAssemblies();
                var name = a.Name.Substring(0, a.Name.IndexOf(","));
                var path = Directory.EnumerateFiles(rootDir, name + ".dll", SearchOption.AllDirectories).FirstOrDefault();

                var useLoadFromContext = true;
                if (path != null)
                    return useLoadFromContext ? Assembly.LoadFrom(path) : Assembly.Load(File.ReadAllBytes(path));

                throw new Exception("Error, unable to resolve assembly: " + a.Name);
            });
        }

        [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool SetDllDirectory(string lpPathName);

        [DllImport("kernel32.dll")]
        public static extern IntPtr LoadLibrary(string lpFileName);

        public static void RemoveBorder(IntPtr hwnd, System.Drawing.Rectangle rectangle)
        {
            const int SWP_FRAMECHANGED = 0x0020;
            const int GWL_STYLE = -16;
            const int GWL_EXSTYLE = -20;
            const int WS_SYSMENU = 524288;
            const int WS_THICKFRAME = 262144;
            const int WS_MINIMIZE = 536870912;
            const int WS_MAXIMIZE = 16777216;
            const int WS_BORDER = 8388608;
            const int WS_DLGFRAME = 4194304;
            const int WS_CAPTION = WS_BORDER | WS_DLGFRAME;
            const int WS_EX_DLGMODALFRAME = 1;
            const int WS_EX_CLIENTEDGE = 512;
            const int WS_EX_STATICEDGE = 131072;

            var style = GetWindowLong(hwnd, GWL_STYLE) & ~(WS_CAPTION | WS_THICKFRAME | WS_MINIMIZE | WS_MAXIMIZE | WS_SYSMENU);
            SetWindowLong(hwnd, GWL_STYLE, style);

            var exst = GetWindowLong(hwnd, GWL_EXSTYLE) & ~(WS_EX_DLGMODALFRAME | WS_EX_CLIENTEDGE | WS_EX_STATICEDGE);
            SetWindowLong(hwnd, GWL_EXSTYLE, exst);

            var ok = SetWindowPos(hwnd, 0, rectangle.X, rectangle.Y, rectangle.Width, rectangle.Height, SWP_FRAMECHANGED);
            if (!ok)
                throw new Exception("Error, can't remove border apparently.");
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct RECT
        {
            public int X;
            public int Y;
            public int Width;
            public int Height;
        }

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool SetWindowPos(IntPtr hwnd, int level, int x, int y, int w, int h, int flags);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool GetWindowRect(IntPtr hwnd, out RECT lpRect);

        [DllImport("user32.dll")]
        private static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern int GetWindowLong(IntPtr hWnd, int nIndex);

        public static bool NoPendingWindowMessages()
        {
            Message message;
            return !PeekMessage(out message, IntPtr.Zero, 0, 0, 0);
        }

        public const uint PM_REMOVE = 0x1;
        public const uint PM_NOYIELD = 0x2;
        public const uint WM_QUIT = 0x12;
        public const uint WM_PAINT = 0x000F;

        [StructLayout(LayoutKind.Sequential)]
        public struct Message
        {
            // Do not confuse this structure with the one System.Windows.Forms provides, because the latter
            // is not meant to be usable as a Win32 POD (in fact, the field order is different.)
            public IntPtr hWnd;
            public uint Msg;
            public IntPtr WParam;
            public IntPtr LParam;
            public uint Time;
            public Point Point;
        }

        [DllImport("User32.dll")]
        public static extern void PostQuitMessage(int nExitCode);

        [DllImport("User32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool PeekMessage(out Message message, IntPtr hWnd, uint filterMin, uint filterMax, uint flags);

        [DllImport("User32.dll")]
        public static extern IntPtr DispatchMessage([In] ref Message message);

        [DllImport("User32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool TranslateMessage([In] ref Message message);

        public static IntPtr CreateDummyWindowHandle()
        {
            // Based on MSDN sample code
            // found at http://msdn.microsoft.com/en-us/library/cc656710.aspx

            const int CS_HREDRAW = 1;
            const int CS_VREDRAW = 2;

            WNDCLASS wc = new WNDCLASS()
            {
                style = CS_HREDRAW | CS_VREDRAW,
                lpfnWndProc = DefWindowProcKeepAlive,
                cbClsExtra = 0,
                cbWndExtra = 0,
                hInstance = IntPtr.Zero,
                hIcon = IntPtr.Zero,
                hCursor = IntPtr.Zero,
                hbrBackground = IntPtr.Zero,
                lpszMenuName = null,
                lpszClassName = Global.AppName,
            };

            var atom = RegisterClassW(ref wc);
            if (0 == atom)
                throw new Win32Exception("Unable to RegisterClassW for dummy window.");

            var hwnd = CreateWindowEx(0, (uint)atom, Global.AppName, WindowStyles.WS_OVERLAPPEDWINDOW,
                0, 0, 0, 0, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero);

            if (hwnd == IntPtr.Zero)
                throw new Win32Exception("Unable to CreateWindowExW for dummy window.");

            return hwnd;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct WNDCLASS
        {
            public uint style;
            public WndProc lpfnWndProc;
            public int cbClsExtra;
            public int cbWndExtra;
            public IntPtr hInstance;
            public IntPtr hIcon;
            public IntPtr hCursor;
            public IntPtr hbrBackground;
            [MarshalAs(UnmanagedType.LPWStr)]
            public string lpszMenuName;
            [MarshalAs(UnmanagedType.LPWStr)]
            public string lpszClassName;
        }

        [DllImport("user32.dll")]
        static extern IntPtr DefWindowProc(IntPtr hWnd, uint uMsg, IntPtr wParam, IntPtr lParam);
        private delegate IntPtr WndProc(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam);
        private static WndProc DefWindowProcKeepAlive = DefWindowProc;

        [DllImport("user32.dll", SetLastError = true)]
        private static extern ushort RegisterClassW([In] ref WNDCLASS lpWndClass);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern IntPtr CreateWindowEx(UInt32 dwExStyle,
           uint lpClassNameByAtomFromRegisterWindow,
           [MarshalAs(UnmanagedType.LPWStr)] string lpWindowName,
           WindowStyles dwStyle,
           Int32 x,
           Int32 y,
           Int32 nWidth,
           Int32 nHeight,
           IntPtr hWndParent,
           IntPtr hMenu,
           IntPtr hInstance,
           IntPtr lpParam
        );

        [Flags]
        private enum WindowStyles : uint
        {
            WS_OVERLAPPED = 0x00000000,
            WS_POPUP = 0x80000000,
            WS_CHILD = 0x40000000,
            WS_MINIMIZE = 0x20000000,
            WS_VISIBLE = 0x10000000,
            WS_DISABLED = 0x08000000,
            WS_CLIPSIBLINGS = 0x04000000,
            WS_CLIPCHILDREN = 0x02000000,
            WS_MAXIMIZE = 0x01000000,
            WS_BORDER = 0x00800000,
            WS_DLGFRAME = 0x00400000,
            WS_VSCROLL = 0x00200000,
            WS_HSCROLL = 0x00100000,
            WS_SYSMENU = 0x00080000,
            WS_THICKFRAME = 0x00040000,
            WS_GROUP = 0x00020000,
            WS_TABSTOP = 0x00010000,

            WS_MINIMIZEBOX = 0x00020000,
            WS_MAXIMIZEBOX = 0x00010000,

            WS_CAPTION = WS_BORDER | WS_DLGFRAME,
            WS_TILED = WS_OVERLAPPED,
            WS_ICONIC = WS_MINIMIZE,
            WS_SIZEBOX = WS_THICKFRAME,
            WS_TILEDWINDOW = WS_OVERLAPPEDWINDOW,

            WS_OVERLAPPEDWINDOW = WS_OVERLAPPED | WS_CAPTION | WS_SYSMENU | WS_THICKFRAME | WS_MINIMIZEBOX | WS_MAXIMIZEBOX,
        }

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool IsGUIThread(bool bConvert);

        public static void MoveBroadCastEventWindowToThisThread()
        {
            var dummy = 0;
            var nop = new Action(() => { var d = dummy; dummy = d; });

            // Based on Stack Overflow which linked to this
            // forum post: http://social.msdn.microsoft.com/Forums/en-US/netfxbcl/thread/fb267827-1765-4bd9-ae2f-0abbd5a2ae22
            if (IsGUIThread(false))
                SystemEvents.InvokeOnEventsThread(nop);
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        private struct UNSIGNED_RATIO
        {
            public UInt32 uiNumerator;
            public UInt32 uiDenominator;
        };

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        private struct DwmTimingInfo
        {
            public UInt32 cbSize;
            public UNSIGNED_RATIO rateRefresh;
            public UInt64 qpcRefreshPeriod;
            public UNSIGNED_RATIO rateCompose;
            public UInt64 qpcVBlank;
            public UInt64 cRefresh;
            public UInt32 cDXRefresh;
            public UInt64 qpcCompose;
            public UInt64 cFrame;
            public UInt32 cDXPresent;
            public UInt64 cRefreshFrame;
            public UInt64 cFrameSubmitted;
            public UInt32 cDXPresentSubmitted;
            public UInt64 cFrameConfirmed;
            public UInt32 cDXPresentConfirmed;
            public UInt64 cRefreshConfirmed;
            public UInt32 cDXRefreshConfirmed;
            public UInt64 cFramesLate;
            public UInt32 cFramesOutstanding;

            public UInt64 cFrameDisplayed;
            public UInt64 qpcFrameDisplayed;
            public UInt64 cRefreshFrameDisplayed;
            public UInt64 cFrameComplete;
            public UInt64 qpcFrameComplete;

            public UInt64 cFramePending;
            public UInt64 qpcFramePending;
            public UInt64 cFramesDisplayed;
            public UInt64 cFramesComplete;
            public UInt64 cFramesPending;
            public UInt64 cFramesAvailable;
            public UInt64 cFramesDropped;
            public UInt64 cFramesMissed;
            public UInt64 cRefreshNextDisplayed;
            public UInt64 cRefreshNextPresented;
            public UInt64 cRefreshesDisplayed;
            public UInt64 cRefreshesPresented;
            public UInt64 cRefreshStarted;

            public UInt64 cPixelsReceived;
            public UInt64 cPixelsDrawn;
            public UInt64 cBuffersEmpty;
        };

        [DllImport("Dwmapi.dll")]
        private static extern int DwmGetCompositionTimingInfo(IntPtr hwnd, ref DwmTimingInfo pTimingInfo);

        public static string DwmCompositionTimingInfo()
        {
            var dwmTimingInfo = new DwmTimingInfo() { cbSize = 73 * 4 };
            var error = DwmGetCompositionTimingInfo(IntPtr.Zero, ref dwmTimingInfo);

            return String.Format("{0:X} DWM refresh={1}, DX refresh={2} {3} Hz",
                    error,
                    dwmTimingInfo.cRefresh / dwmTimingInfo.qpcRefreshPeriod,
                    dwmTimingInfo.cDXRefresh / dwmTimingInfo.qpcRefreshPeriod,
                    dwmTimingInfo.rateRefresh.uiNumerator);
        }
    }
}
