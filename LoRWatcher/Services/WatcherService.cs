using LoRWatcher.Logger;
using LoRWatcher.Stores;
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
            var getMetadataResult = await this.watcherDataStore.GetMatchReportsMetadataAsync(cancellationToken);
            if (getMetadataResult != null)
            {
                var setMetadataResult = await this.watcherDataStore.SetMatchReportsMetadataAsync(getMetadataResult, cancellationToken);
                if (setMetadataResult != null)
                {
                    this.logger.Debug("Match report metadata initialised");

                    return true;
                }
            }

            return false;
        }
    }
}
