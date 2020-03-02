using log4net;
using System;
using System.Threading.Tasks;

namespace ca
{
    /// <summary>
    /// Represents a core action used on <see cref="CoreThread"/>.
    /// </summary>
    public struct CoreAction
    {
        private readonly Action action;
        private Task task;

        public CoreAction(Action action)
        {
            this.action = action;

            task = null;
        }

        public void runTask(ILog log = null)
        {
            if (task == null || task.IsCompleted || task.IsFaulted)
                task = Task.Factory.StartNew(action)
                    .ContinueWith(exception => log?.Fatal(exception.Exception.InnerException),
                        TaskContinuationOptions.OnlyOnFaulted);
        }
    }
}