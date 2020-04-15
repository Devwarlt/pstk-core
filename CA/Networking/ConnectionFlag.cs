using System;

namespace CA.Networking
{
    /// <summary>
    /// Represents current flag of connection routines.
    /// </summary>
    [Flags, Obsolete]
    public enum ConnectionFlag
    {
        /// <summary>
        /// When not listening for inbound connections.
        /// </summary>
        Idle,

        /// <summary>
        /// When listening for inbound traffic.
        /// </summary>
        Listening,

        /// <summary>
        /// When listener aborted and no new connection is stablished along inbound traffic.
        /// </summary>
        Aborted
    }
}
