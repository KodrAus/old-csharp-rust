using System;
using Xunit;

namespace ByteArray
{
    public class ResizeBufTests
    {
        [Fact]
        public void Single_Write_And_Read()
        {
            var data = new byte[] { 0, 1, 2, 3 }.Slice();

            var writer = new ResizeBufWriter();
            writer.Write(data);

            var reader = writer.ToReader();

            Assert.Equal(data.ToArray(), reader.Slice().ToArray());
        }

        [Fact]
        public void Multi_Write_And_Read()
        {
            var data = new byte[] { 0, 1, 2, 3, 4, 5, 6 }.Slice();

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
        public void Write_After_Conversion_To_Reader()
        {
            var writer = new ResizeBufWriter();
            var reader = writer.ToReader();

            Assert.Throws<InvalidOperationException>(() => writer.Write(Span<byte>.Empty));
        }
    }
}
