namespace PSTk.Threading.Tasks.Procedures
{
#pragma warning disable

    public interface IAsyncProcedure : IAttachedTask
    {
        bool Execute();
    }

#pragma warning restore
}
