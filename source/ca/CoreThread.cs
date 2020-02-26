using log4net;
using System.Threading;
using wServer.realm;

namespace ca
{
    /// <summary>
    /// Represents a core thread according <see cref="CoreType"/>.
    /// This algorithm runs a thread with such priority and background or not.
    /// However, this algorithm is designed for procedures that require highest priority,
    /// based on <see cref="CoreType"/> rather than <see cref="CoreWorldTask"/> which runs
    /// with low priority and doesn't require such high attention by processor cores of
    /// current server environment.
    /// </summary>
    ///
    /// <example>
    ///
    /// Usage of this algorithm:
    /// - any thread that require priority and runs synchronously (concurrent scenario)
    /// will fit with conditions of <see cref="CoreThread"/>
    ///
    /// - if you do use it very well, you'll not see any thread-unsafe issue, see below an
    /// example of it:
    ///
    /// <code>
    ///
    ///     /* asuming <see cref="CoreThread"/> is running on <see cref="RealmManager"/> */
    ///
    ///
    ///     var ct = new CoreThread(
    ///         CoreType.Monitor,
    ///         new CoreAction(() => Console.WriteLine("Monitor task: repeats every 1000 ms.")),
    ///         this,
    ///         1000,
    ///         false);
    ///     /* once this method is invoked, it'll invoke <see cref="action"/> every
    ///     <see cref="delay"/>, repeating this process until its over (synchronously) */
    ///     ct.start();
    ///
    ///     /* to stop the cycle must invoke <see cref="stop(bool)"/> method */
    ///     ct.stop();
    ///
    /// </code>
    ///
    /// </example>
    public sealed class CoreThread
    {
        private readonly CoreAction action;
        private readonly CoreType coreType;
        private readonly int delay;
        private readonly ILog log;
        private readonly RealmManager manager;
        private readonly ManualResetEvent resetEvent;
        private readonly Thread thread;

        public CoreThread(CoreType coreType, CoreAction action, RealmManager manager, int delay, bool isBackground)
            : this(null, coreType, action, manager, delay, isBackground)
        {
        }

        public CoreThread(ILog log, CoreType coreType, CoreAction action, RealmManager manager, int delay, bool isBackground)
            : this(log, coreType, action, manager, delay, isBackground, isBackground ? ThreadPriority.Normal : ThreadPriority.AboveNormal)
        {
        }

        public CoreThread(ILog log, CoreType coreType, CoreAction action, RealmManager manager, int delay, bool isBackground, ThreadPriority priority)
        {
            this.coreType = coreType;
            this.delay = delay;
            this.log = log;
            this.manager = manager;
            this.action = action;

            resetEvent = new ManualResetEvent(false);
            thread = new Thread(new ThreadStart(() =>
            {
                do
                {
                    action.runTask();
                    resetEvent.WaitOne(delay);
                } while (!manager.Terminating || isAlive());
            }))
            {
                IsBackground = isBackground,
                Priority = priority
            };
        }

        public CoreType getCoreType() => coreType;

        public bool isAlive() => thread.IsAlive;

        public void start(bool notifyOnLog = true)
        {
            if (log != null && notifyOnLog) log.InfoFormat("Starting {0} core thread...", coreType);

            thread.Start();
        }

        public void stop(bool notifyOnLog = true)
        {
            if (!thread.IsAlive) return;
            if (log != null && notifyOnLog) log.InfoFormat("Stopping {0} core thread...", coreType);

            thread.Abort();
        }
    }
}