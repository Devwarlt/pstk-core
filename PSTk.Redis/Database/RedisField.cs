namespace PSTk.Redis.Database
{
    /// <summary>
    /// Base class to iterate with <see cref="RedisObject"/>.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public sealed class RedisField<T>
    {
        private string key;
        private RedisObject redisObject;

        private RedisField()
        { }

        /// <summary>
        /// Getter and setter for current <see cref="RedisObject"/>.
        /// </summary>
        public T Field
        {
            get => redisObject.GetValue<T>(key);
            set => redisObject.SetValue(key, value);
        }

        /// <summary>
        /// Create a generic version of object for <see cref="RedisField{T}"/>.
        /// </summary>
        /// <param name="redisObject"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public static RedisField<T> Create(RedisObject redisObject, string key)
        {
            var redisField = new RedisField<T>()
            {
                redisObject = redisObject,
                key = key
            };
            return redisField;
        }
    }
}
