using LoRDeckCodes;
using LoRWatcher.Clients;
using LoRWatcher.Logger;
using LoRWatcher.Stores.Documents;
using LoRWatcher.Utils;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace LoRWatcher.Caches
{
    public class ActiveGameCache
        : IActiveGameCache
    {
        private readonly ILogger logger;

        private readonly IActiveExpeditionCache activeExpeditionCache;

        private readonly IGameClient loRClient;

        private MatchReport currentMatch;

        public bool IsEmpty { get => currentMatch == null; }

        public ActiveGameCache(IGameClient loRClient, IActiveExpeditionCache activeExpeditionCache, ILogger logger)
        {
            this.loRClient = loRClient;
            this.activeExpeditionCache = activeExpeditionCache;
            this.logger = logger;
        }

        public async Task<MatchReport> GetMatchReportAsync(CancellationToken cancellationToken)
        {
            MatchReport matchReport = null;
            await Retry.InvokeAsync(async () =>
            {
                try
                {
                    var gameResult = await this.loRClient.GetGameResultAsync(cancellationToken);
                    if (gameResult != null)
                    {
                        this.currentMatch.Result = bool.Parse(gameResult.LocalPlayerWon);
                        this.currentMatch.FinishTime = DateTimeOffset.UtcNow;

                        matchReport = MatchReport.Create(this.currentMatch);

                        this.currentMatch = null;

                        return true;
                    }
                }
                catch (Exception ex)
                {
                    this.logger.Error($"Error occurred getting match report: {ex.Message}");
                }

                return false;
            });

            if (matchReport == null)
            {
                this.logger.Debug("No match to report");
            }

            return matchReport;
        }

        public async Task UpdateActiveMatchAsync(PositionalRectangles positionalRectangles, CancellationToken cancellationToken)
        {
            this.logger.Debug($"Positional rectangles: {JsonConvert.SerializeObject(positionalRectangles)}");

            if (this.currentMatch != null)
            {
                // Update match data
                return;
            }

            var activeDecklist = await this.loRClient.GetActiveDecklistAsync(cancellationToken);
            if (activeDecklist != null &&
                activeDecklist.DeckCode != null &&
                activeDecklist.CardsInDeck.Any())
            {
                this.logger.Debug($"Active decklist: {JsonConvert.SerializeObject(activeDecklist)}");

                // TODO: Looks like there is an issue with the active decklist returned from the client being incorrect
                // This can be changed to use the deckcode returned from the client once fixed (02/05/20).
                var cards = new List<CardCodeAndCount>();
                foreach (var card in activeDecklist.CardsInDeck)
                {
                    cards.Add(new CardCodeAndCount { CardCode = card.Key, Count = card.Value });
                }

                var activeDeckCode = LoRDeckEncoder.GetCodeFromDeck(cards);

                this.logger.Debug($"Retrieved active game deck code: {activeDeckCode}");

                var gameType = await this.GetGameTypeAsync(activeDeckCode, cancellationToken);

                this.currentMatch = new MatchReport
                {
                    PlayerName = positionalRectangles.PlayerName,
                    PlayerDeckCode = activeDeckCode,
                    OpponentName = positionalRectangles.OpponentName,
                    Type = gameType
                };
            }
        }

        private async Task<GameType> GetGameTypeAsync(string activeDeckCode, CancellationToken cancellationToken)
        {
            var activeExpeditionDeckCode = await this.activeExpeditionCache.GetDeckCodeAsync(cancellationToken);
            if (activeExpeditionDeckCode == activeDeckCode)
            {
                return GameType.Expedition;
            }

            return GameType.Normal;
        }
    }
}
