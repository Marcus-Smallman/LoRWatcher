using LoRWatcher.Clients;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace LoRWatcher.Watchers
{
    public class LoRPollWatcher
        : IWatcher
    {
        private readonly int pollIntervalMS = 1000; // Recommended to poll no more than one second.

        private readonly IGameClient loRClient;

        private CancellationTokenSource cancellationTokenSource;

        public LoRPollWatcher(IGameClient loRClient)
        {
            this.loRClient = loRClient;
        }

        public async Task StartAsync()
        {
            this.cancellationTokenSource = new CancellationTokenSource();

            await Task.Run(async () =>
            {
                // We only want to run this when the game client exists,
                // If the game is not running potentially leave it in a
                // long poll and then when the game client is alive
                // set it to a faster poll time, then when the player
                // is in an active game, increase the poll rate to capture
                // game data
                var running = true;
                while (running)
                {
                    await Task.Delay(this.pollIntervalMS);
                }
            },
            cancellationTokenSource.Token);
        }

        public void Stop()
        {
            this.cancellationTokenSource.Cancel();
        }
    }
}
