using LoRWatcher.Caches;
using System.Threading.Tasks;

namespace LoRWatcher.Clients
{
    public interface IServiceClient
    {
        Task ReportGameAsync(MatchReport matchReport);
    }
}
