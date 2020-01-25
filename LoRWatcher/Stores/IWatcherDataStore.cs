using LoRWatcher.Caches;
using System.Threading.Tasks;

namespace LoRWatcher.Stores
{
    public interface IWatcherDataStore
    {
        Task<bool> ReportGameAsync(MatchReport matchReport);
    }
}
