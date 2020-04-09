using System.Net;

namespace CA.Networking
{
    /// <summary>
    /// Represents a structure for inbound packet from <see cref="InboundTraffic"/>.
    /// </summary>
    public readonly struct InboundPacket
    {
        /// <summary>
        /// Gets the network address of <see cref="InboundTraffic"/>.
        /// </summary>
        public readonly EndPoint EndPoint;

        /// <summary>
        /// Gets the pending packet of <see cref="InboundTraffic"/>.
        /// </summary>
        public readonly int PacketData;

#pragma warning disable

        public InboundPacket(
            EndPoint endPoint,
            int packetData
            )

#pragma warning restore

        {
            EndPoint = endPoint;
            PacketData = packetData;
        }
    }
}
