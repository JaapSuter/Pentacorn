using System;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;

namespace Pentacorn
{
    class History
    {
        public History() { UnsafeUpdate(); }

        public void Update()
        {
            lock (Sync)
                UnsafeUpdate();
        }

        private TimeSpan Age { get { return DateTime.Now - Birth;  } }

        public override string ToString()
        {
            return Accum.ToString();
        }

        private void UnsafeUpdate()
        {
            var trace = new StackTrace(true);
            var begin = 2;
            var end = 8;
            var query = from idx in Enumerable.Range(begin, end)
                        let frame = trace.GetFrame(idx) where frame != null
                        let met = frame.GetMethod() where met != null
                        select String.Format("    {0}.{1}", met.DeclaringType.Name, met.Name);

            Accum.AppendLine(String.Join(Environment.NewLine, query.ToArray().StartWith(String.Format("Age: {0}, Thread {1}[{2}]:", Age, Thread.CurrentThread.ManagedThreadId, Thread.CurrentThread.Name))));
        }


        private Object Sync = new Object();

        private DateTime Birth = DateTime.Now;

        private const int Cap = 512;
        private StringBuilder Accum = new StringBuilder(Cap);
        
        private const int NumFrames = 4;        
    }
}