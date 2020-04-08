using System;

namespace CA.Networking
{
    /// <summary>
    /// Represents the type of connection for <see cref="ConnectionCentral"/> instance.
    /// </summary>
    [Flags]
    public enum ConnectionType : short
    {
        /// <summary>
        /// Restricted to the same host.
        /// </summary>
        Local = 0,

        /// <summary>
        /// Restricted to the same subnet.
        /// </summary>
        Subnet = 1,

        /// <summary>
        /// Restricted to the same site.
        /// </summary>
        Intranet = 32,

        /// <summary>
        /// Restricted to the same region.
        /// </summary>
        Regional = 64,

        /// <summary>
        /// Restricted to the same continent.
        /// </summary>
        Continental = 128,

        /// <summary>
        /// Unrestricted.
        /// </summary>
        World = 255
    }
}
