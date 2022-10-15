using LoRWatcher.Caches;
using LoRWatcher.Clients.Functions;
using LoRWatcher.Logger;
using LoRWatcher.Stores;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace LoRWatcher.Services
{
    public class WatcherService
        : IWatcherService
    {
        private readonly IWatcherDataStore watcherDataStore;

        private readonly IPlayerDataStore playerDataStore;

        private readonly IFunctionsClient functionsClient;

        private readonly ILogger logger;

        public WatcherService(
            IWatcherDataStore watcherDataStore,
            IPlayerDataStore playerDataStore,
            IFunctionsClient functionsClient,
            ILogger logger)
        {
            this.watcherDataStore = watcherDataStore;
            this.playerDataStore = playerDataStore;
            this.functionsClient = functionsClient;
            this.logger = logger;
        }

        public async Task<bool> InitialiseMetadataAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                var metadataV2Result = await this.watcherDataStore.GetMatchReportsMetadataV2Async(cancellationToken);
                if (metadataV2Result == null)
                {
                    var getMetadataResult = await this.watcherDataStore.GetMatchReportsMetadataAsync(cancellationToken);
                    if (getMetadataResult != null)
                    {
                        var setMetadataResult = await this.watcherDataStore.SetMatchReportsMetadataAsync(getMetadataResult, cancellationToken);
                        if (setMetadataResult != null)
                        {
                            this.logger.Debug("Match report metadata initialised");
                        }
                    }
                }

                var getMatchesResult = await this.watcherDataStore.GetAllMatchReportsAsync(cancellationToken);
                var matchReplays = getMatchesResult.Where(mr => mr.Snapshots != null && mr.Snapshots?.Any() == true);
                foreach (var matchReplay in matchReplays)
                {
                    await this.watcherDataStore.ReportReplayAsync(matchReplay, cancellationToken);
                    await this.watcherDataStore.ClearMatchReplayAsync(matchReplay, cancellationToken);

                    this.logger.Debug("Match replay moved");
                }
            }
            catch (Exception ex)
            {
                this.logger.Debug($"Error occurred initialising store: {ex.Message}");

                return false;
            }

            return true;
        }

        public async Task<bool> SyncMatchReportsAsync(CancellationToken cancellationToken = default)
        {
            var metadata = await this.watcherDataStore.GetMatchReportsMetadataV2Async(cancellationToken);

            var player = await this.playerDataStore.GetAccountAsync(metadata.PlayerName, metadata.TagLine, cancellationToken);
            if (player == null)
            {
                // TODO: support region select
                var account = await this.functionsClient.GetAccountAsync(metadata.PlayerName, metadata.TagLine, "Europe", cancellationToken);
                if (account != null)
                {
                    player = await this.playerDataStore.AddAccountAsync(account.PlayerId, account.GameName, account.TagLine, cancellationToken);
                }
                else
                {
                    return false;
                }
            }

            var syncMatches = new List<MatchReport>();

            var matchReports = await this.watcherDataStore.GetMatchReportsAsync(0, 20, cancellationToken);
            if (matchReports != null &&
                matchReports.Any() == true)
            {
                foreach (var matchReport in matchReports)
                {
                    // Is mtach report id in the sync table, which will have the assocaiated player match id with it
                    var synced = await this.playerDataStore.IsMatchSyncedAsync(matchReport.Id, cancellationToken);
                    if (synced == false)
                    {
                        syncMatches.Add(matchReport);
                    }
                }
            }

            if (syncMatches.Any() == true)
            {
                var allowGetMatchIds = true;
                foreach (var syncMatch in syncMatches)
                {
                    var playerMatch = await this.playerDataStore.GetPlayerMatchAsync(player.PlayerId, syncMatch, cancellationToken);
                    if (playerMatch != null)
                    {
                        var syncResult = await this.playerDataStore.SyncMatchAsync(syncMatch.Id, playerMatch.Id, cancellationToken);
                        if (syncResult == false)
                        {
                            return false;
                        }
                    }
                    else
                    {
                        // false = Whether the match has been retrieved, so it only returns match ids that do not have a synced match
                        var matchIds = await this.playerDataStore.GetMatchIdsAsync(false, cancellationToken);
                        if (matchIds != null &&
                            matchIds.Any() == true)
                        {
                            foreach (var matchId in matchIds)
                            {
                                // TODO: support region select
                                var match = await this.functionsClient.GetMatchAsync(matchId, "Europe", cancellationToken);
                                if (match == null)
                                {
                                    // TODO: There may be scenarios where by null returned is fine as the match may not exists anymore. We should set the match id flag to true for these scenarios 'SyncMatchIdAsync'.
                                    return false;
                                }

                                var addMatchResult = await this.playerDataStore.AddMatchAsync(match, cancellationToken);
                                if (addMatchResult == false)
                                {
                                    return false;
                                }

                                // Set match id flag to true
                                var syncMatchIdResult = await this.playerDataStore.SyncMatchIdAsync(matchId, cancellationToken);
                                if (syncMatchIdResult == false)
                                {
                                    return false;
                                }
                            }

                            playerMatch = await this.playerDataStore.GetPlayerMatchAsync(player.PlayerId, syncMatch, cancellationToken);
                            if (playerMatch != null)
                            {
                                var syncResult = await this.playerDataStore.SyncMatchAsync(syncMatch.Id, playerMatch.Id, cancellationToken);
                                if (syncResult == false)
                                {
                                    return false;
                                }

                                continue;
                            }
                        }
                        
                        if (allowGetMatchIds == true)
                        {
                            // TODO: support region select
                            var playerMatchIds = await this.functionsClient.GetMatchIdsAsync(player.PlayerId, "Europe", cancellationToken);
                            if (playerMatchIds == null ||
                                playerMatchIds?.Any() == false)
                            {
                                return false;
                            }

                            var updateMatchIdsResult = await this.playerDataStore.UpdateMatchIdsAsync(matchIds, cancellationToken);
                            if (updateMatchIdsResult == false)
                            {
                                return false;
                            }

                            allowGetMatchIds = false;
                        }

                        // false = Whether the match has been retrieved, so it only returns match ids that do not have a synced match
                        var matchIds = await this.playerDataStore.GetMatchIdsAsync(false, cancellationToken);
                        if (matchIds != null &&
                            matchIds.Any() == true)
                        {
                            foreach (var matchId in matchIds)
                            {
                                // TODO: support region select
                                var match = await this.functionsClient.GetMatchAsync(matchId, "Europe", cancellationToken);
                                if (match == null)
                                {
                                    // TODO: There may be scenarios where by null returned is fine as the match may not exists anymore. We should set the match id flag to true for these scenarios 'SyncMatchIdAsync'.
                                    return false;
                                }

                                var addMatchResult = await this.playerDataStore.AddMatchAsync(match, cancellationToken);
                                if (addMatchResult == false)
                                {
                                    return false;
                                }

                                // Set match id flag to true
                                var syncMatchIdResult = await this.playerDataStore.SyncMatchIdAsync(matchId, cancellationToken);
                                if (syncMatchIdResult == false)
                                {
                                    return false;
                                }
                            }

                            playerMatch = await this.playerDataStore.GetPlayerMatchAsync(player.PlayerId, syncMatch, cancellationToken);
                            if (playerMatch != null)
                            {
                                var syncResult = await this.playerDataStore.SyncMatchAsync(syncMatch.Id, playerMatch.Id, cancellationToken);
                                if (syncResult == false)
                                {
                                    return false;
                                }
                            }
                            else
                            {
                                // Update sync table to include the match id but not the player match id and possibly a state to say it's 'NotFound'
                                // We know it's not found because we have retrieved all player match data from the riot apis and there are no matches that match this match so we report it as not found so that we do not attempt to find it again.
                                var matchNotFoundResult = await this.playerDataStore.MatchNotFoundAsync(syncMatch.Id, cancellationToken);
                                if (matchNotFoundResult == false)
                                {
                                    return false;
                                }
                            }
                        }
                    }
                }
            }

            // 1. Get up to the last 20 matches
            // 2. Check if they have been synced
            // 3. If all have been synced, return
            // 4. If there any that have not been synced retrieve the list of match ids 
            // 5. Then call the get match function for match ids that have not already been synced and store the data in the player datastore
            // 6. Once all recent match ids have been stored sync up the watcher store data and link them using the match id

            return true;
        }
    }
}
