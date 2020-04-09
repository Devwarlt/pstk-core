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
    /// Represents an inbound connection type.
    /// </summary>
    /// <exception cref="InvalidCastException"></exception>
    /// <exception cref="InvalidOperationException"></exception>
    /// <exception cref="OperationCanceledException"></exception>
    /// <exception cref="SocketException"></exception>
    public class InboundConnection : IAttachedTask
    {
        private readonly int bufferSize;
        private readonly ConnectionCentral central;
        private readonly object inboundLocker;
        private readonly int maxConnections;
        private readonly ushort maxPacketsPerEndPoint;

        private List<InboundTraffic> inboundTraffics;

#pragma warning disable

        private CancellationToken token;

        public InboundConnection(
            ConnectionCentral central,
            int bufferSize,
            ushort maxPacketsPerEndPoint
            )
        {
            this.central = central;
            this.bufferSize = bufferSize;
            this.maxPacketsPerEndPoint = maxPacketsPerEndPoint;

            inboundLocker = new object();
            maxConnections = central.MaxInboundConnectionsByIp;
            token = default;
            inboundTraffics = new List<InboundTraffic>();

            GetFlag = ConnectionFlag.Idle;
            Clients = new List<TcpClient>();

            onAdd = null;
        }

#pragma warning restore

        private event EventHandler<TcpClient> onAdd;

        /// <summary>
        /// Get a list of all sockets associated to <see cref="IPAddress"/>.
        /// </summary>
        public List<TcpClient> Clients { get; private set; }

        /// <summary>
        /// Get number of connections associated to <see cref="Clients"/>.
        /// </summary>
        public int GetConnections { get { lock (inboundLocker) return Clients.Count; } }

        /// <summary>
        /// Get the current <see cref="ConnectionFlag"/> flag.
        /// </summary>
        public ConnectionFlag GetFlag { get; private set; }

        /// <summary>
        /// Returns the <see cref="IPAddress"/> from remote associated <see cref="Clients"/>.
        /// </summary>
        public IPAddress GetIPAddress
        {
            get
            {
                lock (inboundLocker)
                    if (Clients.Count == 0)
                        throw new InvalidOperationException("There is no IP associated to any socket yet.");

                return Clients.Where(skt => skt != null).First().Client.GetIpAddress();
            }
        }

        /// <summary>
        /// Get the <see cref="CancellationToken"/> of attached task.
        /// </summary>
        public CancellationToken GetToken => token;

#pragma warning disable

        public static bool operator !=(InboundConnection a, InboundConnection b) => !a.Equals(b);

        public static bool operator ==(InboundConnection a, InboundConnection b) => a.Equals(b);

#pragma warning restore

        /// <summary>
        /// Add <see cref="TcpClient"/> to current inbound connection of current thread.
        /// </summary>
        /// <exception cref="InvalidCastException"></exception>
        /// <param name="tcpClient"></param>
        public void Add(TcpClient tcpClient)
        {
            lock (inboundLocker)
                if (Clients.Count == maxConnections)
                    throw new InvalidCastException($"Cannot associate more sockets to current IP {tcpClient.Client.GetIpAddress().ToString()}, limit is set to {maxConnections}.");

            Clients.Add(tcpClient);

            onAdd.Invoke(null, tcpClient);
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

        /// <summary>
        /// Get an enumerable of <see cref="InboundPacket"/> from <see cref="InboundTraffic"/>.
        /// All pending packets are enqueued into <see cref="InboundConnection"/> for each <see cref="EndPoint"/>.
        /// </summary>
        /// <returns></returns>
        public IEnumerable<InboundPacket> GetEndPointPackets()
        {
            var traffics = inboundTraffics.ToArray();

            for (var i = 0; i < traffics.Length; i++)
            {
                if (GetFlag == ConnectionFlag.Aborted) yield break;

                var inboundTraffic = traffics[i];

                if (!inboundTraffic.HasPackets) continue;

                yield return new InboundPacket(traffics[i].EndPoint, traffics[i].Dequeue());
            }
        }

#pragma warning disable

        public override int GetHashCode() => GetIPAddress.GetHashCode();

#pragma warning restore

        /// <summary>
        /// Remove <see cref="TcpClient"/> to current inbound connection of current thread.
        /// </summary>
        /// <exception cref="InvalidOperationException"></exception>
        /// <exception cref="SocketException"></exception>
        /// <param name="tcpClient"></param>
        public void Remove(TcpClient tcpClient)
        {
            lock (inboundLocker)
                if (Clients.Count == 0)
                    throw new InvalidOperationException($"There is no socket to remove from current IP {tcpClient.Client.GetIpAddress().ToString()}.");

            if (Clients.Contains(tcpClient))
            {
                Clients.Remove(tcpClient);
                tcpClient.Close();
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

            onAdd += OnAddSocket;
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

            onAdd -= OnAddSocket;

            lock (inboundLocker)
                for (var i = 0; i < Clients.Count; i++)
                    Clients[i].Close();
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

        private void OnAddSocket(object sender, TcpClient tcpClient)
        {
            if (sender == null) return;

            if (tcpClient.Connected)
            {
                var procedure = new AsyncProcedure<KeyValuePair<InboundConnection, TcpClient>>(
                    $"{GetIPAddress.ToString()} - PID: #{Clients.Count + 1}",
                    new KeyValuePair<InboundConnection, TcpClient>((InboundConnection)sender, tcpClient),
                    (instance, name, input) =>
                    {
                        var inboundConn = input.Key;
                        var socket = input.Value;

                        do
                        {
                            if (!socket.Connected) break;

                            var buffer = new byte[inboundConn.bufferSize];
                            var receiveTask = Task.Run(async () => await socket.GetStream().ReadAsync(buffer, 0, buffer.Length));

                            Task.WaitAll(receiveTask);

                            var data = receiveTask.Result;
                            var inboundTraffic = inboundConn.inboundTraffics.FirstOrDefault(inb => inb.EndPoint == socket.Client.RemoteEndPoint);

                            if (inboundTraffic == default)
                            {
                                inboundTraffic = new InboundTraffic(socket.Client.RemoteEndPoint, inboundConn.maxPacketsPerEndPoint);
                                inboundConn.inboundTraffics.Add(inboundTraffic);
                            }

                            inboundTraffic.Enqueue(data);
                        } while (inboundConn.GetFlag == ConnectionFlag.Listening);

                        inboundConn.Remove(socket);

                        return new AsyncProcedureEventArgs<KeyValuePair<InboundConnection, TcpClient>>(input, true);
                    }
                );
                procedure.AttachToParent(GetToken);
                procedure.Execute();
            }
            else throw new SocketException((int)SocketError.NotConnected);
        }
    }
}
