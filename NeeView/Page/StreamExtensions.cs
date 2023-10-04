using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System;
using System.Diagnostics;

namespace NeeView
{
    public static class StreamExtensions
    {
        public static byte[] ToArray(this Stream stream, int start, int length)
        {
            var array = new byte[length];
            stream.Seek(start, SeekOrigin.Begin);
            var readSize = stream.Read(array.AsSpan());
            Debug.Assert(readSize == length);
            return array;
        }

        public static async Task<byte[]> ToArrayAsync(this Stream stream, int start, int length, CancellationToken token)
        {
            var array = new byte[length];
            stream.Seek(start, SeekOrigin.Begin);
            var readSize = await stream.ReadAsync(array.AsMemory(), token);
            Debug.Assert(readSize == length);
            return array;
        }

        public static ReadOnlySpan<byte> ToSpan(this Stream stream, int start, int length)
        {
            return new ReadOnlySpan<byte>(stream.ToArray(start, length));
        }
    }

    public static class MemoryStreamExtensions
    {
        public static ReadOnlySpan<byte> ToSpan(this MemoryStream stream)
        {
            return new ReadOnlySpan<byte>(stream.GetBuffer(), 0, (int)stream.Length);
        }

    }


}