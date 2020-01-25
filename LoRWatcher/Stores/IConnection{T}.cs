namespace LoRWatcher.Stores
{
    public interface IConnection<T>
    {
        T GetConnection();
    }
}
