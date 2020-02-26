using System;
using System.Threading;
using System.Threading.Tasks;
using wServer.realm.worlds;

namespace ca
{
    /// <summary>
    /// Represents a core world task according <see cref="CoreType"/>.
    /// This algorithm runs an internal task for <see cref="World"/> and
    /// verify if <see cref="World.Manager"/> is running or world isn't deleted yet.
    /// </summary>
    ///
    /// <example>
    ///
    /// Usage of this algorithm:
    /// - add it into <see cref="World"/> class on method <see cref="World.Init"/>
    ///
    /// - proceed properly setup of it adding this algorithm for each procedure
    /// whose require some priority along world loop like tick and tick logic methods
    /// (its also highly recommended to split timers task as another method, see below)
    ///
    /// - another tip is split timers task using <see cref="CoreWorldTask"/> feature:
    ///
    /// <code>
    ///
    ///     var timerCWT = new CoreWorldTask(this,
    ///         () => {
    ///             for (var i = 0; i < Timers.Count; i++)
    ///                 try {
    ///                     if (Timers[i] == null) continue;
    ///                     var timer = Timers[i].Tick(this);
    ///                     if (timer.HasValue && timer == false) continue;
    ///                     Timers.RemoveAt(i);
    ///                     i--;
    ///                 } catch (Exception e) { Log.Error(e); }
    ///         }, CoreType.WorldTimer);
    ///     timerCWT.run();
    ///
    /// </code>
    ///
    /// - doing this you'll make all internal loops of <see cref="World"/> asynchronous
    /// and use processor cores for this parallel programing job
    ///
    /// <para>
    ///
    /// Note: make sure you aren't in a concurrent scenario to avoid thread-unsafe issues,
    /// if you match with some conditions, better not use <see cref="CoreWorldTask"/> to do
    /// such loop task until you properly add <see cref="TimedLock"/> or <see cref="Monitor.TryEnter(object, int)"/>
    /// to avoid possible deadlocks.
    ///
    /// </para>
    ///
    /// </example>
    public sealed class CoreWorldTask
    {
        private readonly Action action;
        private readonly World world;
        private readonly ManualResetEvent mre;
        private readonly CoreType type;
        private bool running;

        public CoreWorldTask(World world, Action action, CoreType type)
        {
            this.world = world;
            this.action = action;
            this.type = type;

            mre = new ManualResetEvent(false);
        }

        public void run()
        {
            if (action == null) throw new ArgumentNullException("Action of CoreWorldTask shouldn't be null.");

            running = true;

            int timeout;

            switch (type)
            {
                case CoreType.World:
                case CoreType.WorldTimer: timeout = (int)CoreConstant.worldTickMs; break;
                case CoreType.WorldLogic: timeout = (int)CoreConstant.worldLogicTickMs; break;
                default: throw new ArgumentException($"Not supported CoreType '{type}' argument.");
            }

            Task.Factory.StartNew(() =>
            {
                do
                {
                    mre.WaitOne(timeout);
                    action.Invoke();
                } while (world.Manager != null && !world.Deleted);
            }, TaskCreationOptions.LongRunning);
        }
    }
}