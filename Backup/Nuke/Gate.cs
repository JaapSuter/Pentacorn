using System.Diagnostics;
using System.Threading;

namespace Pentacorn.Vision
{
    struct Gate
    {
        public static readonly Stopwatch Age = Stopwatch.StartNew();

        public bool Knock()
        {
            if (Current == Closed)
                return false;

            for (;;)
            {
                var previous = Interlocked.CompareExchange(ref Current, Occupied, Open);
                if (previous == Open) return true;
                if (previous == Occupied) Thread.Yield();
                if (previous == Closed) return false;
            }
        }

        public void ThankYou()
        {
            Interlocked.CompareExchange(ref Current, Open, Occupied);
        }

        public void Close()
        {
            for (; Current != Closed; )
            {
                var previous = Interlocked.CompareExchange(ref Current, Closed, Open);
                if (previous == Closed) return;
                if (previous == Open) return;
                if (previous == Occupied) Thread.Yield();                
            }
        }

        private const int Open = 0;
        private const int Occupied = 1;
        private const int Closed = 2;

        private static int Current;        
    }
}
