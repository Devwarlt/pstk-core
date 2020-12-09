using System;

namespace PSTk.Threading.Tasks
{
    /// <summary>
    /// Represents current flag of <see cref="AutomatedRestarter"/> routine.
    /// </summary>
    [Flags]
    public enum AutomatedRestarterFlag
    {
        /// <summary>
        /// When routine is idle.
        /// </summary>
        Idle,

        /// <summary>
        /// When routine invoked <see cref="AutomatedRestarter.Start"/>.
        /// </summary>
        Running,

        /// <summary>
        /// When routine invoked <see cref="AutomatedRestarter.Stop"/>.
        /// </summary>
        Stopped
    }
}
