using System;
using System.Threading;
using System.Threading.Tasks;

namespace CA.Threading.Tasks
{
    /// <summary>
    /// Used for synchronous or asynchronous routine.
    /// </summary>
    /// <exception cref="ArgumentException"></exception>
    /// <exception cref="ArgumentNullException"></exception>
    public sealed class InternalRoutine : IDisposable
    {
        private readonly ManualResetEvent resetEvent;
        private readonly Action<bool> routine;
        private readonly int timeout;

        private bool isCanceled = false;

#pragma warning disable CS1591

        public InternalRoutine(

#pragma warning restore CS1591
            int timeout,
            Action routine
            ) : this(timeout, routine, null) { }

#pragma warning disable CS1591

        public InternalRoutine(

#pragma warning restore CS1591
            int timeout,
            Action routine,
            Action<string> errorLogger
            )
        {
            if (timeout <= 0) throw new ArgumentException("Only positive numbers are allowed.", "timeout");
            if (routine == null) throw new ArgumentNullException("routine", "Routine couldn't be null.");

            this.timeout = timeout;
            this.routine = (cancel) => { if (!cancel) routine.Invoke(); };

            resetEvent = new ManualResetEvent(false);
            onError += (s, e) =>
            {
                errorLogger?.Invoke(e.ToString());
                Stop();
            };
        }

        private event EventHandler<Exception> onError;

#pragma warning disable CS1591

        public void Dispose() => isCanceled = true;

#pragma warning restore CS1591

        /// <summary>
        /// Starts the routine.
        /// </summary>
        public void Start() => Loop();

        /// <summary>
        /// Starts the routine asynchronously.
        /// </summary>
        public void StartAsync() => LoopAsync();

        /// <summary>
        /// Stop the routine.
        /// </summary>
        public void Stop() => Dispose();

        private void Loop()
        {
            Task.Run(() => routine.Invoke(isCanceled)).ContinueWith(t =>
                onError.Invoke(null, t.Exception.InnerException),
                TaskContinuationOptions.OnlyOnFaulted
            );

            if (isCanceled) return;

            Loop();
        }

        private async void LoopAsync()
        {
            await Task.Run(() => routine.Invoke(isCanceled)).ContinueWith(t =>
                onError.Invoke(null, t.Exception.InnerException),
                TaskContinuationOptions.OnlyOnFaulted
            );

            if (isCanceled) return;

            LoopAsync();
        }
    }
}