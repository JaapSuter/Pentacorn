using System;
using System.Collections.Concurrent;
using System.Runtime.InteropServices;
using System.Threading;

namespace Pentacorn.Vision
{
    abstract class InteropBuffer : IDisposable
    {
        public byte[] Bytes { get { return bytes; } }
        public IntPtr Ptr { get { return ptr; } }
        public int Length { get { return bytes.Length; } }

        public InteropBuffer(int size)
        {
            if (num >= max)
                throw new Exception("Interop buffers are leaking...");

            var pool = pools.GetOrAdd(size, (_) => new ConcurrentBag<byte[]>());
            if (!pool.TryTake(out bytes))
                bytes = new byte[size];

            Interlocked.Increment(ref num);
            gcHandle = GCHandle.Alloc(bytes, GCHandleType.Pinned);
            ptr = gcHandle.AddrOfPinnedObject();
            refCount = 1;
        }

        public void Dispose()
        {
            GC.SuppressFinalize(this);
            Dispose(true);
        }

        protected virtual void Dispose(bool disposing)
        {
            Release();
        }

        public void Release()
        {
            if (0 < Interlocked.Decrement(ref refCount))
                return;

            var steal = Interlocked.Exchange(ref bytes, null);
            if (steal != null)
            {
                Interlocked.Decrement(ref num);
                ptr = IntPtr.Zero;
                gcHandle.Free();
                pools[steal.Length].Add(steal);
            }
        }

        public int AddRef()
        {
            return Interlocked.Increment(ref refCount);
        }

        private int refCount;
        private IntPtr ptr;
        private byte[] bytes;
        private GCHandle gcHandle = new GCHandle();

        private const int max = 20;
        public static int num;
        private static ConcurrentDictionary<int, ConcurrentBag<byte[]>> pools = new ConcurrentDictionary<int, ConcurrentBag<byte[]>>();
    }
}
