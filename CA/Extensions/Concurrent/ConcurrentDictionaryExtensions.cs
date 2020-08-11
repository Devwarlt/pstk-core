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
        /// Provides a predicate check from all <see cref="ConcurrentDictionary{TKey, TValue}.Values"/> entries,
        /// but rather than create a new collection, it'll do a loop through exist values.
        /// </summary>
        /// <typeparam name="TKey"></typeparam>
        /// <typeparam name="TValue"></typeparam>
        /// <param name="collection"></param>
        /// <param name="predicate"></param>
        /// <returns></returns>
        public static IEnumerable<TValue> ValueWhere<TKey, TValue>(this ConcurrentDictionary<TKey, TValue> collection, Predicate<TValue> predicate)
        {
            foreach (var entry in collection)
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
        /// <returns></returns>
        public static TValue[] ValueWhereAsParallel<TKey, TValue>(this ConcurrentDictionary<TKey, TValue> collection, Predicate<TValue> predicate)
            => collection.AsParallel().Where(_ => predicate.Invoke(_.Value)).Select(_ => _.Value).ToArray();
    }
}
