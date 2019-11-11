using System;
using System.Threading.Tasks;
using LoRWatcher.Caches;

namespace LoRWatcher.Clients
{
    public class LoRServiceClient
        : IServiceClient
    {
        public Task ReportGameAsync(MatchReport matchReport)
        {
            Console.WriteLine("Player: {0}, Opponent: {1}", matchReport.PlayerName, matchReport.OpponentName);

            // TODO: Call service API with correct data

            return Task.CompletedTask;
        }
    }
}
