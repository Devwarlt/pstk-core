namespace ca.interfaces
{
    public interface IClient
    {
        void disconnect(string message);

        IPlayer getPlayer();
    }
}