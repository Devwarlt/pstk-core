using CA.Networking.Utils;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace CA.Networking
{
    /// <summary>
    /// Represents a local instance of server listener, using TCP protocol.
    /// </summary>
    /// <exception cref="ArgumentException"></exception>
    /// <exception cref="ArgumentOutOfRangeException"></exception>
    /// <exception cref="InvalidOperationException"></exception>
    public sealed class ConnectionCentral
    {
        private readonly int bufferSize;
        private readonly TcpListener listener;
        private readonly CancellationTokenSource source;

        private List<InboundConnection> connections;

#pragma warning disable

        public ConnectionCentral(int port, ushort maxConnections, int bufferSize)
            : this(port, maxConnections, bufferSize, ConnectionType.Local)
        {
        }

        public ConnectionCentral(int port, ushort maxConnections, int bufferSize, ConnectionType type)
        {
            if (maxConnections == 0)
                throw new ArgumentException("This service needs to listen for at last 1 inbound connection.", "maxConnections");

            if (!Enum.IsDefined(typeof(ConnectionType), (short)type))
                throw new ArgumentOutOfRangeException("type", $"ConnectionType {(ushort)type} is invalid.");

            this.bufferSize = bufferSize;

            listener = TcpListener.Create(port);
            listener.Server.NoDelay = true;
            listener.Server.UseOnlyOverlappedIO = true;
            listener.Server.Ttl = (short)type;
            source = new CancellationTokenSource();
            connections = new List<InboundConnection>(maxConnections);
        }

#pragma warning restore

        /// <summary>
        /// Get the current <see cref="ConnectionCentralFlag"/> flag.
        /// </summary>
        public ConnectionCentralFlag GetFlag { get; private set; } = ConnectionCentralFlag.Idle;

        /// <summary>
        /// Get the maximum number of <see cref="InboundConnection"/> associated per <see cref="IPAddress"/>.
        /// </summary>
        public int MaxInboundConnectionsByIp { get; private set; }

        /// <summary>
        /// Start listening for new clients.
        /// </summary>
        /// <exception cref="InvalidOperationException"></exception>
        public void Start()
        {
            if (GetFlag == ConnectionCentralFlag.Listening)
                throw new InvalidOperationException("This service can only run in a single thread for inbound connections.");

            GetFlag = ConnectionCentralFlag.Listening;

            Accept();
        }

        /// <summary>
        /// Stop listening for new clients.
        /// </summary>
        public void Stop()
        {
            if (source.IsCancellationRequested || GetFlag == ConnectionCentralFlag.Aborted)
                throw new InvalidOperationException("This server was already stopped.");

            GetFlag = ConnectionCentralFlag.Aborted;
        }

        private void Accept()
        {
            Initialize(async () =>
            {
                var connection = await listener.AcceptTcpClientAsync();
                var ip = connection.Client.GetIpAddress();

                if (ip != null)
                {
                    int? index = connections.FindIndex(conn => conn.Equals(ip));

                    if (!index.HasValue)
                    {
                        var inboundConn = new InboundConnection(this, bufferSize);
                        inboundConn.Add(connection.Client);
                        connections.Add(inboundConn);
                    }
                    else
                        connections[index.Value].Add(connection.Client);
                }
            });

            if (GetFlag != ConnectionCentralFlag.Listening) return;

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
