using System;
using System.Runtime.InteropServices;

namespace dotnet
{
    class Native
    {
        [DllImport("native", EntryPoint = "alloc")]
        internal static extern Buf Alloc(UIntPtr size);

        [DllImport("native", EntryPoint = "drop")]
        internal static extern void Drop(Buf buf);

        [DllImport("native", EntryPoint = "reserve")]
        internal static extern Buf Reserve(Buf buf, UIntPtr reserve);
    }

    [StructLayout(LayoutKind.Sequential)]
    struct Buf
    {
        public IntPtr ptr;
        public UIntPtr len;
        public IntPtr cap;
    }

    class BufHandle : SafeHandle
    {
        public BufHandle()
            : base(IntPtr.Zero, true)
        { }

        private bool allocated;
        private Buf value;

        private Span<byte> Slice()
        {
            unsafe
            {
                return new Span<byte>(value.ptr.ToPointer(), (int)value.cap).Slice((int)value.len);
            }
        }

        public void Write(Span<byte> data)
        {
            if (IsClosed)
            {
                throw new ObjectDisposedException("Buffer already closed");
            }

            Write(data, false);
        }

        // A method to copy the contents of a buffer to the our native buffer.
        // This isn't optimised, so it's simple and readable.
        private void Write(Span<byte> data, bool resized)
        {
            var dataLen = data.Length;

            if (!allocated)
            {
                // If the buffer isn't allocated then go and do that

                value = Native.Alloc(new UIntPtr((uint)dataLen));
                allocated = true;

                Write(data, true);
            }
            else
            {
                // If the buffer is allocated, make sure it has enough space

                var available = (int)value.cap - (int)value.len;

                if (dataLen > available)
                {
                    // If there isn't enough space, resize and try write again

                    if (resized)
                    {
                        throw new Exception("Already tried to resize buffer");
                    }

                    var needed = dataLen - available;

                    value = Native.Reserve(value, new UIntPtr((uint)needed));

                    Write(data, true);
                }
                else
                {
                    // If the buffer is allocated and there's space, then copy the bits

                    var slice = Slice();

                    data.CopyTo(slice);
                    value.len += dataLen;
                }
            }
        }

        public override bool IsInvalid => false;

        protected override bool ReleaseHandle()
        {
            Native.Drop(value);
            return true;
        }
    }

    public class Buffer : IDisposable
    {
        private BufHandle handle;

        public Buffer()
        {
            handle = new BufHandle();
        }

        public void Write(Span<byte> data)
        {
            handle.Write(data);
        }

        public void Dispose()
        {
            handle.Dispose();
        }
    }
}
