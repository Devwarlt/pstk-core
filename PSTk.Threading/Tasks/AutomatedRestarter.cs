using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;

namespace PSTk.Threading.Tasks
{
    /// <summary>
    /// Creates an <see cref="InternalRoutine"/> adapted to handle events and execute a process when threshold is achieved.
    /// </summary>
    /// <exception cref="ArgumentNullException"></exception>
    /// <exception cref="ArgumentOutOfRangeException"></exception>
    /// <exception cref="InvalidOperationException"></exception>
    public sealed class AutomatedRestarter
    {
        private readonly int routineMs;
        private readonly CancellationTokenSource source;
        private readonly TimeSpan timeout;

        private long finalTickCount;
        private long initialTickCount;
        private List<AutomatedRestarterListener> listeners;
        private InternalRoutine routine;

        /// <summary>
        /// Create a new instance of <see cref="AutomatedRestarter"/>.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="timeout"></param>
        /// <param name="routineMs"></param>
        /// <param name="errorLogger"></param>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public AutomatedRestarter(string name, TimeSpan timeout, int routineMs = 1000, Action<string> errorLogger = null)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentNullException(nameof(name));
            if (routineMs <= 0)
                throw new ArgumentOutOfRangeException(nameof(routineMs), "Only non-zero and non-negative values are permitted.");
            if (timeout == null)
                throw new ArgumentNullException(nameof(timeout));

            Name = name;
            this.routineMs = routineMs;
            this.timeout = timeout;
            source = new CancellationTokenSource();
            listeners = new List<AutomatedRestarterListener>();
            onError += (s, e) => errorLogger?.Invoke(e.ToString());
        }

        /// <summary>
        /// When routine finished its task.
        /// </summary>
        public event EventHandler OnFinished;

        private event EventHandler<Exception> onError;

        /// <summary>
        /// Get the current <see cref="AutomatedRestarter"/> flag.
        /// </summary>
        public AutomatedRestarterFlag GetFlag { get; private set; } = AutomatedRestarterFlag.Idle;

        /// <summary>
        /// Get name of <see cref="AutomatedRestarter"/>.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Adds a new event into listeners.
        /// </summary>
        /// <param name="timeout"></param>
        /// <param name="action"></param>
        /// <exception cref="InvalidOperationException"></exception>
        public void AddEventListener(TimeSpan timeout, Action action)
        {
            switch (GetFlag)
            {
                default: break;
                case AutomatedRestarterFlag.Running | AutomatedRestarterFlag.Stopped:
                    throw new InvalidOperationException($"You cannot perform new listener addition when in {GetFlag} mode.");
            }

            var entry = new AutomatedRestarterListener
            {
                Timeout = timeout,
                Handler = action
            };
            if (entry.IsInvalid)
                throw new InvalidOperationException("The listener is invalid.");

            listeners.Add(entry);
        }

        /// <summary>
        /// Adds a collection of events into listeners.
        /// </summary>
        /// <param name="listeners"></param>
        /// <exception cref="InvalidOperationException"></exception>
        public void AddEventListeners(KeyValuePair<TimeSpan, Action>[] listeners)
        {
            try
            {
                switch (GetFlag)
                {
                    default: break;
                    case AutomatedRestarterFlag.Running | AutomatedRestarterFlag.Stopped:
                        throw new InvalidOperationException($"You cannot perform new listener addition when in {GetFlag} mode.");
                }

                foreach (var listener in listeners)
                {
                    var entry = new AutomatedRestarterListener
                    {
                        Timeout = listener.Key,
                        Handler = listener.Value
                    };
                    if (entry.IsInvalid)
                        throw new InvalidOperationException("The listener is invalid.");

                    this.listeners.Add(entry);
                }
            }
            catch (InvalidOperationException e) { onError.Invoke(this, e); }
        }

        /// <summary>
        /// Start the core routine.
        /// </summary>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="InvalidOperationException"></exception>
        public void Start()
        {
            try
            {
                if (routine != null || GetFlag == AutomatedRestarterFlag.Running)
                    throw new InvalidOperationException("AutomatedRestarter instance is already running.");
                if (OnFinished == null)
                    throw new ArgumentNullException("OnFinished", "Event must be raised once routine finish its execution.");

                listeners = listeners
                    .OrderByDescending(listener => listener.Timeout.TotalMilliseconds)
                    .ToList();

                var isCompleted = false;
                var stopwatch = Stopwatch.StartNew();
                routine = new InternalRoutine(Name, routineMs, () =>
                {
                    if (stopwatch.ElapsedMilliseconds >= finalTickCount && !isCompleted)
                    {
                        isCompleted = true;
                        Stop(true);
                        return;
                    }

                    if (listeners.Count > 0)
                        for (var i = 0; i < listeners.Count; i++)
                        {
                            var timeout = finalTickCount - listeners[i].Timeout.TotalMilliseconds;
                            if (stopwatch.ElapsedMilliseconds >= timeout)
                            {
                                listeners[i].Handler.Invoke();
                                listeners.RemoveAt(i);
                                break;
                            }
                        }
                });
                routine.AttachToParent(source.Token);
                initialTickCount = stopwatch.ElapsedMilliseconds;
                finalTickCount = initialTickCount + (long)timeout.TotalMilliseconds;
                routine.Start();
                GetFlag = AutomatedRestarterFlag.Running;
            }
            catch (Exception e)
            {
                if (e is InvalidCastException || e is ArgumentNullException)
                    onError.Invoke(null, e);
            }
        }

        /// <summary>
        /// Stop the core routine, make sure <paramref name="isFinished"/> is used internally to invoke event <see cref="OnFinished"/>.
        /// </summary>
        /// <param name="isFinished"></param>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="InvalidOperationException"></exception>
        public void Stop(bool isFinished = false)
        {
            try
            {
                if (source.IsCancellationRequested || GetFlag == AutomatedRestarterFlag.Stopped)
                    throw new InvalidOperationException("AutomatedRestarter instance is already stopped.");
                if (OnFinished == null)
                    throw new ArgumentNullException("OnFinished", "Event must be raised once routine finish its execution.");

                source.Cancel();

                if (isFinished)
                    OnFinished.Invoke(this, null);

                GetFlag = AutomatedRestarterFlag.Stopped;
            }
            catch (Exception e)
            {
                if (e is InvalidCastException || e is ArgumentNullException)
                    onError.Invoke(null, e);
            }
        }
    }
}
