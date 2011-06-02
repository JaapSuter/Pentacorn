using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;

namespace Pentacorn
{
    sealed class InteropBuffer
    {
        public byte[] Bytes { get { return RcBytes.Bytes; } }
        public IntPtr Ptr { get; private set; }

        public InteropBuffer(int size)
        {
            var MaxNumAllocated = 823;
            if (OutAndAbout.Count > MaxNumAllocated)
                throw new Exception("Too many interop buffers around, are some leaking?");

            var pool = SizedFreeList.GetOrAdd(size, (_) => new Bag());
            if (!pool.TryTake(out RcBytes))
                RcBytes = new RefCountedByteArray(size);

            Debug.Assert(!OutAndAbout.ContainsKey(RcBytes.Id));
            Debug.Assert(1 == RcBytes.RefCount);
            Debug.Assert(size == RcBytes.Bytes.Length);

            GCHandle = GCHandle.Alloc(RcBytes.Bytes, GCHandleType.Pinned);
            Ptr = GCHandle.AddrOfPinnedObject();

            var maxNumTries = 3;
            var actualTries = Enumerable.Range(1, maxNumTries).FirstOrDefault(i => OutAndAbout.TryAdd(RcBytes.Id, RcBytes));
            Debug.Assert(maxNumTries != actualTries && maxNumTries != default(int));
        }

        public int Release()
        {
            if (RcBytes == null)
                return 0; // Todo

            Debug.Assert(OutAndAbout.ContainsKey(RcBytes.Id));
            Debug.Assert(0 < RcBytes.RefCount);

            var rc = Interlocked.Decrement(ref RcBytes.RefCount);

            Debug.Assert(0 <= RcBytes.RefCount);
            Debug.Assert(0 <= rc);
            
            if (0 == rc)
            {
                var rcBytesOther = RcBytes;
                
                // Shouldn't have to be interlocked, but right now I'm double thorough.
                var rcBytes = Interlocked.CompareExchange(ref RcBytes, null, RcBytes);
                Debug.Assert(rcBytes == rcBytesOther);

                Ptr = IntPtr.Zero;
                GCHandle.Free();
                GCHandle = default(GCHandle);
                
                var maxNumTries = 3;
                var actualTries = Enumerable.Range(1, maxNumTries).FirstOrDefault(i => OutAndAbout.TryRemove(rcBytes.Id, out rcBytesOther));
                Debug.Assert(maxNumTries != actualTries && maxNumTries != default(int));

                Debug.Assert(rcBytes == rcBytesOther);

                rcBytes.RefCount = 1;
                SizedFreeList[rcBytes.Bytes.Length].Add(rcBytes);
            }

            return rc;
        }

        internal int RefCountImpl { get { return RcBytes.RefCount;  } }

        public void AddRefs(int n)
        {
            Debug.Assert(OutAndAbout.ContainsKey(RcBytes.Id));
            Debug.Assert(0 < RcBytes.RefCount);

            var rc = Interlocked.Add(ref RcBytes.RefCount, n);

            Debug.Assert(0 <= RcBytes.RefCount);
            Debug.Assert(0 <= rc);
        }

        private GCHandle GCHandle = new GCHandle();

        private class RefCountedByteArray
        {
            public RefCountedByteArray(int size)
            {
                Id = Interlocked.Increment(ref IdProvider);
                Bytes = new byte[size];
                RefCount = 1;
            }

            private static int IdProvider = -1;

            public int Id;
            public int RefCount;
            public byte [] Bytes;
        }

        private RefCountedByteArray RcBytes;

        private class Bag : ConcurrentBag<RefCountedByteArray> {}
        private class Dict : ConcurrentDictionary<int, Bag> { }
        private class Oaa : ConcurrentDictionary<int, RefCountedByteArray> { }

        private static Oaa OutAndAbout = new Oaa();
        private static Dict SizedFreeList = new Dict();        
    }
}
