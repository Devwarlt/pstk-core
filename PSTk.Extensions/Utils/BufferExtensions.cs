using System;
using System.IO;
using System.IO.Compression;

namespace PSTk.Extensions.Utils
{
    /// <summary>
    /// Contains <see cref="byte"/> array utilities.
    /// </summary>
    public static class BufferExtensions
    {
        /// <summary>
        /// Compress <paramref name="buffer"/> using <see cref="GZipStream"/> with <see cref="CompressionMode.Compress"/> flag.
        /// </summary>
        /// <param name="buffer"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public static byte[] GZipCompress(this byte[] buffer)
        {
            if (buffer == null)
                throw new ArgumentNullException(nameof(buffer), "Cannot compress a null buffer.");

            using var stream = new MemoryStream();
            using var zip = new GZipStream(stream, CompressionMode.Compress);
            zip.Write(buffer, 0, buffer.Length);
            zip.Close();
            return stream.ToArray();
        }

        /// <summary>
        /// Decompress <paramref name="buffer"/> using <see cref="GZipStream"/> with <see cref="CompressionMode.Decompress"/> flag.
        /// </summary>
        /// <param name="buffer"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public static byte[] GZipDecompress(this byte[] buffer)
        {
            if (buffer == null)
                throw new ArgumentNullException(nameof(buffer), "Cannot decompress a null buffer.");

            using var zipStream = new MemoryStream(buffer);
            using var stream = new MemoryStream();
            using var unzip = new GZipStream(zipStream, CompressionMode.Decompress);
            unzip.CopyTo(stream);
            return stream.ToArray();
        }
    }
}
