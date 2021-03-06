namespace PSTk.Redis.Database
{
    /// <summary>
    /// Standard configuration for <see cref="RedisEngine"/>.
    /// </summary>
    public sealed class RedisSettings
    {
        /// <summary>
        /// Create a new instance of <see cref="RedisSettings"/> with default values.
        /// </summary>
        public RedisSettings()
        { }

        /// <summary>
        /// Create a new instance of <see cref="RedisSettings"/> with custom values.
        /// </summary>
        /// <param name="host"></param>
        /// <param name="index"></param>
        /// <param name="password"></param>
        /// <param name="port"></param>
        /// <param name="syncTimeout"></param>
        /// <param name="asyncTimeout"></param>
        public RedisSettings(string host, sbyte index, string password, int port, int syncTimeout, int asyncTimeout)
        {
            Host = host;
            Index = index;
            Password = password;
            Port = port;
            SyncTimeout = syncTimeout;
            AsyncTimeout = asyncTimeout;
        }

        /// <summary>
        /// Time in milliseconds to allow for asynchronous operations.
        /// Default value:
        /// <code>5000</code>
        /// </summary>
        public int AsyncTimeout { get; set; } = 5000;

        /// <summary>
        /// The host name, default value:
        /// <code>localhost</code>
        /// </summary>
        public string Host { get; set; } = "localhost";

        /// <summary>
        /// The cluster index, default value:
        /// <code>0</code>
        /// </summary>
        public sbyte Index { get; set; } = 0;

        /// <summary>
        /// The password, default value:
        /// <code><see cref="string.Empty"/></code>
        /// </summary>
        public string Password { get; set; } = string.Empty;

        /// <summary>
        /// The connection port, default value:
        /// <code>6379</code>
        /// </summary>
        public int Port { get; set; } = 6379;

        /// <summary>
        /// Time in milliseconds to allow for synchronous operations.
        /// Default value:
        /// <code>5000</code>
        /// </summary>
        public int SyncTimeout { get; set; } = 5000;
    }
}
