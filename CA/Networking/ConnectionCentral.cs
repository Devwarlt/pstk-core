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
    /// Represents a local instance of server listener, using TCP protocol, that could be configured for any <see cref="ConnectionType"/>.
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

        public ConnectionCentral(
            int port,
            ushort maxConnections,
            int bufferSize
            ) : this(port, maxConnections, bufferSize, ConnectionType.Local) { }

        public ConnectionCentral(
            int port, ushort maxConnections,
            int bufferSize,
            ConnectionType type,
            Action<string> errorLogger = null
            )
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
            onError += (s, e) => errorLogger?.Invoke(e.ToString());
        }

#pragma warning restore

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
        public void Stop()
        {
            try
            {
                if (source.IsCancellationRequested || GetFlag == ConnectionFlag.Aborted)
                    throw new InvalidOperationException("This server was already stopped.");

                GetFlag = ConnectionFlag.Aborted;
            }
            catch (InvalidOperationException e) { onError.Invoke(null, e); }
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

                        try
                        {
                            inboundConn.Add(connection.Client);
                            inboundConn.AttachToParent(source.Token);
                            connections.Add(inboundConn);
                        }
                        catch (InvalidCastException e) { onError.Invoke(null, e); }
                    }
                    else
                        connections[index.Value].Add(connection.Client);
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
