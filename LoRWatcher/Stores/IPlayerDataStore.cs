﻿using LoRWatcher.Caches;
using LoRWatcher.Clients.Functions;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace LoRWatcher.Stores
{
    public interface IPlayerDataStore
    {
        Task<Account> GetAccountAsync(string gameName, string tagLine, CancellationToken cancellationToken = default);

        Task<Account> AddAccountAsync(string playerId, string gameName, string tagLine, CancellationToken cancellationToken = default);

        Task<bool> IsMatchSyncedAsync(string watcherMatchId, CancellationToken cancellationToken = default);

        Task<PlayerMatch> GetPlayerMatchAsync(string playerId, MatchReport matchReport, CancellationToken cancellationToken = default);

        Task<bool> SyncMatchAsync(string playerMatchId, string watcherMatchId, CancellationToken cancellationToken = default);

        Task<IEnumerable<string>> GetMatchIdsAsync(bool includeSynced = true, CancellationToken cancellationToken = default);

        Task<bool> AddMatchAsync(Match playerMatch, CancellationToken cancellationToken = default);

        Task<bool> SyncMatchIdAsync(string matchId, CancellationToken cancellationToken = default);

        Task<bool> UpdateMatchIdsAsync(IEnumerable<string> matchIds, CancellationToken cancellationToken = default);

        Task<bool> MatchNotFoundAsync(string watcherMatchId, CancellationToken cancellationToken = default);

        Task<MatchSync> GetSyncedMatchByWatcherIdAsync(string watcherMatchId, CancellationToken cancellationToken = default);

        Task<MatchSync> GetSyncedMatchByPlayerIdAsync(string playerMatchId, CancellationToken cancellationToken = default);

        Task<PlayerMatch> GetPlayerMatchByIdAsync(string playerMatchId, CancellationToken cancellationToken = default);

        Task<IEnumerable<PlayerMatch>> GetPlayerMatchesAsync(int skip, int limit, int gameTypeSortDirection, CancellationToken cancellationToken = default);
    }
}
