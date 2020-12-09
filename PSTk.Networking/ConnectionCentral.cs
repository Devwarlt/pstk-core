using PSTk.Networking.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace PSTk.Networking
{
    /// <summary>
    /// Represents a local instance of server listener, using TCP protocol, that could be configured for any <see cref="ConnectionType"/>.
    /// </summary>
    /// <exception cref="ArgumentException"></exception>
    /// <exception cref="ArgumentOutOfRangeException"></exception>
    /// <exception cref="InvalidOperationException"></exception>
    [Obsolete("This feature isn't completed, avoid use it for your projects. No test was made within its development period.", true)]
    public sealed class ConnectionCentral
    {
        private readonly int bufferSize;
        private readonly List<InboundConnection> connections;
        private readonly TcpListener listener;
        private readonly ushort maxPacketsPerEndPoint;
        private readonly CancellationTokenSource source;

#pragma warning disable

        public ConnectionCentral(int port, ushort maxConnections, ushort maxPacketsPerEndPoint, int bufferSize)
            : this(port, maxConnections, maxPacketsPerEndPoint, bufferSize, ConnectionType.Local) { }

        public ConnectionCentral(int port, ushort maxConnections, ushort maxPacketsPerEndPoint, int bufferSize, ConnectionType type, Action<string> errorLogger = null)

#pragma warning restore

        {
            if (maxConnections == 0)
                throw new ArgumentException("This service needs to listen for at least 1 inbound connection.", nameof(maxConnections));

            if (maxPacketsPerEndPoint == 0)
                throw new ArgumentException("This service needs to handle at least 1 inbound packet per endpoint.", nameof(maxPacketsPerEndPoint));

            if (!Enum.IsDefined(typeof(ConnectionType), (short)type))
                throw new ArgumentOutOfRangeException(nameof(type), $"ConnectionType {(ushort)type} is invalid.");

            if (bufferSize <= 0) throw new ArgumentOutOfRangeException(nameof(bufferSize), "Only non-zero and non-negative values are permitted.");

            this.maxPacketsPerEndPoint = maxPacketsPerEndPoint;
            this.bufferSize = bufferSize;

            listener = TcpListener.Create(port);
            listener.Server.NoDelay = true;
            listener.Server.UseOnlyOverlappedIO = true;
            listener.Server.Ttl = (short)type;
            source = new CancellationTokenSource();
            connections = new List<InboundConnection>(maxConnections);
            onError += (s, e) => errorLogger?.Invoke(e.ToString());
        }

        private event EventHandler<Exception> onError;

        /// <summary>
        /// Get the current <see cref="ConnectionFlag"/> flag.
        /// </summary>
        public ConnectionFlag GetFlag { get; private set; } = ConnectionFlag.Idle;

        /// <summary>
        /// Get the maximum number of <see cref="InboundConnection"/> associated per <see cref="IPAddress"/>.
        /// </summary>
        public int MaxInboundConnectionsByIp { get; private set; }

        /// <summary>
        /// Gets a matrix of all <see cref="InboundPacket"/> from all <see cref="InboundConnection"/>.
        /// </summary>
        /// <returns></returns>
        public InboundPacket[][] GetAllInboundPackets()
        {
            var matrix = new List<InboundPacket[]>();
            var conns = connections.ToArray();

            for (var i = 0; i < conns.Length; i++)
            {
                var packets = conns[i].GetEndPointPackets().ToArray();

                if (packets.Length == 0) continue;

                matrix.Add(packets);
            }

            return matrix.ToArray();
        }

        /// <summary>
        /// Start listening for new clients.
        /// </summary>
        /// <exception cref="InvalidOperationException"></exception>
        public void Start()
        {
            try
            {
                if (GetFlag == ConnectionFlag.Listening)
                    throw new InvalidOperationException("This service can only run in a single thread for inbound connections.");

                GetFlag = ConnectionFlag.Listening;

                Accept();
            }
            catch (InvalidOperationException e) { onError.Invoke(null, e); }
        }

        /// <summary>
        /// Stop listening for new clients.
        /// </summary>
        /// <exception cref="InvalidOperationException"></exception>
        public void Stop()
        {
            try
            {
                if (source.IsCancellationRequested || GetFlag == ConnectionFlag.Aborted)
                    throw new InvalidOperationException("This server was already stopped.");

                GetFlag = ConnectionFlag.Aborted;

                var conns = connections.ToArray();

                for (var i = 0; i < conns.Length; i++)
                    try { conns[i].Stop(); }
                    catch (InvalidOperationException) { }
            }
            catch (InvalidOperationException e) { onError.Invoke(null, e); }
        }

        private void Accept()
        {
            Initialize(async () =>
            {
                var conn = await listener.AcceptTcpClientAsync();
                var ip = conn.Client.GetIpAddress();

                if (ip != null)
                {
                    var currConn = connections.FirstOrDefault(connection => connection.Equals(ip));

                    if (currConn == default)
                    {
                        var inboundConn = new InboundConnection(this, bufferSize, maxPacketsPerEndPoint);

                        try
                        {
                            inboundConn.Add(conn);
                            inboundConn.AttachToParent(source.Token);
                            connections.Add(inboundConn);
                        }
                        catch (InvalidCastException e) { onError.Invoke(null, e); }
                    }
                    else
                        currConn.Add(conn);
                }
            });

            if (GetFlag != ConnectionFlag.Listening) return;

            Accept();
        }

        private void Initialize(Action method)
        {
            try
            {
                source.Token.ThrowIfCancellationRequested();

                Task.Run(() => method.Invoke(), source.Token);
            }
            catch (OperationCanceledException) { Stop(); }
        }
    }
}
