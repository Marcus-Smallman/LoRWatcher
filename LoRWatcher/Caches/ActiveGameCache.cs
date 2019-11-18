using LoRWatcher.Clients;
using LoRWatcher.Logger;
using LoRWatcher.Utils;
using Newtonsoft.Json;
using System;
using System.Threading.Tasks;

namespace LoRWatcher.Caches
{
    public class ActiveGameCache
        : ICache
    {
        private readonly ILogger logger;

        private readonly IGameClient loRClient;

        private MatchReport currentMatch;

        public bool IsEmpty { get => currentMatch == null; }

        private int PreviousGameId { get; set; }

        // Set to -1 as the game id resets to that every time the game client restarts
        private const int GameIdStartCount = -1;

        public ActiveGameCache(IGameClient loRClient, ILogger logger)
        {
            this.loRClient = loRClient;
            this.logger = logger;

            
            this.PreviousGameId = GameIdStartCount;
        }

        public async Task<MatchReport> GetMatchReportAsync()
        {
            MatchReport matchReport = null;
            await Retry.InvokeAsync(async () =>
            {
                var gameResult = await this.loRClient.GetGameResult();
                if (gameResult.GameId != this.PreviousGameId)
                {
                    this.currentMatch.Result = bool.Parse(gameResult.LocalPlayerWon);

                    matchReport = MatchReport.Create(this.currentMatch);

                    this.currentMatch = null;
                    this.PreviousGameId = gameResult.GameId;

                    return true;
                }

                return false;
            });

            if (matchReport == null)
            {
                if (this.PreviousGameId == GameIdStartCount)
                {
                    this.logger.Debug("No match to report");
                }
                else
                {
                    this.logger.Debug("Match already reported");
                }

                return null;
            }

            return matchReport;
        }

        public async Task UpdateActiveMatchAsync(PositionalRectangles positionalRectangles)
        {
            this.logger.Debug($"Positional rectangles: {JsonConvert.SerializeObject(positionalRectangles)}");

            if (currentMatch != null)
            {
                // Update match data
                return;
            }

            var activeDecklist = await this.loRClient.GetActiveDecklistAsync();

            this.currentMatch = new MatchReport
            {
                PlayerName = positionalRectangles.PlayerName,
                PlayerDeckCode = activeDecklist.DeckCode,
                OpponentName = positionalRectangles.OpponentName,
            };
        }
    }
}
