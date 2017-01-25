using System;

namespace dotnet
{
    class Program
    {
        static void Main(string[] args)
        {
            Check.SingleWrite();
            Check.MultiWrite();
        }
    }

    // Here until I can be bothered to write some proper tests
    static class Check
    {
        public static void SingleWrite()
        {
            using (var buffer = new Buffer())
            {
                buffer.Write(new byte[] { 0, 1, 2, 3 }.Slice());
            }
        }

        public static void MultiWrite()
        {
            using (var buffer = new Buffer())
            {
                buffer.Write(new byte[] { 0, 1, 2, 3 }.Slice());
                buffer.Write(new byte[] { 4, 5, 6 }.Slice());
            }
        }
    }
}