using System;

namespace PSTk.Redis.Exceptions
{
    /// <summary>
    /// Represents errors that occur during connection attempt to a Redis server.
    /// </summary>
    public sealed class ConnectionException : Exception
    {
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

        public ConnectionException(string message, Exception innerException = null)

#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
            : base(message, innerException)
        { }
    }
}
