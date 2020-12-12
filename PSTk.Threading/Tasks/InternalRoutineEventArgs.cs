using System;

namespace PSTk.Threading.Tasks
{
    /// <summary>
    /// Contains event arguments from <see cref="InternalRoutine.OnDeltaVariation"/> event.
    /// </summary>
    public class InternalRoutineEventArgs : EventArgs
    {
        /// <summary>
        /// Create a new instance of <see cref="InternalRoutineEventArgs"/>.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="delta"></param>
        /// <param name="ticksPerSecond"></param>
        /// <param name="timeout"></param>
        public InternalRoutineEventArgs(string name, long delta, int ticksPerSecond, long timeout)
            : base()
        {
            Name = name;
            Delta = delta;
            TicksPerSecond = ticksPerSecond;
            Timeout = timeout;
        }

        /// <summary>
        /// Get delta variation of <see cref="InternalRoutine"/> raised on last tick execution.
        /// </summary>
        public long Delta { get; }

        /// <summary>
        /// Get name of <see cref="InternalRoutine"/>.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Get ticks per second or number of executions per 1000ms used by <see cref="InternalRoutine"/> to execute its internal task.
        /// </summary>
        public int TicksPerSecond { get; }

        /// <summary>
        /// Get timeout in milliseconds used by <see cref="InternalRoutine"/> to execute its internal task.
        /// </summary>
        public long Timeout { get; }
    }
}
