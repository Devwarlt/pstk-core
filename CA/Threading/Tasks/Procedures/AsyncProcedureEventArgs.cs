using System;

namespace CA.Threading.Tasks.Procedures
{
    /// <summary>
    /// Contains result of <see cref="AsyncProcedure{TInput}"/> event.
    /// </summary>
    public class AsyncProcedureEventArgs<TInput> : EventArgs
    {
#pragma warning disable

        public AsyncProcedureEventArgs(TInput input, bool result)
            : base()
        {
            Input = input;
            Result = result;
        }

#pragma warning restore

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