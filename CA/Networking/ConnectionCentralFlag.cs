using System;

namespace CA.Networking
{
    /// <summary>
    /// Represents current flag of <see cref="ConnectionCentral"/> routine.
    /// </summary>
    [Flags]
    public enum ConnectionCentralFlag
    {
        /// <summary>
        /// When not listening for inbound connections.
        /// </summary>
        Idle,

        /// <summary>
        /// When listening for inbound connections and processing new clients.
        /// </summary>
        Listening,

        /// <summary>
        /// When listener aborted and no new connection is stablished along inbound traffic.
        /// </summary>
        Aborted
    }
}
