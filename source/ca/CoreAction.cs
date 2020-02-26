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

        public void runTask()
        {
            if (task == null || task.IsCompleted || task.IsFaulted)
                task = Task.Factory.StartNew(action);

            /*
             * Consider to remove manually all RealmTime references of
             * project to proceed with this installation of CA on your
             * NR-Core project.
             */
            /* .ContinueWith(exception => Program.Log.Fatal(exception.Exception.InnerException),
                TaskContinuationOptions.OnlyOnFaulted); */
        }
    }
}