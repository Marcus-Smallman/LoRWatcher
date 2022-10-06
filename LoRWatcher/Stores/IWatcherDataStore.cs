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

        Task<IEnumerable<MatchReport>> GetAllMatchReportsAsync(CancellationToken cancellationToken);

        Task<MatchReportMetadata> SetMatchReportsMetadataAsync(MatchReportMetadata matchReportMetadata, CancellationToken cancellationToken);

        Task<MatchReportMetadata> SetTagLineAsync(string tagLine, CancellationToken cancellationToken);

        Task<MatchReportMetadata> GetMatchReportsMetadataAsync(CancellationToken cancellationToken);

        Task<MatchReportMetadata> GetMatchReportsMetadataV2Async(CancellationToken cancellationToken);

        Task<MatchReportMetadata> UpdateMatchReportsMetadataAsync(MatchReport matchReport, CancellationToken cancellationToken);

        Task<bool> ReportReplayAsync(MatchReport matchReport, CancellationToken cancellationToken);

        Task<SortedList<string, Snapshot>> GetReplayByIdAsync(string id, CancellationToken cancellationToken);

        Task<bool> ClearMatchReplayAsync(MatchReport matchReport, CancellationToken cancellationToken);
    }
}
