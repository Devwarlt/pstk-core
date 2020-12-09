using System;
using System.Diagnostics;

namespace PSTk.Profiler
{
    /// <summary>
    /// Prints the total elapsed time in full and in milliseconds that it has taken to call Dispose
    /// until <see cref="TimedProfiler"/> is disposed.
    /// </summary>
    public sealed class TimedProfiler : IDisposable
    {
        /// <summary>
        /// Create a new instance of <see cref="TimedProfiler"/>.
        /// </summary>
        /// <param name="message"></param>
        /// <param name="condition"></param>
        /// <param name="output"></param>
        public TimedProfiler(string message, Func<bool> condition = null, Action<string> output = null)
        {
            Message = message;
            Stopwatch = Stopwatch.StartNew();
            Condition = condition;
            Output = output;
        }

        private Func<bool> Condition { get; }
        private string Message { get; }
        private Action<string> Output { get; }
        private Stopwatch Stopwatch { get; }

        /// <summary>
        /// Called automatically at the end of the scope when used along side a using statment or explicitly called in the code
        /// It will only print out the elapsed time when the condition is met if there is a condition to the desired output if set
        /// otherwise it will log to the console
        /// </summary>
        public void Dispose()
        {
            if (Condition != null && !Condition.Invoke())
                return;

            Stopwatch.Stop();

            var result = $"{Message} | Elapsed: {Stopwatch.Elapsed} ({Stopwatch.ElapsedMilliseconds}ms)";
            Output?.Invoke(result);
            if (Output == null)
                Console.WriteLine(result);
        }
    }
}
