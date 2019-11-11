using LoRWatcher.Caches;
using LoRWatcher.Clients;
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

        private readonly IServiceClient serviceClient;

        private CancellationTokenSource cancellationTokenSource;

        public LoRPollWatcher(IGameClient loRClient, ICache activeGameCache, IServiceClient serviceClient)
        {
            this.loRClient = loRClient;
            this.activeGameCache = activeGameCache;
            this.serviceClient = serviceClient;

            this.SetDefaults();
        }

        public async Task StartAsync()
        {
            this.cancellationTokenSource = new CancellationTokenSource();

            await Task.Run(async () =>
            {
                var running = true;
                while (running)
                {
                    try
                    {
                        var cardPositions = await this.loRClient.GetCardPositionsAsync();
                        switch (cardPositions.GameState)
                        {
                            case GameState.Menus:
                                this.PollIntervalMS = 5000;
                                if (this.activeGameCache.IsEmpty == false)
                                {
                                    var matchReport = this.activeGameCache.GetMatchReport();

                                    await this.serviceClient.ReportGameAsync(matchReport);
                                }

                                break;
                            case GameState.InProgress:
                                this.PollIntervalMS = 1000;

                                this.activeGameCache.UpdateActiveMatch(cardPositions);
                                break;
                            default:
                                this.SetDefaults();
                                break;
                        }
                    }
                    catch
                    { 
                        // Log error
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
