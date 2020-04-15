namespace CA.Threading.Tasks.Procedures
{
#pragma warning disable

    public interface IAsyncProcedure : IAttachedTask
    {
        string GetName { get; }

        bool Execute();
    }

#pragma warning restore
}
