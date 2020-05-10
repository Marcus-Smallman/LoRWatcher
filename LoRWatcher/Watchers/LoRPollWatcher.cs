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
        private int PollIntervalMilliseconds { get; set; }

        private bool IsClientActive { get; set; }

        private int? GameId { get; set; }

        private bool CanUpdateActiveMatch { get; set; }

        private readonly IGameClient loRClient;

        private readonly IActiveGameCache activeGameCache;

        private readonly IActiveExpeditionCache activeExpeditionCache;

        private readonly IGameStateCache gameStateCache;

        private readonly IWatcherDataStore watcherDataStore;

        private readonly ILogger logger;

        public LoRPollWatcher(
            IGameClient loRClient,
            IActiveGameCache activeGameCache,
            IActiveExpeditionCache activeExpeditionCache,
            IGameStateCache gameStateCache,
            IWatcherDataStore watcherDataStore,
            ILogger logger)
        {
            this.GameId = null;
            this.CanUpdateActiveMatch = true;

            this.loRClient = loRClient;
            this.activeGameCache = activeGameCache;
            this.activeExpeditionCache = activeExpeditionCache;
            this.gameStateCache = gameStateCache;
            this.watcherDataStore = watcherDataStore;
            this.logger = logger;

            this.SetDefaults();
        }

        private void SetDefaults()
        {
            this.PollIntervalMilliseconds = 1000;
            this.IsClientActive = false;
        }

        protected override Task ExecuteAsync(CancellationToken cancellationToken)
        {
            return Task.Run(async () =>
            {
                this.logger.Info("Starting watcher");

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

                    await Task.Delay(this.PollIntervalMilliseconds, cancellationToken);
                }
            },
            cancellationToken);
        }

        private async Task IsClientActiveAsync(CancellationToken cancellationToken)
        {
            try
            {
                this.IsClientActive = await this.loRClient.IsClientActiveAsync(cancellationToken);
                if (this.IsClientActive == true)
                {
                    var gameResult = await this.loRClient.GetGameResultAsync(cancellationToken);
                    this.GameId = gameResult.GameId;
                }
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
                        this.PollIntervalMilliseconds = 500;
                        this.CanUpdateActiveMatch = true;

                        var expeditionsState = await this.loRClient.GetExpeditionsStateAsync(cancellationToken);
                        if (expeditionsState.IsActive == true)
                        {
                            this.activeExpeditionCache.UpdateState(expeditionsState);
                        }

                        this.logger.Debug("Waiting for active match");
                        break;
                    case GameState.InProgress:
                        if (this.CanUpdateActiveMatch == true)
                        {
                            this.PollIntervalMilliseconds = 100;

                            this.logger.Debug("Updating active match");

                            await this.activeGameCache.UpdateActiveMatchAsync(cardPositions, cancellationToken);
                        }
                        break;
                    default:
                        this.SetDefaults();
                        break;
                }

                await this.ReportMatchAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                this.logger.Error($"Error occurred polling client: {ex.Message}");
            }
        }

        private async Task ReportMatchAsync(CancellationToken cancellationToken)
        {
            if (this.activeGameCache.IsEmpty == false)
            {
                var gameResult = await this.loRClient.GetGameResultAsync(cancellationToken);
                if (gameResult.GameId != this.GameId)
                {
                    this.logger.Debug("Getting match report");

                    var matchReport = await this.activeGameCache.GetMatchReportAsync(cancellationToken);
                    if (matchReport != null)
                    {
                        this.logger.Debug("Reporting match");

                        await this.watcherDataStore.ReportGameAsync(matchReport, cancellationToken);
                    }

                    this.GameId = gameResult.GameId;
                    this.CanUpdateActiveMatch = false;
                }
            }
        }
    }
}
