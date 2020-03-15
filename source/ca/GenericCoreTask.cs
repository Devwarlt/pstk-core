using System;
using System.Threading;
using System.Threading.Tasks;

namespace ca
{
    /// <summary>
    /// Represents a generic version of a core task type. Applies to any task that need to
    /// run within a specific condition in a loop cycle (milliseconds).
    /// </summary>
    public sealed class GenericCoreTask
    {
        private readonly Action action;
        private readonly ManualResetEvent mre;
        private readonly int timeout;

        public GenericCoreTask(Action action, int timeout)
             : this(action, timeout, null)
        {
        }

        public GenericCoreTask(Action action, int timeout, Action<Exception> onError)
        {
            this.action = action;
            this.timeout = timeout;

            mre = new ManualResetEvent(false);

            if (onError != null) errorHandler += (sender, exception) => onError.Invoke(exception);
            else errorHandler += (sender, exception) => { };
        }

        private event EventHandler<Exception> errorHandler;

        public void run()
        {
            if (action == null) throw new ArgumentNullException("Action of GenericCoreTask shouldn't be null.");

            Task.Factory.StartNew(() =>
            {
                do
                {
                    mre.WaitOne(timeout);
                    action.Invoke();
                } while (true);
            },
                TaskCreationOptions.LongRunning)
            .ContinueWith(task => errorHandler.Invoke(null, task.Exception.InnerException),
                TaskContinuationOptions.OnlyOnFaulted);
        }
    }
}