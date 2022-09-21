using System.Threading;
using System.Threading.Tasks;

namespace LoRWatcher.Services
{
    public interface IWatcherService
    {
        Task<bool> InitialiseMetadataAsync(CancellationToken cancellationToken);
    }
}
