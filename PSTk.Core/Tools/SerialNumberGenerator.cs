using PSTk.Extensions.Utils;
using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace PSTk.Core.Tools
{
    /// <summary>
    /// Used to generate random serial numbers.
    /// </summary>
    public sealed class SerialNumberGenerator
    {
        private readonly int bufferThreshold;
        private readonly char pieceSeparator;
        private readonly int pieceSize;
        private readonly string secret;

        /// <summary>
        /// Create a new instance of <see cref="SerialNumberGenerator"/>.
        /// </summary>
        /// <param name="secret"></param>
        /// <param name="pieceSize"></param>
        /// <param name="pieceSeparator"></param>
        /// <param name="bufferThreshold"></param>
        public SerialNumberGenerator(string secret, int pieceSize = 4, char pieceSeparator = '-', int bufferThreshold = 1000000000)
        {
            this.secret = secret;
            this.pieceSize = pieceSize;
            this.pieceSeparator = pieceSeparator;
            this.bufferThreshold = bufferThreshold;
        }

        /// <summary>
        /// Create a new serial number based on <paramref name="text"/>.
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public string Create(string text)
        {
            using (var sha256 = new SHA256Managed())
            {
                var buffer = Encoding.UTF8.GetBytes(text + secret);
                var hash = sha256.ComputeHash(buffer);
                var encoder = Convert.ToBase64String(hash);
                var byteHash = EightByteHash(encoder);
                var byteHashAbs = Math.Abs(byteHash) * 10f + (byteHash < 0 ? 1d : 0d);
                var key = new StringBuilder();
                var hashStr = byteHashAbs.ToString();
                var pieces = hashStr.ChunkSplit(pieceSize).ToArray();
                for (var i = 0; i < pieces.Length; i++)
                {
                    var piece = ushort.Parse(pieces[i]);
                    key.Append($"{piece:X4}");
                    if (i + 1 < pieces.Length)
                        key.Append(pieceSeparator);
                }
                return key.ToString();
            }
        }

        private int EightByteHash(string text)
        {
            var hash = 0;
            foreach (var b in Encoding.Unicode.GetBytes(text))
            {
                hash += b;
                hash += hash << 0b1010;
                hash ^= hash >> 0b110;
            }

            hash += hash << 0b11;
            hash ^= hash >> 0b1011;
            hash += hash << 0b1111;

            var result = hash % bufferThreshold;
            if (result == 0)
                throw new InternalBufferOverflowException(
                    $"Hash must be less than threshold (hash: {hash}, threshold: {bufferThreshold})!"
                );

            var check = bufferThreshold / result;
            if (check > 1)
                result *= check;

            if (bufferThreshold == result)
                result /= 10;

            return result;
        }
    }
}
