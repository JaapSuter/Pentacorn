using System;
using System.Linq;
using System.Drawing;
using System.Threading;
using Emgu.CV;
using Emgu.CV.Structure;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Pentacorn
{
    sealed class Picture<TColor, TDepth> : IDisposable
        where TColor : struct, IColor
        where TDepth : new()
    {
        public bool IsDisposed { get { return Buffer == null; } } // Todo, major hack;

        public Size Size { get { return Emgu.Size; } }
        public int Width { get { return Emgu.Width; } }
        public int Height { get { return Emgu.Height; } }

        public byte[] Bytes { get { return Buffer.Bytes; } }
        public IntPtr Ptr { get { return Buffer.Ptr; } }

        public Image<TColor, TDepth> Emgu { get; private set; }

        internal Picture<TColor, TDepth> AddRefs(int n)
        {
            Buffer.AddRefs(n);
            return this;
        }

        internal Picture<TColor, TDepth> AddRef()
        {
            Buffer.AddRefs(1);
            return this;
        }

        public Picture(Size size)
            : this(size.Width, size.Height) { }

        public Picture(int width, int height, IEnumerable<byte> bytes)
            : this(width, height)
        {
            int i = 0;
            bytes.Do(b => Bytes[i++] = b).Run();

        }

        public Picture(int width, int height)
        {
            ID = Interlocked.Increment(ref Count);
            Buffer = new InteropBuffer(width * height * default(TColor).Dimension * Marshal.SizeOf(default(TDepth)));
            Emgu = new Image<TColor, TDepth>(width, height, width * default(TColor).Dimension, Buffer.Ptr);
        }

        public void Dispose()
        {
            if (Buffer == null)
                return;

            if (0 == Buffer.Release())
            {
                Emgu = Util.Dispose(Emgu);
                Buffer = null;
            }
        }

        private int ID;
        private static int Count = 0;
        private InteropBuffer Buffer;
    }
}
