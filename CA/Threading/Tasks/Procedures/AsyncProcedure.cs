using System;
using System.Threading;
using System.Threading.Tasks;

namespace CA.Threading.Tasks.Procedures
{
    /// <summary>
    /// Used for situations that require dependency between other procedure. Recommended to use it with <see cref="AsyncProcedurePool"/>.
    /// </summary>
    /// <typeparam name="TInput"></typeparam>
    /// <exception cref="ArgumentNullException"></exception>
    /// <exception cref="OperationCanceledException"></exception>
    public sealed class AsyncProcedure<TInput> : IAsyncProcedure
    {
#pragma warning disable
        private readonly TInput input;
        private readonly string name;
        private readonly Func<TInput, CancellationToken, Task<AsyncProcedureEventArgs<TInput>>> procedure;

        private CancellationToken token = default;

        public AsyncProcedure(
            string name,
            TInput input,
            Func<AsyncProcedure<TInput>, string, TInput, AsyncProcedureEventArgs<TInput>> procedure,
            Action<string> errorLogger = null
            )
        {
            if (input == null) throw new ArgumentNullException("input");
            if (procedure == null) throw new ArgumentNullException("procedure");

            this.name = name;
            this.input = input;
            this.procedure = async (inputRef, tokenRef) =>
            {
                Task<AsyncProcedureEventArgs<TInput>> task;

                if (tokenRef != null) task = Task.Run(() => procedure(this, name, inputRef), tokenRef);
                else task = Task.Run(() => procedure(this, name, inputRef));

                return task.Result;
            };

            onError += (s, e) => errorLogger?.Invoke(e.ToString());
        }

#pragma warning restore

        /// <summary>
        /// When procedure is canceled by parent task via <see cref="CancellationToken"/> and forced to stop all running processes.
        /// </summary>
        public event EventHandler<AsyncProcedureEventArgs<TInput>> OnCanceled;

        /// <summary>
        /// When procedure is completed with success.
        /// </summary>
        public event EventHandler<AsyncProcedureEventArgs<TInput>> OnCompleted;

        private event EventHandler<Exception> onError;

        /// <summary>
        /// Get the name of procedure.
        /// </summary>
        public string GetName => name;

        /// <summary>
        /// Get the <see cref="CancellationToken"/> of attached task.
        /// </summary>
        public CancellationToken GetToken => token;

        /// <summary>
        /// Attach a process to parent in case of external task cancellation request.
        /// </summary>
        /// <param name="token"></param>
        public void AttachToParent(CancellationToken token) => this.token = token;

        /// <summary>
        /// Execute the procedure.
        /// </summary>
        /// <exception cref="OperationCanceledException"></exception>
        /// <returns></returns>
        public bool Execute()
        {
            try
            {
                var task = procedure.Invoke(input, token);

                token.ThrowIfCancellationRequested();

                var result = task.Result;

                OnCompleted?.Invoke(this, result);

                return true;
            }
            catch (OperationCanceledException e)
            {
                OnCanceled?.Invoke(this, new AsyncProcedureEventArgs<TInput>(input, false));
                onError.Invoke(null, e);
            }

            return false;
        }
    }
}
