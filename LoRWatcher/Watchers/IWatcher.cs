using System.Threading.Tasks;

namespace LoRWatcher.Watchers
{
    public interface IWatcher
    {
        Task StartAsync();
    }
}
