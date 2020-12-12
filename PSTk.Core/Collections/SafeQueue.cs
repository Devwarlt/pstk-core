using System.Collections.Concurrent;
using System.Collections.Generic;

namespace PSTk.Core.Collections
{
    /// <summary>
    /// A faster version of <see cref="ConcurrentQueue{T}"/>.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public sealed class SafeQueue<T>
    {
        private readonly Queue<T> queue = new Queue<T>();

        /// <summary>
        /// Gets the number of elements contained in <see cref="SafeQueue{T}"/>.
        /// </summary>
        public int Count
        {
            get
            {
                lock (queue)
                    return queue.Count;
            }
        }

        /// <summary>
        /// Removes all objects from <see cref="SafeQueue{T}"/>.
        /// </summary>
        public void Clear()
        {
            lock (queue)
                queue.Clear();
        }

        /// <summary>
        /// Adds an object to the end of the <see cref="SafeQueue{T}"/>.
        /// </summary>
        /// <param name="item"></param>
        public void Enqueue(T item)
        {
            lock (queue)
                queue.Enqueue(item);
        }

        /// <summary>
        /// Tries to remove and return the object at the beginning of the <see cref="SafeQueue{T}"/>.
        /// </summary>
        /// <param name="result"></param>
        /// <returns></returns>
        public bool TryDequeue(out T result)
        {
            lock (queue)
            {
                result = default;

                if (queue.Count > 0)
                {
                    result = queue.Dequeue();
                    return true;
                }

                return false;
            }
        }

        /// <summary>
        /// Tries to remove all objects of the <see cref="SafeQueue{T}"/>.
        /// </summary>
        /// <param name="result"></param>
        /// <returns></returns>
        public bool TryDequeueAll(out T[] result)
        {
            lock (queue)
            {
                result = queue.ToArray();
                queue.Clear();
                return result.Length > 0;
            }
        }
    }
}
