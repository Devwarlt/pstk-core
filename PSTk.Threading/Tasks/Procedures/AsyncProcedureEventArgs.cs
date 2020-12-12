using System;

namespace PSTk.Threading.Tasks.Procedures
{
    /// <summary>
    /// Contains result of <see cref="AsyncProcedure{TInput}"/> event.
    /// </summary>
    public sealed class AsyncProcedureEventArgs<TInput> : EventArgs
    {
        /// <summary>
        /// Create a new instance of <see cref="AsyncProcedureEventArgs{TInput}"/>.
        /// </summary>
        /// <param name="input"></param>
        /// <param name="result"></param>
        public AsyncProcedureEventArgs(TInput input, bool result) : base()
        {
            Input = input;
            Result = result;
        }

        /// <summary>
        /// The input of <see cref="AsyncProcedure{TInput}"/> event.
        /// </summary>
        public TInput Input { get; }

        /// <summary>
        /// The result of <see cref="AsyncProcedure{TInput}"/> event.
        /// </summary>
        public bool Result { get; }
    }
}
