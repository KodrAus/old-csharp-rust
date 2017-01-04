using System;
using System.Buffers;
using System.Text.Utf8;

class Program
{
    class Pool
    {
        const int _size = 1024;
        NativeBufferPool _pool = new NativeBufferPool(_size);

        public OwnedMemory<byte> Rent()
        {
            return _pool.Rent(_size);
        }

        public void Return(OwnedMemory<byte> buffer)
        {
            _pool.Return(buffer);
        }
    }

    static int Write(ReadOnlySpan<char> utf16Src, Span<byte> dst)
    {
        Utf8Encoder.TryEncode(utf16Src, dst, out int encoded);

        return encoded;
    }

    static int Write(ReadOnlySpan<byte> src, Span<byte> dst)
    {
        src.CopyTo(dst);

        return src.Length;
    }

    static void Main(string[] args)
    {
        var pool = new Pool();
        
        var mem1 = pool.Rent();
        var written1 = Write("Hello, World".Slice(), mem1.Span);
        var slice1 = mem1.Memory.Slice(0, written1);

        var mem2 = pool.Rent();
        var written2 = Write(new byte[] { 0, 1, 2, 3, 4, 5, 6, 7 }.Slice(), mem2.Span);
        var slice2 = mem2.Memory.Slice(0, written2);
    }
}
