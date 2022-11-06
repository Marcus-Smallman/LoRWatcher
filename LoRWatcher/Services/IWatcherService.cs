﻿using LoRWatcher.Stores.Documents;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace LoRWatcher.Services
{
    public interface IWatcherService
    {
        Task<bool> InitialiseMetadataAsync(CancellationToken cancellationToken = default);

        Task<bool> SyncMatchReportsAsync(CancellationToken cancellationToken = default);

        Task<IEnumerable<ServiceMatchReport>> GetMatchReportsAsync(
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

        Task<ServiceMatchReport> GetMatchReportByIdAsync(string id, CancellationToken cancellationToken = default);
    }
}
