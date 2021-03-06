using PSTk.Redis.Exceptions;
using StackExchange.Redis;
using System;
using System.Text;

namespace PSTk.Redis.Database
{
    /// <summary>
    /// Base class to initialize Redis storage engine.
    /// </summary>
    public sealed class AsyncRedisEngine
        : IDisposable
    {
        private readonly RedisSettings redisSettings;

        private IConnectionMultiplexer connectionMultiplexer;
        private IDatabase database;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

        public AsyncRedisEngine(RedisSettings redisSettings) => this.redisSettings = redisSettings;

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
        /// Dispose <see cref="IConnectionMultiplexer"/> asynchronously and close connection.
        /// </summary>
        public async void Dispose()
        {
            if (!IsConnected)
                return;

            await connectionMultiplexer.CloseAsync();
            connectionMultiplexer.Dispose();
        }

        /// <summary>
        /// Tries to create a new <see cref="IConnectionMultiplexer"/> connection asynchronously.
        /// </summary>
        /// <exception cref="ConnectionException"></exception>
        public async void StartAsync()
        {
            var connectionStr = new StringBuilder();
            connectionStr.Append($"{redisSettings.Host}:{redisSettings.Port}");
            if (!string.IsNullOrWhiteSpace(redisSettings.Password))
                connectionStr.Append($",password={redisSettings.Password}");

            connectionStr.Append($",syncTimeout={redisSettings.SyncTimeout}");
            connectionStr.Append($",asyncTimeout={redisSettings.AsyncTimeout}");

            try
            {
                connectionMultiplexer = await ConnectionMultiplexer.ConnectAsync(connectionStr.ToString());
                database = connectionMultiplexer.GetDatabase(redisSettings.Index);
            }
            catch (RedisConnectionException e)
            {
                throw new ConnectionException(
                    "Redis service cannot initialize. Turn on cluster server to begin database transactions.",
                    e
                );
            }
        }
    }
}
