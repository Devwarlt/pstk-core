using System.IO;
using System.Net.Sockets;

namespace PSTk.Extensions.Utils
{
    /// <summary>
    /// Contains <see cref="NetworkStream"/> utilities.
    /// </summary>
    public static class NetworkStreamExtensions
    {
        /// <summary>
        /// Helper function to read exactly <paramref name="amount"/> of bytes. This is thread blocking and releases until <paramref name="amount"/>
        /// from <paramref name="buffer"/> was exactly read. Immediately returns false if <paramref name="stream"/> when <see cref="Socket"/>
        /// disconnects.
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="buffer"></param>
        /// <param name="amount"></param>
        /// <returns></returns>
        public static bool ReadExactly(this NetworkStream stream, byte[] buffer, int amount)
        {
            var bytesRead = 0;
            while (bytesRead < amount)
            {
                var remaining = amount - bytesRead;
                var result = stream.ReadSafely(buffer, bytesRead, remaining);
                if (result == 0)
                    return false;

                bytesRead += result;
            }

            return true;
        }

        /// <summary>
        /// Returns 0 if remote closed the connection.
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="buffer"></param>
        /// <param name="offset"></param>
        /// <param name="size"></param>
        /// <returns></returns>
        public static int ReadSafely(this NetworkStream stream, byte[] buffer, int offset, int size)
        {
            try { return stream.Read(buffer, offset, size); }
            catch (IOException) { return 0; }
        }
    }
}
