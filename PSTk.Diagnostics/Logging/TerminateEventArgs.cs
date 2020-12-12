using System;

namespace PSTk.Diagnostics.Logging
{
    /// <summary>
    /// Contains event arguments from <see cref="LogSlim.OnTerminate"></see> event.
    /// </summary>
    public sealed class TerminateEventArgs : EventArgs
    {
        /// <summary>
        /// Create a new instance of <see cref="TerminateEventArgs"/>.
        /// </summary>
        /// <param name="message"></param>
        public TerminateEventArgs(string message)
            : base()
            => Message = message;

        /// <summary>
        /// Gets message from <see cref="Log"/> before termination.
        /// </summary>
        public string Message { get; }
    }
}
