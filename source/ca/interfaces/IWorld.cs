namespace ca.interfaces
{
    public interface IWorld
    {
        IRealmManager getManager();

        void initTasks();

        bool isDeleted();
    }
}