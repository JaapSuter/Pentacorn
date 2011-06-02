using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Pentacorn.Vision
{
    static class Statistics
    {
        public static void Update(string name)
        {
            if (Global.No)
                return;

            if (Gate.Knock())
            {
                if (!Counters.ContainsKey(name))
                    Counters.Add(name, new Counter());

                Gate.ThankYou();
            }
            else
            {
                Counter counter;
                if (Counters.TryGetValue(name, out counter))
                    counter.Update();
            }
        }

        public new static string ToString()
        {
            Update("Statistics.ToString()");

            if (Gate.Age.Elapsed < TimeUntilGateCloses)
                return "Statistics Gate Still Open";

            Gate.Close();
            
            return String.Join("\n", Counters.Select(c => String.Format("{0}: {1}", c.Key, c.Value)))
                 + String.Format("\nThreads: {0}", Process.GetCurrentProcess().Threads.Count);
        }

        private class Counter
        {
            public void Update()
            {
                var currentTicks = Stpwtch.ElapsedTicks;
                History.Enqueue(currentTicks - PreviousTicks);
                PreviousTicks = currentTicks;
            }
            
            public override string ToString()
            {
                const int num = 87;
                if (History.Count >= num)
                {
                    long average = (long)History.Average();
                    long max = History.Max();
                    long min = History.Min();

                    long dummy;
                    while (History.Count > num)
                        History.TryDequeue(out dummy);

                    return String.Format("{0} FPS [{1}/{2}]",
                        TicksPerSecond / average,
                        TicksToMilliseconds(min), TicksToMilliseconds(average), TicksToMilliseconds(max));
                }
                else return "Not enough values yet: " + History.Count;
            }

            private static long TicksToMilliseconds(long ticks)
            {
                return (ticks * 1000) / TicksPerSecond;
            }

            private long PreviousTicks;
            private ConcurrentQueue<long> History = new ConcurrentQueue<long>();
            private Stopwatch Stpwtch = Stopwatch.StartNew();
            private static readonly long TicksPerSecond = Stopwatch.Frequency;
        }

        private static readonly TimeSpan TimeUntilGateCloses = TimeSpan.FromSeconds(0.5);
        private static Gate Gate = new Gate();
        private static Dictionary<string, Counter> Counters = new Dictionary<string, Counter>();        
    }
}
