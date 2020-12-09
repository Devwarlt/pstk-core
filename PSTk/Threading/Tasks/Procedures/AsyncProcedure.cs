using System;
using System.Threading;
using System.Threading.Tasks;

namespace PSTk.Threading.Tasks.Procedures
{
    /// <summary>
    /// Used for situations that require dependency between other procedure. Recommended to use it with <see cref="AsyncProcedurePool"/>.
    /// </summary>
    /// <typeparam name="TInput"></typeparam>
    /// <exception cref="ArgumentNullException"></exception>
    /// <exception cref="OperationCanceledException"></exception>
    public sealed class AsyncProcedure<TInput> : IAsyncProcedure
    {
        private readonly TInput input;
        private readonly Func<TInput, CancellationToken, Task<AsyncProcedureEventArgs<TInput>>> procedure;

        /// <summary>
        /// Create a new instance of <see cref="AsyncProcedure{TInput}"/>.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="input"></param>
        /// <param name="procedure"></param>
        /// <param name="errorLogger"></param>
        public AsyncProcedure(string name, TInput input, Func<AsyncProcedure<TInput>, string, TInput, AsyncProcedureEventArgs<TInput>> procedure, Action<string> errorLogger = null)
        {
            if (input == null)
                throw new ArgumentNullException("input");

            if (procedure == null)
                throw new ArgumentNullException("procedure");

            Name = name;

            this.input = input;
            this.procedure = (inputRef, tokenRef) =>
                tokenRef != null
                    ? Task.Run(() => procedure(this, name, inputRef), tokenRef)
                    : Task.Run(() => procedure(this, name, inputRef));

            onError += (s, e) => errorLogger?.Invoke(e.ToString());
        }

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
        public string Name { get; private set; }

        /// <summary>
        /// Get the <see cref="CancellationToken"/> of attached task.
        /// </summary>
        public CancellationToken Token { get; private set; } = default;

        /// <summary>
        /// Attach a process to parent in case of external task cancellation request.
        /// </summary>
        /// <param name="token"></param>
        public void AttachToParent(CancellationToken token) => Token = token;

        /// <summary>
        /// Execute the procedure.
        /// </summary>
        /// <exception cref="OperationCanceledException"></exception>
        /// <returns></returns>
        public bool Execute()
        {
            try
            {
                var task = procedure.Invoke(input, Token);
                Token.ThrowIfCancellationRequested();
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
