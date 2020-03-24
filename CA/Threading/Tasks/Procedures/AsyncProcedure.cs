using System;
using System.Threading.Tasks;

namespace CA.Threading.Tasks.Procedures
{
    /// <summary>
    /// Used for situations that require dependency between other procedure.
    /// Recommended to use it with <see cref="AsyncProcedurePool"/>.
    /// </summary>
    /// <typeparam name="TInput"></typeparam>
    public sealed class AsyncProcedure<TInput> : IAsyncProcedure
    {
        private readonly TInput input;
        private readonly string name;
        private readonly Func<string, TInput, Task<bool>> procedure;
        private readonly Action whenCanceled;

        private bool isCanceled;
        private string message;

#pragma warning disable CS1591

        public AsyncProcedure(

#pragma warning restore CS1591
            string name,
            TInput input,
            Func<string, TInput, Task<bool>> procedure,
            Action whenCanceled = null
            )
        {
            this.name = name;
            this.whenCanceled = whenCanceled;
            this.procedure = procedure;

            message = string.Empty;
            isCanceled = false;
        }

        /// <summary>
        /// Execute the procedure.
        /// </summary>
        /// <returns></returns>
        public bool Execute()
        {
            var task = procedure.Invoke(message, input);
            var result = task.Result;

            isCanceled = !result;

            if (!result && whenCanceled != null) whenCanceled.Invoke();

            return result;
        }

        /// <summary>
        /// Execute the procedure asynchronously.
        /// </summary>
        /// <returns></returns>
        public async Task<bool> ExecuteAsync()
        {
            var asyncTask = Task.Run(() => procedure.Invoke(message, input));

            await asyncTask;

            var result = asyncTask.Result;

            isCanceled = !result;

            if (!result && whenCanceled != null) whenCanceled.Invoke();

            return result;
        }

        /// <summary>
        /// Gets the current <see cref="ProcedureInfo"/> state information.
        ///
        /// <para>
        /// Note: values could be updated while procedure is running.
        /// </para>
        /// </summary>
        /// <returns></returns>
        public ProcedureInfo GetProcedureInfo()
            => new ProcedureInfo
            {
                Name = name,
                Message = message
            };
    }
}