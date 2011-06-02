using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;

namespace Pentacorn.Vision
{
    static class Interop
    {
        public static void OpenConsole()
        {
            AttachConsole(ATTACH_PARENT_PROCESS);
        }

        public static void SetLibraryPaths(string rootDir)
        {
            var dirs = Directory.EnumerateDirectories(Global.LibDir, "*", SearchOption.AllDirectories);
            var ok = SetDllDirectory("")
                && SetDllDirectory(Global.ExeDir) 
                && SetDllDirectory(rootDir) 
                && Enumerable.All(dirs, (d) => SetDllDirectory(d));
            if (!ok)
                throw new Exception("Error, unable to set DLL search path.");

            AppDomain.CurrentDomain.AssemblyResolve += new ResolveEventHandler((s, a) =>
            {
                var references = Assembly.GetExecutingAssembly().GetReferencedAssemblies();
                var name = a.Name.Substring(0, a.Name.IndexOf(","));
                var path = Directory.EnumerateFiles(rootDir, name + ".dll", SearchOption.AllDirectories).FirstOrDefault();

                if (path != null)
                    return Assembly.LoadFrom(path);

                throw new Exception("Error, unable to resolve assembly: " + a.Name);
            });
        }

        [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool SetDllDirectory(string lpPathName);

        [DllImport("kernel32.dll")]
        private static extern uint LoadModule(string lpModuleName, IntPtr lpParameterBlock);

        public static void RemoveBorder(IntPtr hwnd)
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

            var rect = new RECT();
            var ok = GetWindowRect(hwnd, out rect)
                  && SetWindowPos(hwnd, 0, rect.X, rect.Y, rect.Width, rect.Height, SWP_FRAMECHANGED);
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

        [DllImport("kernel32.dll")]
        private static extern bool AttachConsole(int dwProcessId);
        private const int ATTACH_PARENT_PROCESS = -1;
    }
}
