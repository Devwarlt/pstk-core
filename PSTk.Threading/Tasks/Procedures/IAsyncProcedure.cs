namespace PSTk.Threading.Tasks.Procedures
{
#pragma warning disable

    public interface IAsyncProcedure : IAttachedTask
    {
        string Name { get; }

        bool Execute();
    }

#pragma warning restore
}
