using PSTk.Redis.Exceptions;
using StackExchange.Redis;
using System;
using System.Text;

namespace PSTk.Redis.Database
{
    /// <summary>
    /// Base class to initialize Redis storage engine.
    /// </summary>
    public sealed class RedisEngine
        : IDisposable
    {
        private readonly RedisSettings redisSettings;

        private IConnectionMultiplexer connectionMultiplexer;
        private IDatabase database;
        private ISubscriber subscriber;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

        public RedisEngine(RedisSettings redisSettings) => this.redisSettings = redisSettings;

#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member

        /// <summary>
        /// Gets attached <see cref="IDatabase"/> of current instance.
        /// </summary>
        public IDatabase Database => database;

        /// <summary>
        /// Verify if <see cref="IConnectionMultiplexer"/> is connected.
        /// </summary>
        public bool IsConnected => connectionMultiplexer != null && connectionMultiplexer.IsConnected;

        /// <summary>
        /// Gets <see cref="ISubscriber"/> of current <see cref="IDatabase"/> instance.
        /// </summary>
        public ISubscriber Subscriber => subscriber;

        /// <summary>
        /// Tries to close <see cref="IConnectionMultiplexer"/> connection synchronously.
        /// </summary>
        public void Close()
        {
            if (!IsConnected)
                return;

            connectionMultiplexer.Close();
        }

        /// <summary>
        /// Tries to close <see cref="IConnectionMultiplexer"/> connection asynchronously.
        /// </summary>
        public async void CloseAsync()
        {
            if (!IsConnected)
                return;

            await connectionMultiplexer.CloseAsync();
        }

        /// <summary>
        /// Dispose <see cref="IConnectionMultiplexer"/>.
        /// </summary>
        public void Dispose() => connectionMultiplexer.Dispose();

        /// <summary>
        /// Tries to create a new <see cref="IConnectionMultiplexer"/> connection synchronously.
        /// </summary>
        /// <exception cref="ConnectionException"></exception>
        public void Start()
        {
            try
            {
                var connectionStr = GetConnectionStrings();
                connectionMultiplexer = ConnectionMultiplexer.Connect(connectionStr);
                database = connectionMultiplexer.GetDatabase(redisSettings.Index);
                subscriber = connectionMultiplexer.GetSubscriber();
            }
            catch (RedisConnectionException e)
            {
                throw new ConnectionException(
                    "Redis service cannot initialize. Turn on cluster server to begin database transactions.",
                    e
                );
            }
        }

        /// <summary>
        /// Tries to create a new <see cref="IConnectionMultiplexer"/> connection asynchronously.
        /// </summary>
        /// <exception cref="ConnectionException"></exception>
        public async void StartAsync()
        {
            try
            {
                var connectionStr = GetConnectionStrings();
                connectionMultiplexer = await ConnectionMultiplexer.ConnectAsync(connectionStr);
                database = connectionMultiplexer.GetDatabase(redisSettings.Index);
                subscriber = connectionMultiplexer.GetSubscriber();
            }
            catch (RedisConnectionException e)
            {
                throw new ConnectionException(
                    "Redis service cannot initialize. Turn on cluster server to begin database transactions.",
                    e
                );
            }
        }

        private string GetConnectionStrings()
        {
            var connectionStr = new StringBuilder();
            connectionStr.Append($"{redisSettings.Host}:{redisSettings.Port}");
            if (!string.IsNullOrWhiteSpace(redisSettings.Password))
                connectionStr.Append($",password={redisSettings.Password}");

            connectionStr.Append($",syncTimeout={redisSettings.SyncTimeout}");
            connectionStr.Append($",asyncTimeout={redisSettings.AsyncTimeout}");
            return connectionStr.ToString();
        }
    }
}
