using System;
using Xunit;

// ReSharper disable HeapView.ObjectAllocation.Evident
// ReSharper disable HeapView.ClosureAllocation

namespace ByteArray
{
    public class ResizeBufTests
    {
        [Fact]
        public void Convert_To_Reader_Multiple_Times()
        {
            using (var writer = new ResizeBufWriter())
            {
                writer.ToReader();

                Assert.Throws<InvalidOperationException>(() => writer.ToReader());
            }
        }

        [Fact]
        public void Multi_Write_And_Read()
        {
            var data = new Span<byte>(new byte[] {0, 1, 2, 3, 4, 5, 6});

            var writer = new ResizeBufWriter();

            writer.Write(data.Slice(0, 3));
            writer.Write(data.Slice(3, 4));

            var reader = writer.ToReader();

            Assert.Equal(data.ToArray(), reader.Slice().ToArray());
        }

        [Fact]
        public void Read_Empty()
        {
            var reader = new ResizeBufReader();

            Assert.Equal(Span<byte>.Empty.ToArray(), reader.Slice().ToArray());
        }

        [Fact]
        public void Single_Write_And_Read()
        {
            var data = new byte[] {0, 1, 2, 3};

            var writer = new ResizeBufWriter();
            writer.Write(data);

            var reader = writer.ToReader();

            Assert.Equal(data, reader.Slice().ToArray());
        }

        [Fact]
        public void Write_After_Conversion_To_Reader()
        {
            using (var writer = new ResizeBufWriter())
            {
                writer.ToReader();

                Assert.Throws<InvalidOperationException>(() => writer.Write(Span<byte>.Empty));
            }
        }

        [Fact]
        public void Write_After_Dispose()
        {
            var writer = new ResizeBufWriter();
            writer.Dispose();

            Assert.Throws<InvalidOperationException>(() => writer.Write(Span<byte>.Empty));
        }
    }
}