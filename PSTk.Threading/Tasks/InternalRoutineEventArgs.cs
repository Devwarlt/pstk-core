using System;
using System.Threading.Tasks;

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
        /// <param name="delta"></param>
        /// <param name="ticksPerSecond"></param>
        /// <param name="timeout"></param>
        public InternalRoutineEventArgs(long delta, int ticksPerSecond, long timeout)
            : base()
        {
            Delta = delta;
            TicksPerSecond = ticksPerSecond;
            Timeout = timeout;
        }

        /// <summary>
        /// Delta variation of <see cref="InternalRoutine.routine"/>.
        /// </summary>
        public long Delta { get; }

        /// <summary>
        /// Ticks per second for <see cref="InternalRoutine.routine"/> invoke on <see cref="TaskScheduler"/>.
        /// </summary>
        public int TicksPerSecond { get; }

        /// <summary>
        /// Timeout in milliseconds for <see cref="InternalRoutine.routine"/> invoke on <see cref="TaskScheduler"/>.
        /// </summary>
        public long Timeout { get; }
    }
}
