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
        private readonly Action<InternalRoutine, bool> routine;
        private readonly int timeout;

        private bool isCanceled = false;

#pragma warning disable CS1591

        public InternalRoutine(

#pragma warning restore CS1591
            int timeout,
            Action<InternalRoutine> routine
            ) : this(timeout, routine, null) { }

#pragma warning disable CS1591

        public InternalRoutine(

#pragma warning restore CS1591
            int timeout,
            Action<InternalRoutine> routine,
            Action<string> errorLogger
            )
        {
            if (timeout <= 0) throw new ArgumentException("Only positive numbers are allowed.", "timeout");
            if (routine == null) throw new ArgumentNullException("routine", "Routine couldn't be null.");

            this.timeout = timeout;
            this.routine = (instance, cancel) => { if (!cancel) routine.Invoke(instance); };

            resetEvent = new ManualResetEvent(false);
            onError += (s, e) =>
            {
                errorLogger?.Invoke(e.ToString());
                Stop();
            };
        }

        private event EventHandler<Exception> onError;

        /// <summary>
        /// Check if current routine still running in background.
        /// </summary>
        public bool IsRunning => !isCanceled;

#pragma warning disable CS1591

        public void Dispose() => isCanceled = true;

#pragma warning restore CS1591

        /// <summary>
        /// Starts the routine.
        /// </summary>
        public void Start()
            => Task.Factory.StartNew(() => Loop(),
                TaskCreationOptions.LongRunning)
            .ContinueWith(t => onError.Invoke(null, t.Exception.InnerException),
                TaskContinuationOptions.OnlyOnFaulted);

        /// <summary>
        /// Starts the routine asynchronously.
        /// </summary>
        public void StartAsync()
            => Task.Factory.StartNew(() => LoopAsync(),
                TaskCreationOptions.LongRunning)
            .ContinueWith(t => onError.Invoke(null, t.Exception.InnerException),
                TaskContinuationOptions.OnlyOnFaulted);

        /// <summary>
        /// Stop the routine.
        /// </summary>
        public void Stop() => Dispose();

        private void Loop()
        {
            Task.Run(() => routine.Invoke(this, isCanceled)).ContinueWith(t =>
                onError.Invoke(null, t.Exception.InnerException),
                TaskContinuationOptions.OnlyOnFaulted
            );

            if (isCanceled) return;

            resetEvent.WaitOne(timeout);

            Loop();
        }

        private async void LoopAsync()
        {
            await Task.Run(() => routine.Invoke(this, isCanceled)).ContinueWith(t =>
                onError.Invoke(null, t.Exception.InnerException),
                TaskContinuationOptions.OnlyOnFaulted
            );

            if (isCanceled) return;

            resetEvent.WaitOne(timeout);

            LoopAsync();
        }
    }
}