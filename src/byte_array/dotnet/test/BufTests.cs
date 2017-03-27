using System;
using Xunit;

namespace ByteArray
{
    public class BufTests
    {
        [Fact]
        public void Single_Write_And_Read()
        {
            var data = new byte[] { 0, 1, 2, 3 }.Slice();

            using (var writer = new VecWriter())
            {
                writer.Write(data);

                using (var reader = writer.ToReader())
                {
                    Assert.Equal(data.ToArray(), reader.Slice().ToArray());
                }
            }
        }

        [Fact]
        public void Multi_Write_And_Read()
        {
            var data = new byte[] { 0, 1, 2, 3, 4, 5, 6 }.Slice();

            using (var writer = new VecWriter())
            {
                writer.Write(data.Slice(0, 3));
                writer.Write(data.Slice(3, 4));

                using (var reader = writer.ToReader())
                {
                    Assert.Equal(data.ToArray(), reader.Slice().ToArray());
                }
            }
        }

        [Fact]
        public void Read_Empty()
        {
            using (var reader = new VecReader())
            {
                Assert.True(Span<byte>.Empty == reader.Slice());
            }
        }

        [Fact]
        public void Write_While_Reader_Is_Active()
        {
            using (var writer = new VecWriter())
            {
                using (var reader = writer.ToReader())
                {
                    Assert.Throws<InvalidOperationException>(() => writer.Write(new byte[] { 0 }.Slice()));
                }
            }
        }

        [Fact]
        public void Read_While_Writer_Is_Active()
        {
            using (var reader = new VecReader())
            {
                using (var writer = reader.ToWriter())
                {
                    Assert.Throws<InvalidOperationException>(() => reader.Slice());
                }
            }
        }
    }
}
