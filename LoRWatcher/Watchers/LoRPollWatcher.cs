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

        private bool IsClientActive { get; set; }

        private readonly IGameClient loRClient;

        private readonly IActiveGameCache activeGameCache;

        private readonly IGameStateCache gameStateCache;

        private readonly IWatcherDataStore watcherDataStore;

        private readonly ILogger logger;

        public LoRPollWatcher(
            IGameClient loRClient,
            IActiveGameCache activeGameCache,
            IGameStateCache gameStateCache,
            IWatcherDataStore watcherDataStore,
            ILogger logger)
        {
            this.loRClient = loRClient;
            this.gameStateCache = gameStateCache;
            this.activeGameCache = activeGameCache;
            this.watcherDataStore = watcherDataStore;
            this.logger = logger;

            this.SetDefaults();
        }

        private void SetDefaults()
        {
            this.PollIntervalMS = 2500;
            this.IsClientActive = false;
        }

        protected override Task ExecuteAsync(CancellationToken cancellationToken)
        {
            return Task.Run(async () =>
            {
                this.logger.Debug("Starting watcher");

                var running = true;
                while (running)
                {
                    if (this.IsClientActive == false)
                    {
                        await IsClientActiveAsync(cancellationToken);
                    }
                    else
                    {
                        await PollClientAsync(cancellationToken);
                    }

                    await Task.Delay(this.PollIntervalMS, cancellationToken);
                }
            },
            cancellationToken);
        }

        private async Task IsClientActiveAsync(CancellationToken cancellationToken)
        {
            try
            {
                this.IsClientActive = await this.loRClient.IsClientActiveAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                this.logger.Error($"Error occured checking if client is active: {ex.Message}");
            }
        }

        private async Task PollClientAsync(CancellationToken cancellationToken)
        {
            try
            {
                var cardPositions = await this.loRClient.GetCardPositionsAsync(cancellationToken);
                this.gameStateCache.SetGameState(cardPositions?.GameState);
                switch (cardPositions?.GameState)
                {
                    case GameState.Menus:
                        this.PollIntervalMS = 1000;
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
                            // TODO: Support expeditions type
                            //var expeditionsState = await this.loRClient.GetExpeditionsStateAsync(cancellationToken);

                            this.logger.Debug("Waiting for active match");
                        }

                        break;
                    case GameState.InProgress:
                        this.PollIntervalMS = 100;

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
        }
    }
}
