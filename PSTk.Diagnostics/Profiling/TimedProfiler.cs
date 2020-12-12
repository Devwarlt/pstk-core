using System;
using System.Diagnostics;

namespace PSTk.Diagnostics
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
            this.message = message;
            this.condition = condition;
            this.output = output;
            stopwatch = Stopwatch.StartNew();
        }

        private Func<bool> condition { get; }
        private string message { get; }
        private Action<string> output { get; }
        private Stopwatch stopwatch { get; }

        /// <summary>
        /// Called automatically at the end of the scope when used along side a using statment or explicitly called in the code
        /// It will only print out the elapsed time when the condition is met if there is a condition to the desired output if set
        /// otherwise it will log to the console
        /// </summary>
        public void Dispose()
        {
            if (condition != null && !condition.Invoke())
                return;

            stopwatch.Stop();

            var result = $"{message} | Elapsed: {stopwatch.Elapsed} ({stopwatch.ElapsedMilliseconds}ms)";
            output?.Invoke(result);
            if (output == null)
                Console.WriteLine(result);
        }
    }
}
