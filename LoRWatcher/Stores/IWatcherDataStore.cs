using LoRWatcher.Caches;
using LoRWatcher.Stores.Documents;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace LoRWatcher.Stores
{
    public interface IWatcherDataStore
    {

        Task<bool> ReportGameAsync(MatchReport matchReport, CancellationToken cancellationToken = default);

        Task<MatchReport> GetMatchReportByIdAsync(string id, CancellationToken cancellationToken = default);

        Task<IEnumerable<MatchReport>> GetMatchReportsAsync(
            int skip,
            int limit,
            string opponentNameFilter = null,
            int opponentNameSortDirection = 0,
            string resultFilter = null,
            int resultSortDirection = 0,
            string regionsFilter = null,
            int regionsSortDirection = 0,
            string gameTypeFilter = null,
            int gameTypeSortDirection = 0,
            CancellationToken cancellationToken = default);

        Task<bool> UpdateGameTypeAsync(string id, string gameType, CancellationToken cancellationToken = default);

        Task<IEnumerable<MatchReport>> GetAllMatchReportsAsync(CancellationToken cancellationToken = default);

        Task<MatchReportMetadata> SetMatchReportsMetadataAsync(MatchReportMetadata matchReportMetadata, CancellationToken cancellationToken = default);

        Task<MatchReportMetadata> SetTagLineAsync(string tagLine, CancellationToken cancellationToken = default);

        Task<MatchReportMetadata> GetMatchReportsMetadataAsync(CancellationToken cancellationToken = default);

        Task<MatchReportMetadata> GetMatchReportsMetadataV2Async(CancellationToken cancellationToken = default);

        Task<MatchReportMetadata> UpdateMatchReportsMetadataAsync(MatchReport matchReport, CancellationToken cancellationToken = default);

        Task<bool> ReportReplayAsync(MatchReport matchReport, CancellationToken cancellationToken = default);

        Task<SortedList<string, Snapshot>> GetReplayByIdAsync(string id, CancellationToken cancellationToken = default);

        Task<bool> ClearMatchReplayAsync(MatchReport matchReport, CancellationToken cancellationToken = default);
    }
}
