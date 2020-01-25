using LoRWatcher.Caches;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace LoRWatcher.Stores
{
    public interface IWatcherDataStore
    {
        Task<bool> ReportGameAsync(MatchReport matchReport, CancellationToken cancellationToken);

        Task<IEnumerable<MatchReport>> GetMatchReportsAsync(CancellationToken cancellationToken);
    }
}
