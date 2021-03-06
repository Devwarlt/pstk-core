using Newtonsoft.Json;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PSTk.Redis.Database
{
    /// <summary>
    /// Represents the base class for <see cref="RedisValue"/> object manipulation.
    /// </summary>
    public abstract class RedisObject
    {
        private Dictionary<RedisValue, KeyValuePair<byte[], bool>> entries;
        private string key;
        private List<HashEntry> update;

        /// <summary>
        /// Gets all keys from <see cref="entries"/>.
        /// </summary>
        public IEnumerable<RedisValue> AllKeys => entries.Keys;

        /// <summary>
        /// Gets current assigned <see cref="IDatabase"/> to the base class.
        /// </summary>
        public IDatabase Database { get; private set; }

        /// <summary>
        /// Verifies if number of <see cref="entries"/> are empty.
        /// </summary>
        public bool IsNull => entries.Count == 0;

        /// <summary>
        /// Gets the currrent key assigned to this object.
        /// </summary>
        public string Key { get; private set; }

        /// <summary>
        /// Performs a flush operation asynchronously. Also, enables configuration for
        /// <see cref="ITransaction"/> execution.
        /// </summary>
        /// <param name="transaction"></param>
        /// <returns></returns>
        public Task FlushAsync(ITransaction transaction = null)
        {
            ReadyFlush();
            return transaction == null
                ? Database.HashSetAsync(Key, update.ToArray())
                : transaction.HashSetAsync(Key, update.ToArray());
        }

        /// <summary>
        /// Gets value from <see cref="RedisValue"/>.
        /// <para>
        /// Supported types: <see cref="byte"/>, <see cref="int"/>, <see cref="uint"/>,
        /// <see cref="ushort"/>, <see cref="float"/>, <see cref="long"/>, <see cref="ulong"/>,
        /// <see cref="double"/>, <see cref="bool"/>, <see cref="DateTime"/>, <see cref="byte"/>[],
        /// <see cref="int"/>[], <see cref="uint"/>[], <see cref="ushort"/>[], <see cref="string"/> and
        /// <see cref="string"/>[].
        /// </para>
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <param name="def"></param>
        /// <returns></returns>
        public T GetValue<T>(RedisValue key, T def = default)
        {
            if (key.IsNullOrEmpty || !entries.TryGetValue(key, out KeyValuePair<byte[], bool> val) || val.Key == null)
                return def;

            if (typeof(T) == typeof(byte))
                try { return (T)(object)byte.Parse(Encoding.UTF8.GetString(val.Key)); }
                catch (OverflowException) { return (T)(object)byte.MaxValue; }

            if (typeof(T) == typeof(int))
                try { return (T)(object)int.Parse(Encoding.UTF8.GetString(val.Key)); }
                catch (OverflowException) { return (T)(object)int.MaxValue; }

            if (typeof(T) == typeof(uint))
                try { return (T)(object)uint.Parse(Encoding.UTF8.GetString(val.Key)); }
                catch (OverflowException) { return (T)(object)uint.MaxValue; }

            if (typeof(T) == typeof(ushort))
                try { return (T)(object)ushort.Parse(Encoding.UTF8.GetString(val.Key)); }
                catch (OverflowException) { return (T)(object)ushort.MaxValue; }

            if (typeof(T) == typeof(float))
                try { return (T)(object)float.Parse(Encoding.UTF8.GetString(val.Key)); }
                catch (OverflowException) { return (T)(object)float.MaxValue; }

            if (typeof(T) == typeof(long))
                try { return (T)(object)long.Parse(Encoding.UTF8.GetString(val.Key)); }
                catch (OverflowException) { return (T)(object)long.MaxValue; }

            if (typeof(T) == typeof(ulong))
                try { return (T)(object)ulong.Parse(Encoding.UTF8.GetString(val.Key)); }
                catch (OverflowException) { return (T)(object)ulong.MaxValue; }

            if (typeof(T) == typeof(double))
                try { return (T)(object)double.Parse(Encoding.UTF8.GetString(val.Key)); }
                catch (OverflowException) { return (T)(object)double.MaxValue; }

            if (typeof(T) == typeof(bool))
                return (T)(object)(val.Key[0] != 0);

            if (typeof(T) == typeof(DateTime))
                return (T)(object)DateTime.FromBinary(BitConverter.ToInt64(val.Key, 0));

            if (typeof(T) == typeof(byte[]))
                return (T)(object)val.Key;

            if (typeof(T) == typeof(ushort[]))
            {
                var ret = new ushort[val.Key.Length / 2];
                Buffer.BlockCopy(val.Key, 0, ret, 0, val.Key.Length);
                return (T)(object)ret;
            }

            if (typeof(T) == typeof(int[]) || typeof(T) == typeof(uint[]))
            {
                var ret = new int[val.Key.Length / 4];
                Buffer.BlockCopy(val.Key, 0, ret, 0, val.Key.Length);
                return (T)(object)ret;
            }

            if (typeof(T) == typeof(string))
                return (T)(object)Encoding.UTF8.GetString(val.Key);

            if (typeof(T) == typeof(string[]))
                return (T)(object)JsonConvert.SerializeObject(val.Key);

            throw new NotSupportedException();
        }

        /// <summary>
        /// Performs a reload operation to a specific field from <see cref="entries"/>.
        /// </summary>
        /// <param name="field"></param>
        public void Reload(string field = null)
        {
            if (field != null && entries != null)
            {
                entries[field] =
#if NET472
                    new KeyValuePair<byte[], bool>(Database.HashGet(Key, field), false);
#else
                    KeyValuePair.Create<byte[], bool>(Database.HashGet(Key, field), false);
#endif
                return;
            }

            entries = Database.HashGetAll(Key).ToDictionary(
                x => x.Name,
#if NET472
                x => new KeyValuePair<byte[], bool>(x.Value, false)
#else
                x => KeyValuePair.Create<byte[], bool>(x.Value, false)
#endif
            );
        }

        /// <summary>
        /// Performs a reload operation to a specific filed from <see cref="entries"/> asynchornously.
        /// Also, enables configuration for <see cref="ITransaction"/> execution.
        /// </summary>
        /// <param name="trans"></param>
        /// <param name="field"></param>
        /// <returns></returns>
        public async Task ReloadAsync(ITransaction trans = null, string field = null)
        {
            if (field != null && entries != null)
            {
                var tf = trans != null
                    ? trans.HashGetAsync(Key, field)
                    : Database.HashGetAsync(Key, field);

                try
                {
                    await tf;
                    entries[field] =
#if NET472
                        new KeyValuePair<byte[], bool>(tf.Result, false);
#else
                        KeyValuePair.Create<byte[], bool>(tf.Result, false);
#endif
                }
                catch { }
                return;
            }

            var t = trans != null ? trans.HashGetAllAsync(Key) : Database.HashGetAllAsync(Key);

            try
            {
                await t;
                entries = t.Result.ToDictionary(
                    x => x.Name,
#if NET472
                    x => new KeyValuePair<byte[], bool>(x.Value, false)
#else
                    x => KeyValuePair.Create<byte[], bool>(x.Value, false)
#endif
                );
            }
            catch { }
        }

        /// <summary>
        /// Sets value to <see cref="RedisValue"/>.
        /// <para>
        /// Supported types: <see cref="byte"/>, <see cref="int"/>, <see cref="uint"/>,
        /// <see cref="ushort"/>, <see cref="float"/>, <see cref="long"/>, <see cref="ulong"/>,
        /// <see cref="double"/>, <see cref="bool"/>, <see cref="DateTime"/>, <see cref="byte"/>[],
        /// <see cref="int"/>[], <see cref="uint"/>[], <see cref="ushort"/>[], <see cref="string"/> and
        /// <see cref="string"/>[].
        /// </para>
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <param name="val"></param>
        public void SetValue<T>(RedisValue key, T val)
        {
            if (val == null)
                return;

            byte[] buff;

            if (typeof(T) == typeof(byte)
                || typeof(T) == typeof(int)
                || typeof(T) == typeof(uint)
                || typeof(T) == typeof(ushort)
                || typeof(T) == typeof(string)
                || typeof(T) == typeof(float)
                || typeof(T) == typeof(long)
                || typeof(T) == typeof(ulong)
                || typeof(T) == typeof(double))
                buff = Encoding.UTF8.GetBytes(val.ToString());
            else if (typeof(T) == typeof(bool))
                buff = new byte[] { (byte)((bool)(object)val ? 1 : 0) };
            else if (typeof(T) == typeof(DateTime))
                buff = BitConverter.GetBytes(((DateTime)(object)val).ToBinary());
            else if (typeof(T) == typeof(byte[]))
                buff = (byte[])(object)val;
            else if (typeof(T) == typeof(ushort[]))
            {
                var v = (ushort[])(object)val;
                buff = new byte[v.Length * 2];
                Buffer.BlockCopy(v, 0, buff, 0, buff.Length);
            }
            else if (typeof(T) == typeof(int[]) || typeof(T) == typeof(uint[]))
            {
                var v = (int[])(object)val;
                buff = new byte[v.Length * 4];
                Buffer.BlockCopy(v, 0, buff, 0, buff.Length);
            }
            else if (typeof(T) == typeof(string[]))
                buff = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(val));
            else
                throw new NotSupportedException();

            if (!entries.ContainsKey(Key) || entries[Key].Key == null || !buff.SequenceEqual(entries[Key].Key))
                entries[key] =
#if NET472
                    new KeyValuePair<byte[], bool>(buff, true);
#else
                    KeyValuePair.Create(buff, true);
#endif
        }

        /// <summary>
        /// Gets all entries asynchronously.
        /// </summary>
        protected async void GetAllEntriesAsync()
        {
            var result = await Database.HashGetAllAsync(key);
            entries = result.ToDictionary(
                x => x.Name,
#if NET472
                x => new KeyValuePair<byte[], bool>(x.Value, false)
#else
                x => KeyValuePair.Create<byte[], bool>(x.Value, false)
#endif
            );
        }

        /// <summary>
        /// Gets raw value from <see cref="RedisValue"/>.
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        protected byte[] GetValueRaw(RedisValue key)
        {
            if (!entries.TryGetValue(key, out var val))
                return null;

            if (val.Key == null)
                return null;

            return (byte[])val.Key.Clone();
        }

        /// <summary>
        /// Internally initialize the base class to fetch all <see cref="entries"/> from <see cref="IDatabase"/>.
        /// </summary>
        /// <param name="db"></param>
        /// <param name="key"></param>
        /// <param name="field"></param>
        protected void Init(IDatabase db, string key, string field = null)
        {
            Key = key;
            Database = db;

            this.key = key;

            if (field == null)
                entries = db.HashGetAll(key).ToDictionary(
                    x => x.Name,
#if NET472
                    x => new KeyValuePair<byte[], bool>(x.Value, false)
#else
                    x => KeyValuePair.Create<byte[], bool>(x.Value, false)
#endif
                );
            else
            {
                var entry = new HashEntry[] { new HashEntry(field, db.HashGet(key, field)) };
                entries = entry.ToDictionary(
                    x => x.Name,
#if NET472
                    x => new KeyValuePair<byte[], bool>(x.Value, false)
#else
                    x => KeyValuePair.Create<byte[], bool>(x.Value, false)
#endif
                );
            }
        }

        private void ReadyFlush()
        {
            if (update == null)
                update = new List<HashEntry>();

            update.Clear();

            foreach (var name in entries.Keys.ToList())
                if (entries[name].Value)
                    update.Add(new HashEntry(name, entries[name].Key));

            foreach (var update in update)
                entries[update.Name] =
#if NET472
                    new KeyValuePair<byte[], bool>(entries[update.Name].Key, false);
#else
                    KeyValuePair.Create(entries[update.Name].Key, false);
#endif
        }
    }
}
