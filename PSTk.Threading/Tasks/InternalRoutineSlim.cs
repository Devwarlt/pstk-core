using PSTk.Threading.Tasks.Procedures;
using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace PSTk.Threading.Tasks
{
    /// <summary>
    /// This class is a lightweight version of <see cref="InternalRoutine"/>. Most of exist
    /// features part of implementation from <see cref="InternalRoutineSlim"/> got revised
    /// and optimized to avoid extra cost for performance.
    /// <para>
    /// <see cref="InternalRoutineSlim"/> runs <see cref="TickCore"/> process on a <see cref="Task"/>
    /// attached on main <see cref="Thread"/> to avoid thread-unsafe issues.
    /// </para>
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException"></exception>
    /// <exception cref="ArgumentNullException"></exception>
    /// <exception cref="OperationCanceledException"></exception>
    public sealed class InternalRoutineSlim : IAttachedTask
    {
        private readonly Action<long> routine;
        private readonly int timeout;

        private Task<bool> coreTask;
        private Stopwatch stopwatch;
        private int ticksPerSecond;

        /// <summary>
        /// Create a new instance of <see cref="InternalRoutineSlim"/> without log tracking.
        /// </summary>
        /// <param name="timeout"></param>
        /// <param name="routine"></param>
        public InternalRoutineSlim(int timeout, Action routine)
            : this(timeout, routine, null)
        { }

        /// <summary>
        /// Create a new instance of <see cref="InternalRoutineSlim"/> with log tracking.
        /// </summary>
        /// <param name="timeout"></param>
        /// <param name="routine"></param>
        /// <param name="errorLogger"></param>
        public InternalRoutineSlim(int timeout, Action routine, Action<string> errorLogger = null)
        {
            if (timeout <= 0)
                throw new ArgumentOutOfRangeException(nameof(timeout), "Only non-zero and non-negative values are permitted.");
            if (routine == null)
                throw new ArgumentNullException(nameof(routine));

            this.timeout = timeout;
            this.routine = (delta) => routine.Invoke();
            onError += (s, e) =>
            {
                errorLogger?.Invoke(e.ToString());
                Finish();
            };
        }

        /// <summary>
        /// Create a new instance of <see cref="InternalRoutineSlim"/> with log and delta per tick tracking.
        /// </summary>
        /// <param name="timeout"></param>
        /// <param name="routine"></param>
        /// <param name="errorLogger"></param>
        public InternalRoutineSlim(int timeout, Action<long> routine, Action<string> errorLogger = null)
        {
            if (timeout <= 0)
                throw new ArgumentOutOfRangeException(nameof(timeout), "Only non-zero and non-negative values are permitted.");

            this.timeout = timeout;
            this.routine = routine ?? throw new ArgumentNullException(nameof(routine));
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
        /// Gets the elapsed delta time per tick loop. This is the delta variation of <see cref="RoutineLoop"/>.
        /// </summary>
        public long Delta { get; private set; } = 0L;

        /// <summary>
        /// Gets if <see cref="TickCore"/> is canceled by <see cref="Token"/>.
        /// </summary>
        public bool IsCanceled { get; private set; } = false;

        /// <summary>
        /// Gets if <see cref="TickCore"/> is running.
        /// </summary>
        public bool IsRunning { get; private set; } = false;

        /// <summary>
        /// Get the <see cref="CancellationToken"/> of attached task.
        /// </summary>
        public CancellationToken Token { get; private set; } = default;

        /// <summary>
        /// Gets total <see cref="Stopwatch.ElapsedMilliseconds"/> since <see cref="Start"/> of <see cref="TickCore"/>.
        /// </summary>
        public long TotalElapsedMilliseconds => stopwatch.ElapsedMilliseconds;

        /// <summary>
        /// Attach a process to parent in case of external task cancellation request.
        /// </summary>
        /// <param name="token"></param>
        public void AttachToParent(CancellationToken token) => Token = token;

        /// <summary>
        /// Initialize and starts the <see cref="TickCore"/>.
        /// </summary>
        public void Start()
        {
            if (IsCanceled || IsRunning)
                return;

            TickCore();
        }

        /// <summary>
        /// Stop on next tick of execution the <see cref="TickCore"/>.
        /// </summary>
        public void Stop()
        {
            IsRunning = false;
            IsCanceled = false;
        }

        private void Finish()
        {
            IsCanceled = true;
            OnFinished?.Invoke(this, null);
        }

        private bool RoutineLoop()
        {
            routine.Invoke(Delta = Math.Max(0, Math.Abs(Delta - stopwatch.ElapsedMilliseconds)));
            if (Delta > timeout)
                OnDeltaVariation?.Invoke(this, new InternalRoutineEventArgs(Delta, ticksPerSecond, timeout));
            return IsRunning && !IsCanceled;
        }

        private async void TickCore()
        {
            ticksPerSecond = 1000 / timeout;
            stopwatch = Stopwatch.StartNew();
            IsRunning = true;
            if (Token == default)
                coreTask = Task.Run(() =>
                {
                    while (RoutineLoop()) ;
                    return true;
                });
            else
                coreTask = Task.Run(() =>
                {
                    try
                    {
                        while (RoutineLoop())
                        {
                            IsCanceled = Token.IsCancellationRequested;
                            Token.ThrowIfCancellationRequested();
                        }
                        return true;
                    }
                    catch (OperationCanceledException) { Finish(); }
                    return false;
                }, Token);

            coreTask?.ContinueWith(t =>
                onError.Invoke(this, t.Exception.InnerException),
                TaskContinuationOptions.OnlyOnFaulted
            );
            IsRunning = await coreTask;
            stopwatch.Stop();
        }
    }
}
