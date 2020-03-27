using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace CA.Threading.Tasks.Procedures
{
    /// <summary>
    /// Handle <see cref="AsyncProcedure{TInput}"/> instances
    /// into pool of synchronous or asynchronous routines.
    /// </summary>
    /// <exception cref="ArgumentNullException"></exception>
    /// <exception cref="ArgumentOutOfRangeException"></exception>
    public sealed class AsyncProcedurePool : IAttachedTask
    {
        private readonly IAsyncProcedure[] pool;
        private readonly CancellationTokenSource source;

#pragma warning disable

        private CancellationToken token;

        public AsyncProcedurePool(
            IAsyncProcedure[] pool,
            CancellationTokenSource source = null
            )

#pragma warning restore

        {
            if (pool == null) throw new ArgumentNullException("pool");
            if (pool.Length == 0) throw new ArgumentOutOfRangeException("pool", "Required at least 1 AsyncProcedure.");

            this.pool = pool;
            this.source = source ?? new CancellationTokenSource();

            token = this.source.Token;

            AttachPoolToContext(token);
        }

        /// <summary>
        /// Get the <see cref="CancellationToken"/> of attached task.
        /// </summary>
        public CancellationToken GetToken => token;

        /// <summary>
        /// Return number of <see cref="IAsyncProcedure"/> in pool.
        /// </summary>
        public int NumProcedures => pool.Length;

#pragma warning disable

        public IAsyncProcedure this[int index]
        {
            get => pool[index];
            set => pool[index] = value;
        }

#pragma warning restore

        /// <summary>
        /// Attach a process to parent in case of external task
        /// cancelation request.
        /// </summary>
        /// <param name="token"></param>
        public void AttachToParent(CancellationToken token)
        {
            this.token = token;

            AttachPoolToContext(this.token);
        }

        /// <summary>
        /// Cancel all routines whose are running.
        /// </summary>
        public void CancelAll() => source.Cancel();

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
        /// in parallel.
        /// </summary>
        /// <returns></returns>
        public bool[] ExecuteAllAsParallel()
        {
            var tasks = pool
                .AsParallel()
                .Select(async asyncProcedure => await Task.Run(() => asyncProcedure.Execute()))
                .ToArray();
            return Task.WhenAll(tasks).Result;
        }

#pragma warning disable

        private void AttachPoolToContext(CancellationToken token)
        {
            for (var i = 0; i < pool.Length; i++)
                pool[i].AttachToParent(token);
        }

#pragma warning restore
    }
}