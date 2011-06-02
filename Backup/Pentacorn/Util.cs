using System;
using System.Threading;

namespace Pentacorn
{
    static class Util
    {
        public static void ThreadInfo(string more = "")
        {
            // var cpts = Process.GetCurrentProcess().Threads;
            // var cpt = cpts.Cast<ProcessThread>().ToList()            
            // var deprecatedButUsefulDuringDevelopment = AppDomain.GetCurrentThreadId()
            
            var cmt = Thread.CurrentThread;
            
            Console.WriteLine("Thread[{0}{1}{2}] {3}: {4}",
                cmt.ManagedThreadId,
                cmt.IsBackground ? " bg" : String.Empty,
                cmt.IsThreadPoolThread ? " tpt" : String.Empty,
                cmt.Name, more);
        }

        public static void Swap<T>(ref T lhs, ref T rhs)
        {
            T tmp = lhs;
            lhs = rhs;
            rhs = tmp;
        }

        public static double Clamp(double value, double min, double max)
        {
            if (value < min) return min;
            if (value > max) return max;
            return value;            
        }

        public static bool Equalish(double lhs, double rhs, double epsilon = 0.01)
        {
            return Math.Abs(lhs - rhs) < epsilon;
        }

        public static readonly Random Random = new Random();

        public static void Dispose<T>(ref T t) where T : IDisposable
        {
            if (t != null)
                t.Dispose();
            t = default(T);
        }

        public static T Dispose<T>(T t) where T : IDisposable
        {
            if (t != null)
                t.Dispose();
            return default(T);
        }
    }
}