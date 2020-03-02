using System.Collections.Concurrent;

namespace ca.interfaces
{
    public interface IRealmManager
    {
        bool isTerminating { get; set; }

        ConcurrentDictionary<IClient, IPlayerInfo> getClients();

        IMonitor getConnManager();

        IISManager getISManager();

        IMonitor getMonitor();

        IProgram getProgram();

        void startCores();

        void stopCores();
    }
}