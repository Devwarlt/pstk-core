using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace CA.Extensions.Concurrent
{
#pragma warning disable

    public static class ConcurrentDictionaryExtensions
#pragma warning restore
    {
        /// <summary>
        /// Provides a predicate check from all <see cref="ConcurrentDictionary{TKey, TValue}"/> entries,
        /// but rather than create a new collection, it'll do a loop through exist entries.
        /// </summary>
        /// <typeparam name="TKey"></typeparam>
        /// <typeparam name="TValue"></typeparam>
        /// <param name="collection"></param>
        /// <param name="predicate"></param>
        /// <exception cref="InvalidOperationException"></exception>
        /// <returns></returns>
        public static IEnumerable<KeyValuePair<TKey, TValue>> EntryWhere<TKey, TValue>(this ConcurrentDictionary<TKey, TValue> collection, Func<KeyValuePair<TKey, TValue>, bool> predicate)
        {
            if (collection == null)
                throw new InvalidOperationException("Collection must be not null.");

            var collectionCpy = collection.ToDictionary(entry => entry.Key, entry => entry.Value);
            foreach (var entry in collectionCpy)
                if (predicate.Invoke(entry))
                    yield return entry;
        }

        /// <summary>
        /// Provides a predicate check from all <see cref="ConcurrentDictionary{TKey, TValue}"/> entries,
        /// but rather than create a new collection, it'll do a loop through exist entries in parallel.
        /// </summary>
        /// <typeparam name="TKey"></typeparam>
        /// <typeparam name="TValue"></typeparam>
        /// <param name="collection"></param>
        /// <param name="predicate"></param>
        /// <exception cref="InvalidOperationException"></exception>
        /// <returns></returns>
        public static KeyValuePair<TKey, TValue>[] EntryWhereAsParallel<TKey, TValue>(this ConcurrentDictionary<TKey, TValue> collection, Func<KeyValuePair<TKey, TValue>, bool> predicate)
        {
            if (collection == null)
                throw new InvalidOperationException("Collection must be not null.");

            return collection.AsParallel().Where(entry => predicate.Invoke(entry)).Select(entry => entry).ToArray();
        }

        /// <summary>
        /// Try to get all occurrency values from all <see cref="ConcurrentDictionary{TKey, TValue}.Keys"/> entries.
        /// </summary>
        /// <typeparam name="TKey"></typeparam>
        /// <typeparam name="TValue"></typeparam>
        /// <param name="collection"></param>
        /// <param name="predicate"></param>
        /// <exception cref="InvalidOperationException"></exception>
        /// <returns></returns>
        public static TKey[] KeyFromValueWhereAsParallel<TKey, TValue>(this ConcurrentDictionary<TKey, TValue> collection, Predicate<TValue> predicate)
        {
            if (collection == null)
                throw new InvalidOperationException("Collection must be not null.");

            var result = collection.EntryWhereAsParallel(entry => predicate.Invoke(entry.Value));
            if (result == null)
                return Array.Empty<TKey>();

            return result.Select(entry => entry.Key).ToArray();
        }

        /// <summary>
        /// Provides a predicate check from all <see cref="ConcurrentDictionary{TKey, TValue}.Keys"/> entries,
        /// but rather than create a new collection, it'll do a loop through exist keys.
        /// </summary>
        /// <typeparam name="TKey"></typeparam>
        /// <typeparam name="TValue"></typeparam>
        /// <param name="collection"></param>
        /// <param name="predicate"></param>
        /// <exception cref="InvalidOperationException"></exception>
        /// <returns></returns>
        public static IEnumerable<TKey> KeyWhere<TKey, TValue>(this ConcurrentDictionary<TKey, TValue> collection, Predicate<TKey> predicate)
        {
            if (collection == null)
                throw new InvalidOperationException("Collection must be not null.");

            var collectionCpy = collection.ToDictionary(entry => entry.Key, entry => entry.Value);
            foreach (var entry in collectionCpy)
                if (predicate.Invoke(entry.Key))
                    yield return entry.Key;
        }

        /// <summary>
        /// Provides a predicate check from all <see cref="ConcurrentDictionary{TKey, TValue}.Keys"/> entries,
        /// but rather than create a new collection, it'll do a loop through exist keys in parallel.
        /// </summary>
        /// <typeparam name="TKey"></typeparam>
        /// <typeparam name="TValue"></typeparam>
        /// <param name="collection"></param>
        /// <param name="predicate"></param>
        /// <exception cref="InvalidOperationException"></exception>
        /// <returns></returns>
        public static TKey[] KeyWhereAsParallel<TKey, TValue>(this ConcurrentDictionary<TKey, TValue> collection, Predicate<TKey> predicate)
        {
            if (collection == null)
                throw new InvalidOperationException("Collection must be not null.");

            return collection.AsParallel().Where(_ => predicate.Invoke(_.Key)).Select(_ => _.Key).ToArray();
        }

        /// <summary>
        /// Try to get a first and single occurrency key from all <see cref="ConcurrentDictionary{TKey, TValue}.Keys"/> entries.
        /// </summary>
        /// <typeparam name="TKey"></typeparam>
        /// <typeparam name="TValue"></typeparam>
        /// <param name="collection"></param>
        /// <param name="predicate"></param>
        /// <param name="key"></param>
        /// <exception cref="InvalidOperationException"></exception>
        /// <returns></returns>
        public static bool TryGetKeySingle<TKey, TValue>(this ConcurrentDictionary<TKey, TValue> collection, Predicate<TValue> predicate, out TKey key)
        {
            if (collection == null)
                throw new InvalidOperationException("Collection must be not null.");

            var result = collection.EntryWhereAsParallel(entry => predicate.Invoke(entry.Value));
            if (result == null)
            {
                key = default;
                return false;
            }

            key = result[0].Key;
            return true;
        }

        /// <summary>
        /// Try to get a first and single occurrency value from all <see cref="ConcurrentDictionary{TKey, TValue}.Values"/> entries.
        /// </summary>
        /// <typeparam name="TKey"></typeparam>
        /// <typeparam name="TValue"></typeparam>
        /// <param name="collection"></param>
        /// <param name="predicate"></param>
        /// <param name="value"></param>
        /// <exception cref="InvalidOperationException"></exception>
        /// <returns></returns>
        public static bool TryGetValueSingle<TKey, TValue>(this ConcurrentDictionary<TKey, TValue> collection, Predicate<TKey> predicate, out TValue value)
        {
            var result = collection.EntryWhereAsParallel(entry => predicate.Invoke(entry.Key));
            if (result == null)
            {
                value = default;
                return false;
            }

            value = result[0].Value;
            return true;
        }

        /// <summary>
        /// Try to remove all <see cref="ConcurrentDictionary{TKey, TValue}.Values"/> using <see cref="ConcurrentDictionary{TKey, TValue}.TryRemove(TKey, out TValue)"/>
        /// iteration loop for each key in <paramref name="collection"/>.
        /// </summary>
        /// <typeparam name="TKey"></typeparam>
        /// <typeparam name="TValue"></typeparam>
        /// <param name="collection"></param>
        /// <param name="keys"></param>
        /// <exception cref="InvalidOperationException"></exception>
        /// <returns></returns>
        public static IEnumerable<bool> TryRemoveRange<TKey, TValue>(this ConcurrentDictionary<TKey, TValue> collection, TKey[] keys)
        {
            if (collection == null)
                throw new InvalidOperationException("Collection must be not null.");

            for (var i = 0; i < keys.Length; i++)
                yield return collection.TryRemove(keys[i], out _);
        }

        /// <summary>
        /// Try to get all occurrency values from all <see cref="ConcurrentDictionary{TKey, TValue}.Values"/> entries.
        /// </summary>
        /// <typeparam name="TKey"></typeparam>
        /// <typeparam name="TValue"></typeparam>
        /// <param name="collection"></param>
        /// <param name="predicate"></param>
        /// <exception cref="InvalidOperationException"></exception>
        /// <returns></returns>
        public static TValue[] ValueFromKeyWhereAsParallel<TKey, TValue>(this ConcurrentDictionary<TKey, TValue> collection, Predicate<TValue> predicate)
        {
            if (collection == null)
                throw new InvalidOperationException("Collection must be not null.");

            var result = collection.EntryWhereAsParallel(entry => predicate.Invoke(entry.Value));
            if (result == null)
                return Array.Empty<TValue>();

            return result.Select(entry => entry.Value).ToArray();
        }

        /// <summary>
        /// Provides a predicate check from all <see cref="ConcurrentDictionary{TKey, TValue}.Values"/> entries,
        /// but rather than create a new collection, it'll do a loop through exist values.
        /// </summary>
        /// <typeparam name="TKey"></typeparam>
        /// <typeparam name="TValue"></typeparam>
        /// <param name="collection"></param>
        /// <param name="predicate"></param>
        /// <exception cref="InvalidOperationException"></exception>
        /// <returns></returns>
        public static IEnumerable<TValue> ValueWhere<TKey, TValue>(this ConcurrentDictionary<TKey, TValue> collection, Predicate<TValue> predicate)
        {
            if (collection == null)
                throw new InvalidOperationException("Collection must be not null.");

            var collectionCpy = collection.ToDictionary(entry => entry.Key, entry => entry.Value);
            foreach (var entry in collectionCpy)
                if (predicate.Invoke(entry.Value))
                    yield return entry.Value;
        }

        /// <summary>
        /// Provides a predicate check from all <see cref="ConcurrentDictionary{TKey, TValue}.Values"/> entries,
        /// but rather than create a new collection, it'll do a loop through exist values in parallel.
        /// </summary>
        /// <typeparam name="TKey"></typeparam>
        /// <typeparam name="TValue"></typeparam>
        /// <param name="collection"></param>
        /// <param name="predicate"></param>
        /// <exception cref="InvalidOperationException"></exception>
        /// <returns></returns>
        public static TValue[] ValueWhereAsParallel<TKey, TValue>(this ConcurrentDictionary<TKey, TValue> collection, Predicate<TValue> predicate)
        {
            if (collection == null)
                throw new InvalidOperationException("Collection must be not null.");

            return collection.AsParallel().Where(entry => predicate.Invoke(entry.Value)).Select(entry => entry.Value).ToArray();
        }
    }
}
