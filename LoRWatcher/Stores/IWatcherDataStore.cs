using LoRWatcher.Caches;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace LoRWatcher.Stores
{
    public interface IWatcherDataStore
    {

        Task<bool> ReportGameAsync(MatchReport matchReport, CancellationToken cancellationToken);

        Task<MatchReport> GetMatchReportByIdAsync(string id, CancellationToken cancellationToken);

        Task<IEnumerable<MatchReport>> GetMatchReportsAsync(int skip, int limit, CancellationToken cancellationToken);

        Task<MatchReportMetadata> SetMatchReportsMetadataAsync(MatchReportMetadata matchReportMetadata, CancellationToken cancellationToken);

        Task<MatchReportMetadata> GetMatchReportsMetadataAsync(CancellationToken cancellationToken);

        Task<MatchReportMetadata> GetMatchReportsMetadataV2Async(CancellationToken cancellationToken);

        Task<MatchReportMetadata> UpdateMatchReportsMetadataAsync(MatchReport matchReport, CancellationToken cancellationToken);
    }
}
