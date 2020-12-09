using System.Threading;

namespace PSTk.Threading.Tasks.Procedures
{
#pragma warning disable

    public interface IAttachedTask
    {
        CancellationToken Token { get; }

        void AttachToParent(CancellationToken token);
    }

#pragma warning restore
}
