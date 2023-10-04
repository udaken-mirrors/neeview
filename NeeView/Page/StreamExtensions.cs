using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System;

namespace NeeView
{
    public static class StreamExtensions
    {
        public static byte[] ToArray(this Stream stream)
        {
            var array = new byte[stream.Length];
            stream.Seek(0, SeekOrigin.Begin);
            stream.Read(array, 0, (int)stream.Length);
            return array;
        }

        public static async Task<byte[]> ToArrayAsync(this Stream stream, CancellationToken token)
        {
            var array = new byte[stream.Length];
            stream.Seek(0, SeekOrigin.Begin);
            await stream.ReadAsync(array.AsMemory(0, (int)stream.Length), token);
            return array;
        }
    }

}