using PSTk.Diagnostics.Logging;
using PSTk.Threading.Tasks;

namespace PSTk.Extensions.Utils
{
    /// <summary>
    /// Contains <see cref="InternalRoutineEventArgsExtensions"/> utilities.
    /// </summary>
    public static class InternalRoutineEventArgsExtensions
    {
        /// <summary>
        /// Track message on <see cref="ILog{T}"/> displaying information about <see cref="InternalRoutineEventArgs"/>
        /// event invoked by <see cref="InternalRoutine"/> or <see cref="InternalRoutineSlim"/>.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="args"></param>
        /// <param name="log"></param>
        public static void TrackDelta<T>(this InternalRoutineEventArgs args, ILog<T> log)
            => log.PrintWarn(
                $"[routine: {args.Name}] High delta detected " +
                $"(delta: {args.Delta}, " +
                $"tps: {args.TicksPerSecond}, " +
                $"timeout: {args.Timeout}ms)!"
            );
    }
}
