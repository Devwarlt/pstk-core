using System.Threading;

namespace PSTk.Threading.Tasks.Procedures
{
#pragma warning disable

    public interface IAttachedTask
    {
        string Name { get; }

        CancellationToken Token { get; }

        void AttachToParent(CancellationToken token);
    }

#pragma warning restore
}
