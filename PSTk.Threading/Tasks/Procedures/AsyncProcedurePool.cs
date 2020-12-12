using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace PSTk.Threading.Tasks.Procedures
{
    /// <summary>
    /// Handle <see cref="AsyncProcedure{TInput}"/> instances into pool of synchronous or asynchronous routines.
    /// </summary>
    /// <exception cref="ArgumentNullException"></exception>
    /// <exception cref="ArgumentOutOfRangeException"></exception>
    public sealed class AsyncProcedurePool : IAttachedTask
    {
        private readonly IAsyncProcedure[] pool;
        private readonly CancellationTokenSource source;

        /// <summary>
        /// Create a new instance of <see cref="AsyncProcedurePool"/>.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="pool"></param>
        /// <param name="source"></param>
        public AsyncProcedurePool(string name, IAsyncProcedure[] pool, CancellationTokenSource source = null)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentNullException(nameof(name));
            if (pool == null)
                throw new ArgumentNullException(nameof(pool));
            if (pool.Length == 0)
                throw new ArgumentOutOfRangeException(nameof(pool), "Required at least 1 AsyncProcedure.");

            Name = name;
            this.pool = pool;
            this.source = source ?? new CancellationTokenSource();
            Token = this.source.Token;
            AttachPoolToContext(Token);
        }

        /// <summary>
        /// Get name of <see cref="AsyncProcedurePool"/>.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Return number of <see cref="IAsyncProcedure"/> in pool.
        /// </summary>
        public int NumProcedures => pool.Length;

        /// <summary>
        /// Get the <see cref="CancellationToken"/> of attached task.
        /// </summary>
        public CancellationToken Token { get; private set; }

        /// <summary>
        /// Get or set <see cref="IAsyncProcedure"/> from <see cref="AsyncProcedurePool"/> collection
        /// using index.
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public IAsyncProcedure this[int index]
        {
            get => pool[index];
            set => pool[index] = value;
        }

        /// <summary>
        /// Attach a process to parent in case of external task cancelation request.
        /// </summary>
        /// <param name="token"></param>
        public void AttachToParent(CancellationToken token)
        {
            Token = token;
            AttachPoolToContext(Token);
        }

        /// <summary>
        /// Cancel all routines whose are running.
        /// </summary>
        public void CancelAll() => source.Cancel();

        /// <summary>
        /// Execute the pool of routines.
        /// </summary>
        /// <returns></returns>
        public IEnumerable<bool> ExecuteAll()
        {
            for (var i = 0; i < pool.Length; i++)
                yield return pool[i].Execute();
        }

        /// <summary>
        /// Execute the <see cref="pool"/> of routines in parallel.
        /// </summary>
        /// <returns></returns>
        public bool[] ExecuteAllAsParallel()
        {
            var tasks = pool
                .AsParallel()
                .Select(asyncProcedure => Task.Run(() => asyncProcedure.Execute()))
                .ToArray();
            return Task.WhenAll(tasks).Result;
        }

        private void AttachPoolToContext(CancellationToken token)
        {
            for (var i = 0; i < pool.Length; i++)
                pool[i].AttachToParent(token);
        }
    }
}
