using System;
using System.Runtime.InteropServices;

namespace ByteArray
{
    internal class Native
    {
        [DllImport("bytearray_native", EntryPoint = "alloc", ExactSpelling = true)]
        internal static extern Vec Alloc(UIntPtr size);

        [DllImport("bytearray_native", EntryPoint = "drop", ExactSpelling = true)]
        internal static extern void Drop(Vec buf);

        [DllImport("bytearray_native", EntryPoint = "reserve", ExactSpelling = true)]
        internal static extern Vec Reserve(Vec buf, UIntPtr reserve);
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct Vec
    {
        public IntPtr ptr;
        public UIntPtr len;
        public IntPtr cap;
    }

    internal class VecHandle : SafeHandle
    {
        public VecHandle()
            : base(IntPtr.Zero, true)
        { }

        private bool _allocated;
        private Vec _value;

        private Span<byte> ValueSpan()
        {
            unsafe
            {
                return new Span<byte>(_value.ptr.ToPointer(), (int)_value.cap);
            }
        }

        public Span<byte> Read()
        {
            return ValueSpan().Slice(0, (int)_value.len);
        }
        
        public void Write(Span<byte> data)
        {
            if (IsClosed)
            {
                throw new ObjectDisposedException("Vec already closed");
            }

            Write(data, false);
        }

        // A method to copy the contents of a buffer to the our native buffer.
        // This isn't optimised, so it's simple and readable.
        private void Write(Span<byte> data, bool resized)
        {
            var dataLen = data.Length;

            if (!_allocated)
            {
                // If the buffer isn't allocated then go and do that

                _value = Native.Alloc(new UIntPtr((uint)dataLen));
                _allocated = true;

                Write(data, true);
            }
            else
            {
                // If the buffer is allocated, make sure it has enough space

                var available = (int)_value.cap - (int)_value.len;

                if (dataLen > available)
                {
                    // If there isn't enough space, resize and try write again

                    if (resized)
                    {
                        throw new Exception("Already tried to resize Vec");
                    }

                    var needed = dataLen - available;

                    _value = Native.Reserve(_value, new UIntPtr((uint)needed));

                    Write(data, true);
                }
                else
                {
                    // If the buffer is allocated and there's space, then copy the bits

                    var slice = ValueSpan().Slice((int)_value.len);

                    data.CopyTo(slice);
                    _value.len += dataLen;
                }
            }
        }

        public override bool IsInvalid => false;

        protected override bool ReleaseHandle()
        {
            Native.Drop(_value);
            return true;
        }
    }

    public class VecWriter : IDisposable
    {
        private VecHandle _handle;

        internal VecWriter(VecHandle handle)
        {
            _handle = handle;
        }

        public VecWriter() : this(new VecHandle())
        {
            
        }
        
        public void Write(Span<byte> data)
        {
            if (_handle == null)
            {
                throw new InvalidOperationException("Vec is not writable");
            }

            _handle.Write(data);
        }

        public VecReader ToReader()
        {
            var reader = new VecReader(_handle);
            _handle = null;

            return reader;
        }

        public void Dispose()
        {
            _handle?.Dispose();
        }
    }

    public class VecReader : IDisposable
    {
        private VecHandle _handle;

        internal VecReader(VecHandle handle)
        {
            _handle = handle;
        }

        public VecReader() : this(new VecHandle())
        {
            
        }

        public Span<byte> Slice()
        {
            if (_handle == null)
            {
                throw new InvalidOperationException("Vec is not readable");
            }

            return _handle.Read();
        }

        public VecWriter ToWriter()
        {
            var writer = new VecWriter(_handle);
            _handle = null;

            return writer;
        }

        public void Dispose()
        {
            _handle?.Dispose();
        }
    }
}
