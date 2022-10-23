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

        private readonly IGameClient loRClient;

        private MatchReport currentMatch;

        public bool IsEmpty { get => currentMatch == null; }

        public ActiveGameCache(IGameClient loRClient, ILogger logger)
        {
            this.loRClient = loRClient;
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
                this.logger.Info("No match to report");
            }

            return matchReport;
        }

        public async Task UpdateActiveMatchAsync(PositionalRectangles positionalRectangles, CancellationToken cancellationToken)
        {
            this.logger.Debug($"Positional rectangles: {JsonConvert.SerializeObject(positionalRectangles)}");

            if (this.currentMatch != null)
            {
                try
                {
                    var currentSnapshot = new Snapshot
                    {
                        ScreenWidth = positionalRectangles.Screen.ScreenWidth,
                        ScreenHeight = positionalRectangles.Screen.ScreenHeight,
                        Rectangles = positionalRectangles.Rectangles
                    };

                    if (this.currentMatch.Snapshots.Count > 0)
                    {
                        var lastSnapshot = this.currentMatch.Snapshots[this.currentMatch.Snapshots.Keys[this.currentMatch.Snapshots.Keys.Count - 1]];
                        if (currentSnapshot.CardEquals(lastSnapshot) == true)
                        {
                            return;
                        }
                    }

                    this.currentMatch.Snapshots.Add(positionalRectangles.RetrievedTimeUtc.ToString("o"), currentSnapshot);
                }
                catch (Exception ex)
                {
                    this.logger.Error($"Error occurred adding snapshot: {ex.Message}");
                }

                return;
            }

            var activeDecklist = await this.loRClient.GetActiveDecklistAsync(cancellationToken);
            if (activeDecklist != null &&
                activeDecklist.CardsInDeck.Any())
            {
                this.logger.Info($"Active decklist: {JsonConvert.SerializeObject(activeDecklist)}");

                // TODO: Looks like there is an issue with the active decklist returned from the client being incorrect
                // This can be changed to use the deck code returned from the client once fixed (02/05/20).
                var cards = new List<CardCodeAndCount>();
                foreach (var card in activeDecklist.CardsInDeck)
                {
                    cards.Add(new CardCodeAndCount { CardCode = card.Key, Count = card.Value });
                }

                cards.Sort(new CardComparer());

                var activeDeckCode = LoRDeckEncoder.GetCodeFromDeck(cards);

                this.logger.Info($"Retrieved active game deck code: {activeDeckCode}");

                cards.Print(this.logger);

                this.currentMatch = new MatchReport
                {
                    PlayerName = positionalRectangles.PlayerName,
                    OpponentName = positionalRectangles.OpponentName,
                    PlayerDeckCode = activeDeckCode,
                    Regions = cards.GetRegions(),
                    StartTime = DateTimeOffset.UtcNow,
                    Snapshots = new SortedList<string, Snapshot>()
                };
            }
        }
    }
}
