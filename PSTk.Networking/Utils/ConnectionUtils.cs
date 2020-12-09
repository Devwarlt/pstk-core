using System;
using System.Net;
using System.Net.Sockets;

namespace PSTk.Networking.Utils
{
    /// <summary>
    /// A collection of connection utils for <see cref="ConnectionCentral"/>.
    /// </summary>
    [Obsolete]
    public static class ConnectionUtils
    {
        /// <summary>
        /// Returns the <see cref="IPAddress"/> from remote <see cref="Socket"/>.
        /// </summary>
        /// <param name="socket"></param>
        /// <returns></returns>
        public static IPAddress GetIpAddress(this Socket socket)
        {
            try
            {
                var endPoint = socket.RemoteEndPoint;
                return ((IPEndPoint)endPoint).Address;
            }
            catch { return null; }
        }
    }
}
