using System;
using System.IO;
using System.Reflection;

namespace Pentacorn
{
    static class Global
    {
        // Yes and no are deliberately _not_ declared as constants to warnings about unreachable code . I use these global
        // during development when I want to disable or enable a snippet of code temporarily while still having the
        // compiler actually compile it. Commenting it out or using a #define would hide code from the compiler.
        public static readonly bool No = false;
        public static readonly bool Yes = true;

        public static readonly bool DebugThorough = true;

        public static string ExeFileName { get { return Path.GetFileName(Assembly.GetEntryAssembly().Location); } }
        public static string ExeDir { get { return Path.GetDirectoryName(Assembly.GetEntryAssembly().Location); } }
        public static string LibDir { get { return Path.Combine(Global.ExeDir, "Library/x86"); } }
        public static string DatDir { get; private set; }
        public static string TmpDir { get { return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Temp", "Pentacorn"); } }
        public static string AppName { get { return String.Format("{0} {1}", AssemblyName.Name, AssemblyName.Version); } }
        
        public static string TmpFileName(string name, string ext)
        {
            return Path.Combine(TmpDir, String.Format("{0}-{1}.{2}", name, DateTime.Now.ToFileTimeUtc(), ext));
        }

        static Global()
        {
            var home = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            var dropBox = Path.Combine(home, "My Dropbox");
            if (!Directory.Exists(dropBox))
                dropBox = Path.Combine(home, @"My Documents\My Dropbox");
            if (!Directory.Exists(dropBox))
                throw new Exception("Unable to find Dropbox directory");

            DatDir = Path.Combine(dropBox, @"Data\Pentacorn");
            if (!Directory.Exists(DatDir))
                Directory.CreateDirectory(DatDir);
            if (!Directory.Exists(DatDir))
                throw new Exception("Unable to creaate Dropbox sub-directory");

            if (Directory.Exists(TmpDir))
                foreach (var file in Directory.EnumerateFiles(TmpDir, "*"))
                    try { File.Delete(file); }
                    catch (Exception) { }
            else
                Directory.CreateDirectory(TmpDir);
        }

        private static AssemblyName AssemblyName = Assembly.GetEntryAssembly().GetName();
    }
}
