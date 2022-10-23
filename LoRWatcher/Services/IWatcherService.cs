using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace LoRWatcher.Services
{
    public interface IWatcherService
    {
        Task<bool> InitialiseMetadataAsync(CancellationToken cancellationToken = default);

        Task<bool> SyncMatchReportsAsync(CancellationToken cancellationToken = default);

        Task<IEnumerable<ServiceMatchReport>> GetMatchReportsAsync(int skip, int limit, CancellationToken cancellationToken = default);

        Task<ServiceMatchReport> GetMatchReportByIdAsync(string id, CancellationToken cancellationToken = default);
    }
}
