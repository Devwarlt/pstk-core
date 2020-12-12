using PSTk.Threading.Tasks.Procedures;
using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace PSTk.Threading.Tasks
{
    /// <summary>
    /// Used for synchronous or asynchronous routine.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException"></exception>
    /// <exception cref="ArgumentNullException"></exception>
    /// <exception cref="OperationCanceledException"></exception>
    public sealed class InternalRoutine : IAttachedTask
    {
        private readonly ManualResetEventSlim resetEvent;
        private readonly Action<long, bool> routine;
        private readonly int ticksPerSecond;
        private readonly int timeout;

        private long delta = 0L;
        private bool isCanceled = false;
        private Stopwatch stopwatch;

        /// <summary>
        /// Create a new instance of <see cref="InternalRoutine"/>. This is a simplified version.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="timeout"></param>
        /// <param name="routine"></param>
        public InternalRoutine(string name, int timeout, Action routine)
            : this(name, timeout, (delta) => routine(), null) { }

        /// <summary>
        /// Create a new instance of <see cref="InternalRoutine"/>.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="timeout"></param>
        /// <param name="routine"></param>
        /// <param name="errorLogger"></param>
        public InternalRoutine(string name, int timeout, Action<long> routine, Action<string> errorLogger = null)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentNullException(nameof(name));
            if (timeout <= 0)
                throw new ArgumentOutOfRangeException(nameof(timeout), "Only non-zero and non-negative values are permitted.");
            if (routine == null)
                throw new ArgumentNullException(nameof(routine));

            Name = name;
            this.timeout = timeout;
            this.routine = (delta, cancel) => { if (!cancel) routine.Invoke(delta); };
            ticksPerSecond = 1000 / timeout;
            resetEvent = new ManualResetEventSlim(false);
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

        private event EventHandler<Exception> onError;

        /// <summary>
        /// Get the name of <see cref="InternalRoutine"/>.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Get the <see cref="CancellationToken"/> of attached task.
        /// </summary>
        public CancellationToken Token { get; private set; } = default;

        /// <summary>
        /// Attach a process to parent in case of external task cancellation request.
        /// </summary>
        /// <param name="token"></param>
        public void AttachToParent(CancellationToken token) => Token = token;

        /// <summary>
        /// Initialize and starts the core routine, to stop it must use <see cref="CancellationTokenSource.Cancel(bool)"/>.
        /// </summary>
        public void Start() => Execute(Loop);

        private Task<long> Execute(Action method)
        {
            if (stopwatch == null)
                stopwatch = Stopwatch.StartNew();

            Task<long> task = null;

            if (Token != default)
                try
                {
                    isCanceled = Token.IsCancellationRequested;
                    Token.ThrowIfCancellationRequested();
                    task = Task.Run(() =>
                    {
                        var elapsedMs = stopwatch.ElapsedMilliseconds;
                        method.Invoke();
                        var elapsedMsDelta = stopwatch.ElapsedMilliseconds - elapsedMs;
                        return timeout - elapsedMsDelta;
                    }, Token);
                }
                catch (OperationCanceledException) { Finish(); }
            else
                task = Task.Run(() =>
                {
                    var elapsedMs = stopwatch.ElapsedMilliseconds;
                    method.Invoke();
                    var elapsedMsDelta = stopwatch.ElapsedMilliseconds - elapsedMs;
                    return timeout - elapsedMsDelta;
                });

            task?.ContinueWith(t => onError.Invoke(this, t.Exception.InnerException), TaskContinuationOptions.OnlyOnFaulted);
            return task;
        }

        private void Finish()
        {
            isCanceled = true;
            OnFinished?.Invoke(this, null);
        }

        private void Loop()
        {
            var task = Execute(() => routine.Invoke(delta, isCanceled));
            if (isCanceled || task == null)
                return;

            var result = task.Result < 0 ? 0 : task.Result;
            delta = Math.Max(0, result);
            if (delta > timeout)
                OnDeltaVariation?.Invoke(this, new InternalRoutineEventArgs(Name, delta, ticksPerSecond, timeout));

            resetEvent.Wait((int)delta);
            Loop();
        }
    }
}
