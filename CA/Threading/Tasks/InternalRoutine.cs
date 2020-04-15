using CA.Threading.Tasks.Procedures;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace CA.Threading.Tasks
{
    /// <summary>
    /// Used for synchronous or asynchronous routine.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException"></exception>
    /// <exception cref="ArgumentNullException"></exception>
    /// <exception cref="OperationCanceledException"></exception>
    public sealed class InternalRoutine : IAttachedTask
    {
        private readonly ManualResetEvent resetEvent;
        private readonly Action<InternalRoutine, bool> routine;
        private readonly int ticksPerSecond;
        private readonly int timeout;

        private int delta = 0;
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
            if (timeout <= 0) throw new ArgumentOutOfRangeException("timeout", "Only non-zero and non-negative values are permitted.");
            if (routine == null) throw new ArgumentNullException("routine");

            this.timeout = timeout;
            this.routine = (instance, cancel) => { if (!cancel) routine.Invoke(instance); };

            ticksPerSecond = 1000 / timeout;
            resetEvent = new ManualResetEvent(false);
            onError += (s, e) =>
            {
                errorLogger?.Invoke(e.ToString());
                Finish();
            };
        }

        /// <summary>
        /// When routine <see cref="timeout"/> takes more time than usual to execute.
        /// </summary>
        public event EventHandler<InternalRoutineEventArgs> OnDeltaVariation;

        /// <summary>
        /// When routine finished its task.
        /// </summary>
        public event EventHandler OnFinished;

        /// <summary>
        /// When routine is already initialized.
        /// </summary>
        public event EventHandler OnInitialized;

        /// <summary>
        /// When routine is preparing to initialize.
        /// </summary>
        public event EventHandler OnInitializing;

        private event EventHandler<Exception> onError;

        /// <summary>
        /// Get the <see cref="CancellationToken"/> of attached task.
        /// </summary>
        public CancellationToken GetToken => token;

        /// <summary>
        /// Attach a process to parent in case of external task cancellation request.
        /// </summary>
        /// <param name="token"></param>
        public void AttachToParent(CancellationToken token) => this.token = token;

        /// <summary>
        /// Initialize and starts the core routine, to stop it must use <see cref="CancellationTokenSource.Cancel(bool)"/>.
        /// </summary>
        public void Start() => Execute(Loop, true);

        private Task<int> Execute(Action method, bool isInitializing)
        {
            Task<int> task = null;

            if (token != default)
                try
                {
                    isCanceled = token.IsCancellationRequested;

                    token.ThrowIfCancellationRequested();

                    task = Task.Run(() =>
                    {
                        if (isInitializing) OnInitializing?.Invoke(this, null);

                        var elapsedMs = Environment.TickCount;

                        method.Invoke();

                        var elapsedMsDelta = Environment.TickCount - elapsedMs;

                        if (isInitializing) OnInitialized?.Invoke(this, null);

                        return timeout - elapsedMsDelta;
                    }, token);
                }
                catch (OperationCanceledException) { Finish(); }
            else
                task = Task.Run(() =>
                {
                    if (isInitializing) OnInitializing?.Invoke(this, null);

                    var elapsedMs = Environment.TickCount;

                    method.Invoke();

                    var elapsedMsDelta = Environment.TickCount - elapsedMs;

                    if (isInitializing) OnInitialized?.Invoke(this, null);

                    return timeout - elapsedMsDelta;
                });

            task?.ContinueWith(t => onError.Invoke(null, t.Exception.InnerException), TaskContinuationOptions.OnlyOnFaulted);

            return task;
        }

        private void Finish()
        {
            isCanceled = true;
            OnFinished?.Invoke(this, null);
        }

        private void Loop()
        {
            var task = Execute(() => routine.Invoke(this, isCanceled), false);

            if (isCanceled || task == null) return;

            if (task.Result < 0) delta = Math.Abs(task.Result);

            if (delta > 0) OnDeltaVariation?.Invoke(this, new InternalRoutineEventArgs(delta, ticksPerSecond, timeout));

            delta = 0;

            resetEvent.WaitOne(Math.Max(0, task.Result));

            Loop();
        }
    }
}
