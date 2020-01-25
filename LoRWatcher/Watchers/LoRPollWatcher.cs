using LoRWatcher.Caches;
using LoRWatcher.Clients;
using LoRWatcher.Logger;
using LoRWatcher.Stores;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace LoRWatcher.Watchers
{
    public class LoRPollWatcher
        : IWatcher
    {
        private int PollIntervalMS { get; set; }

        private readonly IGameClient loRClient;

        private readonly ICache activeGameCache;

        //private readonly IServiceClient serviceClient;

        private readonly IWatcherDataStore watcherDataStore;

        private readonly ILogger logger;

        private CancellationTokenSource cancellationTokenSource;

        public LoRPollWatcher(
            IGameClient loRClient,
            ICache activeGameCache,
            IWatcherDataStore watcherDataStore,
            //IServiceClient serviceClient,
            ILogger logger)
        {
            this.loRClient = loRClient;
            this.activeGameCache = activeGameCache;
            this.watcherDataStore = watcherDataStore;
            //this.serviceClient = serviceClient;
            this.logger = logger;

            this.SetDefaults();
        }

        public async Task StartAsync()
        {
            this.cancellationTokenSource = new CancellationTokenSource();

            await Task.Run(async () =>
            {
                this.logger.Debug("Starting watcher");

                var running = true;
                while (running)
                {
                    try
                    {
                        var cardPositions = await this.loRClient.GetCardPositionsAsync();
                        switch (cardPositions?.GameState)
                        {
                            case GameState.Menus:
                                this.PollIntervalMS = 2500;
                                if (this.activeGameCache.IsEmpty == false)
                                {
                                    this.logger.Debug("Getting match report");

                                    var matchReport = await this.activeGameCache.GetMatchReportAsync();
                                    if (matchReport != null)
                                    {
                                        this.logger.Debug("Reporting match");

                                        await this.watcherDataStore.ReportGameAsync(matchReport);
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

                                await this.activeGameCache.UpdateActiveMatchAsync(cardPositions);
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

                    await Task.Delay(this.PollIntervalMS);
                }
            },
            cancellationTokenSource.Token);
        }

        public void Stop()
        {
            this.cancellationTokenSource.Cancel();
        }

        private void SetDefaults()
        {
            this.PollIntervalMS = 1000;
        }
    }
}
