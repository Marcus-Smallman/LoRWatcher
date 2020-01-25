using LoRWatcher.Caches;
using LoRWatcher.Clients;
using LoRWatcher.Logger;
using LoRWatcher.Stores;
using Microsoft.Extensions.Hosting;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace LoRWatcher.Watchers
{
    public class LoRPollWatcher
        : BackgroundService
    {
        private int PollIntervalMS { get; set; }

        private readonly IGameClient loRClient;

        private readonly ICache activeGameCache;

        private readonly IWatcherDataStore watcherDataStore;

        private readonly ILogger logger;

        public LoRPollWatcher(
            IGameClient loRClient,
            ICache activeGameCache,
            IWatcherDataStore watcherDataStore,
            ILogger logger)
        {
            this.loRClient = loRClient;
            this.activeGameCache = activeGameCache;
            this.watcherDataStore = watcherDataStore;
            this.logger = logger;

            this.SetDefaults();
        }

        private void SetDefaults()
        {
            this.PollIntervalMS = 1000;
        }

        protected override Task ExecuteAsync(CancellationToken cancellationToken)
        {
            return Task.Run(async () =>
            {
                this.logger.Debug("Starting watcher");

                var running = true;
                while (running)
                {
                    try
                    {
                        var cardPositions = await this.loRClient.GetCardPositionsAsync(cancellationToken);
                        switch (cardPositions?.GameState)
                        {
                            case GameState.Menus:
                                this.PollIntervalMS = 2500;
                                if (this.activeGameCache.IsEmpty == false)
                                {
                                    this.logger.Debug("Getting match report");

                                    var matchReport = await this.activeGameCache.GetMatchReportAsync(cancellationToken);
                                    if (matchReport != null)
                                    {
                                        this.logger.Debug("Reporting match");

                                        await this.watcherDataStore.ReportGameAsync(matchReport, cancellationToken);
                                    }
                                }
                                else
                                {
                                    this.logger.Debug("Waiting for active match");
                                }

                                break;
                            case GameState.InProgress:
                                this.PollIntervalMS = 250;

                                this.logger.Debug("Updating active match");

                                await this.activeGameCache.UpdateActiveMatchAsync(cardPositions, cancellationToken);
                                break;
                            default:
                                this.SetDefaults();
                                break;
                        }
                    }
                    catch (Exception ex)
                    {
                        this.logger.Error($"Error occurred polling client: {ex.Message}");
                    }

                    await Task.Delay(this.PollIntervalMS, cancellationToken);
                }
            },
            cancellationToken);
        }
    }
}
