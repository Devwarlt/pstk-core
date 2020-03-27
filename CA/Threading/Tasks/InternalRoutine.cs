using CA.Threading.Tasks.Procedures;
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
    /// <exception cref="OperationCanceledException"></exception>
    public sealed class InternalRoutine : IAttachedTask
    {
        private readonly ManualResetEvent resetEvent;
        private readonly Action<InternalRoutine, bool> routine;
        private readonly int timeout;

        private bool isCanceled = false;

#pragma warning disable

        private CancellationToken token = default;

        public InternalRoutine(
            int timeout,
            Action routine
            ) : this(timeout, (nternalRoutine) => routine(), null) { }

        public InternalRoutine(
            int timeout,
            Action<InternalRoutine> routine,
            Action<string> errorLogger = null
            )

#pragma warning restore

        {
            if (timeout <= 0) throw new ArgumentException("Only positive numbers are allowed.", "timeout");
            if (routine == null) throw new ArgumentNullException("routine");

            this.timeout = timeout;
            this.routine = (instance, cancel) => { if (!cancel) routine.Invoke(instance); };

            resetEvent = new ManualResetEvent(false);
            onError += (s, e) =>
            {
                errorLogger?.Invoke(e.ToString());
                Finish();
            };
        }

        /// <summary>
        /// When routine finished its task.
        /// </summary>
        public event EventHandler onFinished;

        /// <summary>
        /// When routine is already initialized.
        /// </summary>
        public event EventHandler onInitialized;

        /// <summary>
        /// When routine is preparing to initialize.
        /// </summary>
        public event EventHandler onInitializing;

        private event EventHandler<Exception> onError;

        /// <summary>
        /// Get the <see cref="CancellationToken"/> of attached task.
        /// </summary>
        public CancellationToken GetToken => token;

        /// <summary>
        /// Attach a process to parent in case of external task
        /// cancellation request.
        /// </summary>
        /// <param name="token"></param>
        public void AttachToParent(CancellationToken token) => this.token = token;

        /// <summary>
        /// Initialize and starts the core routine, to stop it must use
        /// <see cref="CancellationTokenSource.Cancel(bool)"/>.
        /// </summary>
        public void Start() => Initialize(Loop, true);

        private void Finish()
        {
            isCanceled = true;
            onFinished?.Invoke(this, null);
        }

        private void Initialize(Action method, bool isInitializing)
        {
            if (token != default)
                try
                {
                    isCanceled = token.IsCancellationRequested;

                    token.ThrowIfCancellationRequested();

                    Task.Run(() =>
                    {
                        if (isInitializing) onInitializing?.Invoke(this, null);

                        method.Invoke();

                        if (isInitializing) onInitialized?.Invoke(this, null);
                    }, token).ContinueWith(t =>
                        onError.Invoke(null, t.Exception.InnerException),
                        TaskContinuationOptions.OnlyOnFaulted
                    );
                }
                catch (OperationCanceledException) { Finish(); }
            else
                Task.Run(() =>
                {
                    if (isInitializing) onInitializing?.Invoke(this, null);

                    method.Invoke();

                    if (isInitializing) onInitialized?.Invoke(this, null);
                }).ContinueWith(t =>
                    onError.Invoke(null, t.Exception.InnerException),
                    TaskContinuationOptions.OnlyOnFaulted
                );
        }

        private void Loop()
        {
            Initialize(() => routine.Invoke(this, isCanceled), false);

            if (isCanceled) return;

            resetEvent.WaitOne(timeout);

            Loop();
        }
    }
}