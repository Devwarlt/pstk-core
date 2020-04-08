using CA.Networking.Utils;
using CA.Threading.Tasks.Procedures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace CA.Networking
{
    /// <summary>
    /// Represents a structure of inbound connection type.
    /// </summary>
    /// <exception cref="InvalidCastException"></exception>
    /// <exception cref="InvalidOperationException"></exception>
    /// <exception cref="OperationCanceledException"></exception>
    public struct InboundConnection : IAttachedTask
    {
        private readonly int bufferSize;
        private readonly ConnectionCentral central;
        private readonly object inboundLocker;
        private readonly int maxConnections;

#pragma warning disable

        private CancellationToken token;

        public InboundConnection(
            ConnectionCentral central,
            int bufferSize
            )
        {
            this.central = central;
            this.bufferSize = bufferSize;

            inboundLocker = new object();
            maxConnections = central.MaxInboundConnectionsByIp;
            token = default;

            GetFlag = ConnectionFlag.Idle;
            Sockets = new List<Socket>(maxConnections);
        }

#pragma warning restore

        /// <summary>
        /// Get number of connections associated to <see cref="Sockets"/>.
        /// </summary>
        public int GetConnections => Sockets.Count;

        /// <summary>
        /// Get the current <see cref="ConnectionFlag"/> flag.
        /// </summary>
        public ConnectionFlag GetFlag { get; private set; }

        /// <summary>
        /// Returns the <see cref="IPAddress"/> from remote associated <see cref="Sockets"/>.
        /// </summary>
        public IPAddress GetIPAddress
        {
            get
            {
                if (Sockets.Count == 0)
                    throw new InvalidOperationException("There is no IP associated to any socket yet.");

                return Sockets.Where(skt => skt != null).First().GetIpAddress();
            }
        }

        /// <summary>
        /// Get the <see cref="CancellationToken"/> of attached task.
        /// </summary>
        public CancellationToken GetToken => token;

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
        /// Attach a process to parent in case of external task cancellation request.
        /// </summary>
        /// <param name="token"></param>
        public void AttachToParent(CancellationToken token) => this.token = token;

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
        /// <exception cref="InvalidOperationException"></exception>
        /// <param name="socket"></param>
        public void Remove(Socket socket)
        {
            lock (inboundLocker)
            {
                if (Sockets.Count == 0)
                    throw new InvalidOperationException($"There is no socket to remove from current IP {socket.GetIpAddress().ToString()}.");

                Sockets.Remove(socket);
            }
        }

        /// <summary>
        /// Start listening for inbound traffic.
        /// </summary>
        /// <exception cref="InvalidOperationException"></exception>
        public void Start()
        {
            if (token == default) throw new InvalidOperationException("InboundConnection task is not attached to parent.");

            if (GetFlag == ConnectionFlag.Listening)
                throw new InvalidOperationException("This listener can only run in a single thread for inbound traffic.");

            GetFlag = ConnectionFlag.Listening;

            Initialize(Loop);
        }

        /// <summary>
        /// Stop listening for inbound traffic.
        /// </summary>
        /// <exception cref="InvalidOperationException"></exception>
        public void Stop()
        {
            if (token.IsCancellationRequested || GetFlag == ConnectionFlag.Aborted)
                throw new InvalidOperationException("This listener was already stopped.");

            GetFlag = ConnectionFlag.Aborted;
        }

        private void Initialize(Action method)
        {
            try
            {
                token.ThrowIfCancellationRequested();

                Task.Run(() => method.Invoke(), token);
            }
            catch (OperationCanceledException) { Stop(); }
        }

        private void Loop()
        {
        }
    }
}
