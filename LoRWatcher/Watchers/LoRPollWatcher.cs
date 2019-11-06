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
