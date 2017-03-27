/*

A simple reader/writer for a Rust Vec that can be resized.

*/

using System;
using System.Runtime.InteropServices;

namespace ByteArray
{
    internal class Native
    {
        [DllImport("bytearray_native", EntryPoint = "alloc", ExactSpelling = true)]
        internal static extern ResizeBuf Alloc(UIntPtr size);

        [DllImport("bytearray_native", EntryPoint = "drop", ExactSpelling = true)]
        internal static extern void Drop(ResizeBuf buf);

        [DllImport("bytearray_native", EntryPoint = "reserve", ExactSpelling = true)]
        internal static extern ResizeBuf Reserve(ResizeBuf buf, UIntPtr reserve);
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct ResizeBuf
    {
        public IntPtr ptr;
        public UIntPtr len;
        public IntPtr cap;
    }

    // NOTE: This class is not thread-safe and doesn't prevent
    // reads during writes. Use `ResizeBufReader` and `ResizeBufWriter`
    // instead.
    // 
    // If I rethink the ownership strategy though it'll probably do something
    // here.
    internal class ResizeBufHandle : SafeHandle
    {
        public ResizeBufHandle()
            : base(IntPtr.Zero, true)
        { }

        private bool _allocated;
        private ResizeBuf _value;

        public override bool IsInvalid => false;

        // Get a span for the written part of the buf
        public Span<byte> Read()
        {
            if (IsClosed)
            {
                throw new ObjectDisposedException("ResizeBuf already closed");
            }

            unsafe
            {
                return ValueSpan().Slice(0, (int)_value.len);
            }
        }
        
        // Append some bytes to the end of the buf
        // TODO: Support writing into part of the vec
        public void Write(Span<byte> data)
        {
            if (IsClosed)
            {
                throw new ObjectDisposedException("ResizeBuf already closed");
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
                        throw new Exception("Already tried to resize ResizeBuf");
                    }

                    var needed = dataLen - available;

                    _value = Native.Reserve(_value, new UIntPtr((uint)needed));

                    Write(data, true);
                }
                else
                {
                    // If the buffer is allocated and there's space, then copy the bits

                    Span<byte> slice;

                    unsafe 
                    { 
                        slice = ValueSpan().Slice((int)_value.len); 
                    }

                    data.CopyTo(slice);
                    _value.len += dataLen;
                }
            }
        }

        // Get a span for the entire buf
        // This may span uninitialised memory
        private unsafe Span<byte> ValueSpan()
        {
            return new Span<byte>(_value.ptr.ToPointer(), (int)_value.cap);
        }

        protected override bool ReleaseHandle()
        {
            Native.Drop(_value);
            return true;
        }
    }

    // A writer for a Rust Vec.
    public class ResizeBufWriter : IDisposable
    {
        private object _writeLock = new object();
        private ResizeBufHandle _handle;

        internal ResizeBufWriter(ResizeBufHandle handle)
        {
            _handle = handle;
        }

        public ResizeBufWriter() : this(new ResizeBufHandle())
        {
            
        }
        
        public void Write(Span<byte> data)
        {
            lock(_writeLock)
            {
                if (_handle == null)
                {
                    throw new InvalidOperationException("ResizeBuf is not writable");
                }

                _handle.Write(data);
            }
        }

        public ResizeBufReader ToReader()
        {
            lock(_writeLock)
            {
                if (_handle == null)
                {
                    throw new InvalidOperationException("ResizeBuf is not writable");
                }

                var reader = new ResizeBufReader(_handle);
                _handle = null;

                return reader;
            }
        }

        public void Dispose()
        {
            lock(_writeLock)
            {
                if (_handle != null)
                {
                    _handle.Dispose();
                    _handle = null;
                }
            }
        }
    }

    public class ResizeBufReader
    {
        private ResizeBufHandle _handle;

        internal ResizeBufReader(ResizeBufHandle handle)
        {
            _handle = handle;
        }

        public ResizeBufReader() : this(new ResizeBufHandle())
        {
            
        }

        public Span<byte> Slice()
        {
            return _handle.Read();
        }
    }
}
