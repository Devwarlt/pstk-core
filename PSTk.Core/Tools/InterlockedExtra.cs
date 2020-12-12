using System.Threading;

namespace PSTk.Core.Tools
{
    /// <summary>
    /// Contains extra methods for <see cref="Interlocked.Increment(ref int)"/> increment that supports types
    /// <see cref="ulong"/>, <see cref="float"/> and <see cref="double"/>.
    /// </summary>
    public static class InterlockedExtra
    {
        private static readonly object DoubleIncLock = new object();
        private static readonly object FloatIncLock = new object();
        private static readonly object UlongIncLock = new object();

        /// <summary>
        /// Increments a specified variable and store the result, as an atomic operation.
        /// </summary>
        /// <param name="location"></param>
        /// <returns></returns>
        public static ulong Increment(ref ulong location)
        {
            lock (UlongIncLock)
                return ++location;
        }

        /// <summary>
        /// Increments a specified variable and store the result, as an atomic operation.
        /// </summary>
        /// <param name="location"></param>
        /// <returns></returns>
        public static float Increment(ref float location)
        {
            lock (FloatIncLock)
                return ++location;
        }

        /// <summary>
        /// Increments a specified variable and store the result, as an atomic operation.
        /// </summary>
        /// <param name="location"></param>
        /// <returns></returns>
        public static double Increment(ref double location)
        {
            lock (DoubleIncLock)
                return ++location;
        }
    }
}
