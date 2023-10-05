using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System;
using System.Diagnostics;

namespace NeeView
{
    public static class StreamExtensions
    {
        /// <summary>
        /// stream to byte array (common)
        /// </summary>
        /// <param name="stream">source stream</param>
        /// <param name="start">copy start stream position</param>
        /// <param name="length">copy length</param>
        /// <returns></returns>
        public static byte[] ToArray(this Stream stream, int start, int length)
        {
            var array = new byte[length];
            stream.Seek(start, SeekOrigin.Begin);
            var readSize = stream.Read(array.AsSpan());
            Debug.Assert(readSize == length);
            return array;
        }

        /// <summary>
        /// stream to byte array (common)
        /// </summary>
        /// <param name="stream">source stream</param>
        /// <param name="start">copy start stream position</param>
        /// <param name="length">copy length</param>
        /// <param name="token">cancellatino token</param>
        /// <returns></returns>
        public static async Task<byte[]> ToArrayAsync(this Stream stream, int start, int length, CancellationToken token)
        {
            var array = new byte[length];
            stream.Seek(start, SeekOrigin.Begin);
            var readSize = await stream.ReadAsync(array.AsMemory(), token);
            Debug.Assert(readSize == length);
            return array;
        }

        /// <summary>
        /// stream to byte span (common)
        /// </summary>
        /// <param name="stream">source stream</param>
        /// <param name="start">copy start stream position</param>
        /// <param name="length">copy length</param>
        /// <returns></returns>
        public static ReadOnlySpan<byte> ToSpan(this Stream stream, int start, int length)
        {
            return new ReadOnlySpan<byte>(stream.ToArray(start, length));
        }
    }

    public static class MemoryStreamExtensions
    {
        /// <summary>
        /// stream to byte span (MemoryStream)
        /// </summary>
        /// <param name="stream">source stream</param>
        /// <returns></returns>
        public static ReadOnlySpan<byte> ToSpan(this MemoryStream stream)
        {
            return new ReadOnlySpan<byte>(stream.GetBuffer(), 0, (int)stream.Length);
        }

    }


}