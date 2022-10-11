using LoRWatcher.Clients.Functions;
using LoRWatcher.Logger;
using LoRWatcher.Stores;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace LoRWatcher.Services
{
    public class WatcherService
        : IWatcherService
    {
        private readonly IWatcherDataStore watcherDataStore;

        private readonly IFunctionsClient functionsClient;

        private readonly ILogger logger;

        public WatcherService(IWatcherDataStore watcherDataStore, IFunctionsClient functionsClient, ILogger logger)
        {
            this.watcherDataStore = watcherDataStore;
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

            var player = await this.playerDataStore.GetPlayerAsync(metadata.PlayerName, metadata.TagLine, cancellationToken);
            if (player == null)
            {
                // TODO: support region select
                var account = await this.functionsClient.GetAccountAsync(metadata.PlayerName, metadata.TagLine, "Europe", cancellationToken);

                player = await this.playerDataStore.AddPlayerAsync(account.PlayerId, account.GameName, account.TagLine, cancellationToken);
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
