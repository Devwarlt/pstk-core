using System.Threading.Tasks;

namespace CA.Threading.Tasks.Procedures
{
#pragma warning disable CS1591

    public interface IAsyncProcedure
    {
        bool Execute();

        Task<bool> ExecuteAsync();

        ProcedureInfo GetProcedureInfo();
    }

#pragma warning restore CS1591
}