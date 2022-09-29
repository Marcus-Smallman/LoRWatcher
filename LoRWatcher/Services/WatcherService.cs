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

        private readonly ILogger logger;

        public WatcherService(IWatcherDataStore watcherDataStore, ILogger logger)
        {
            this.watcherDataStore = watcherDataStore;
            this.logger = logger;
        }

        public async Task<bool> InitialiseMetadataAsync(CancellationToken cancellationToken)
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
    }
}
