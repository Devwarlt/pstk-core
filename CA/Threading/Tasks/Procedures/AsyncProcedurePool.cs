using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CA.Threading.Tasks.Procedures
{
    /// <summary>
    /// Handle <see cref="AsyncProcedure{TInput}"/> instances
    /// into pool of synchronous or asynchronous routines.
    /// </summary>
    public sealed class AsyncProcedurePool
    {
        private readonly IAsyncProcedure[] pool;

#pragma warning disable CS1591

        public AsyncProcedurePool(IAsyncProcedure[] pool)

#pragma warning restore CS1591
            => this.pool = pool;

        /// <summary>
        /// Execute the <see cref="pool"/> of routines.
        /// </summary>
        /// <returns></returns>
        public IEnumerable<bool> ExecuteAll()
        {
            for (var i = 0; i < pool.Length; i++)
                yield return pool[i].Execute();
        }

        /// <summary>
        /// Execute the <see cref="pool"/> of routines
        /// asynchronously.
        /// </summary>
        /// <returns></returns>
        public IEnumerable<Task<bool>> ExecuteAsyncAll()
        {
            for (var i = 0; i < pool.Length; i++)
                yield return pool[i].ExecuteAsync();
        }

        /// <summary>
        /// Execute the <see cref="pool"/> of routines
        /// asynchronously in parallel.
        /// </summary>
        /// <returns></returns>
        public Task<bool>[] ExecuteAsyncParallel()
            => pool.AsParallel().Select(asyncProcedure => asyncProcedure.ExecuteAsync()).ToArray();

        /// <summary>
        /// Execute the <see cref="pool"/> of routines
        /// in parallel.
        /// </summary>
        /// <returns></returns>
        public bool[] ExecuteParallel()
            => pool.AsParallel().Select(asyncProcedure => asyncProcedure.Execute()).ToArray();
    }
}