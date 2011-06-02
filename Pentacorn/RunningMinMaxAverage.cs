using System;
using System.Linq;
using Pentacorn.Tasks;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace Pentacorn
{
    class RunningMinMaxAverage
    {
        private readonly Queue<long> Samples;
        private readonly int Capacity;
        private long Sum;

        public float Average { get { return (float)Sum / (float)Samples.Count; } }
        public long Min { get; private set; }
        public long Max { get; private set; }
        
        public RunningMinMaxAverage(int numSamples)
        {
            Capacity = numSamples;
            Min = long.MaxValue;
            Max = 0;
            Samples = new Queue<long>(numSamples);
        }

        public void Add(long val)
        {
            long deq = 0;
            if (Samples.Count == Capacity)
            {
                deq = Samples.Dequeue();
                Sum -= deq;                
            }

            Samples.Enqueue(val);
            Sum += val;
            
            if (Min == deq) Min = Samples.Min(); else Min = Math.Min(Min, val);
            if (Max == deq) Max = Samples.Max(); else Max = Math.Max(Max, val);
        }
    }
}
