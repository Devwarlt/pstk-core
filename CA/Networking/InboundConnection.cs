using CA.Networking.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;

namespace CA.Networking
{
    /// <summary>
    /// Represents a structure of inbound connection type.
    /// </summary>
    /// <exception cref="InvalidCastException"></exception>
    public struct InboundConnection
    {
        private readonly int bufferSize;
        private readonly ConnectionCentral central;
        private readonly object inboundLocker;
        private readonly int maxConnections;

#pragma warning disable

        public InboundConnection(ConnectionCentral central, int bufferSize)
        {
            this.central = central;
            this.bufferSize = bufferSize;

            inboundLocker = new object();
            maxConnections = central.MaxInboundConnectionsByIp;

            Sockets = new List<Socket>(maxConnections);
        }

#pragma warning restore

        /// <summary>
        /// Get number of connections associated to <see cref="Sockets"/>.
        /// </summary>
        public int GetConnections => Sockets.Count;

        /// <summary>
        /// Returns the <see cref="IPAddress"/> from remote associated <see cref="Sockets"/>.
        /// </summary>
        public IPAddress GetIPAddress
        {
            get
            {
                if (Sockets.Count == 0)
                    throw new InvalidOperationException("There is not IP associated to any socket yet.");

                return Sockets.Where(skt => skt != null).First().GetIpAddress();
            }
        }

        /// <summary>
        /// Get a list of all sockets associated to <see cref="IPAddress"/>.
        /// </summary>
        public List<Socket> Sockets { get; private set; }

#pragma warning disable

        public static bool operator !=(InboundConnection a, InboundConnection b) => !a.Equals(b);

        public static bool operator ==(InboundConnection a, InboundConnection b) => a.Equals(b);

#pragma warning restore

        /// <summary>
        /// Add <see cref="Socket"/> to current inbound connection of current thread.
        /// </summary>
        /// <exception cref="InvalidCastException"></exception>
        /// <param name="socket"></param>
        public void Add(Socket socket)
        {
            lock (inboundLocker)
            {
                if (Sockets.Count == maxConnections)
                    throw new InvalidCastException($"Cannot associate more sockets to current IP {socket.GetIpAddress().ToString()}, limit is set to {maxConnections}.");

                Sockets.Add(socket);
            }
        }

        /// <summary>
        /// Compare two <see cref="InboundConnection"/> by <see cref="IPAddress"/>.
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public override bool Equals(object obj)
        {
            if (!(obj is InboundConnection)) return false;

            var toCompare = (InboundConnection)obj;

            return toCompare.GetIPAddress.Equals(toCompare.GetIPAddress);
        }

#pragma warning disable

        public override int GetHashCode() => GetIPAddress.GetHashCode();

#pragma warning restore

        /// <summary>
        /// Remove <see cref="Socket"/> to current inbound connection of current thread.
        /// </summary>
        /// <exception cref="InvalidCastException"></exception>
        /// <param name="socket"></param>
        public void Remove(Socket socket)
        {
            lock (inboundLocker)
            {
                if (Sockets.Count == 0)
                    throw new InvalidOperationException($"There is no socket to remove to current IP {socket.GetIpAddress().ToString()}.");

                Sockets.Remove(socket);
            }
        }
    }
}
