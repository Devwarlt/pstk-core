using System;
using System.Collections.Generic;
using System.Net;

namespace PSTk.Networking
{
    /// <summary>
    /// Represents an inbound traffic from <see cref="InboundConnection"/>.
    /// </summary>
    [Obsolete]
    public class InboundTraffic
    {
        private readonly object inboundLock;
        private readonly ushort maxPackets;
        private Queue<int> packetDatas;

#pragma warning disable

        public InboundTraffic(
            EndPoint endPoint,
            ushort maxPackets
            )

#pragma warning restore

        {
            this.maxPackets = maxPackets;

            EndPoint = endPoint;

            packetDatas = new Queue<int>();
            inboundLock = new object();
        }

        /// <summary>
        /// Gets the network address of <see cref="InboundConnection"/> traffic.
        /// </summary>
        public EndPoint EndPoint { get; }

        /// <summary>
        /// Check if contains pending packets.
        /// </summary>
        public bool HasPackets { get { lock (inboundLock) return packetDatas.Count != 0; } }

        /// <summary>
        /// Dequeue a pending packet data from <see cref="InboundConnection"/> traffic.
        /// </summary>
        /// <returns></returns>
        public int Dequeue() => packetDatas.Dequeue();

        /// <summary>
        /// Enqueue to pending packet datas of <see cref="InboundConnection"/> traffic.
        /// </summary>
        /// <param name="packetData"></param>
        public void Enqueue(int packetData) => packetDatas.Enqueue(packetData);
    }
}
