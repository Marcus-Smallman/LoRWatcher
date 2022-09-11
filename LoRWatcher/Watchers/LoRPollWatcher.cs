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
            this.GameId = null;
            this.CanUpdateActiveMatch = true;

            this.loRClient = loRClient;
            this.activeGameCache = activeGameCache;
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
                else
                {
                    if (this.gameStateCache.GetGameState() != GameState.Offline)
                    {
                        this.logger.Info($"Watcher is now inactive.");
                    }

                    this.gameStateCache.SetGameState(GameState.Offline);
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
                if (cardPositions?.GameState != null)
                {
                    var currentCameState = this.gameStateCache.GetGameState();
                    if (currentCameState == GameState.Offline ||
                        currentCameState == GameState.Startup)
                    {
                        this.logger.Info($"Watcher is now active.");
                    }

                    this.gameStateCache.SetGameState(cardPositions.GameState);
                }

                switch (cardPositions?.GameState)
                {
                    case GameState.Menus:
                        this.PollIntervalMilliseconds = 500;
                        this.CanUpdateActiveMatch = true;

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
                // TODO: Handle the scenario where this is null.. This can be acheived by closing the client during a game.
                var gameResult = await this.loRClient.GetGameResultAsync(cancellationToken);
                if (gameResult.GameId != this.GameId &&
                    gameResult.GameId != -1)
                {
                    this.logger.Info("Getting match report");

                    var matchReport = await this.activeGameCache.GetMatchReportAsync(cancellationToken);
                    if (matchReport != null)
                    {
                        this.logger.Info("Reporting match");

                        await this.watcherDataStore.ReportGameAsync(matchReport, cancellationToken);
                    }

                    this.GameId = gameResult.GameId;
                    this.CanUpdateActiveMatch = false;
                }
            }
        }
    }
}
