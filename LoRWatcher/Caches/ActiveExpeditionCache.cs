using LoRDeckCodes;
using LoRWatcher.Clients;
using LoRWatcher.Logger;
using LoRWatcher.Utils;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace LoRWatcher.Caches
{
    public class ActiveExpeditionCache
        : IActiveExpeditionCache
    {
        private readonly IGameClient gameClient;

        private readonly ILogger logger;

        private ExpeditionsState expeditionsState;

        public ActiveExpeditionCache(IGameClient gameClient, ILogger logger)
        {
            this.gameClient = gameClient;
            this.logger = logger;
        }

        public void UpdateState(ExpeditionsState expeditionsState)
        {
            if (this.expeditionsState == null ||
                this.expeditionsState.State != expeditionsState.State ||
                this.expeditionsState.Record.SequenceEqual(expeditionsState.Record) == false ||
                this.expeditionsState.Deck.SequenceEqual(expeditionsState.Deck) == false ||
                this.expeditionsState.Games != expeditionsState.Games ||
                this.expeditionsState.Wins != expeditionsState.Wins ||
                this.expeditionsState.Losses != expeditionsState.Losses)
            {
                this.logger.Debug($"Updating active expedition state: {JsonConvert.SerializeObject(expeditionsState)}");

                this.expeditionsState = expeditionsState;
            }
        }

        public async Task<string> GetDeckCodeAsync(CancellationToken cancellationToken)
        {
            // There can be scenarios where the user starts the application in the middle of a game.
            // If this is the case, we will not have stored any expedition state. Therefore, if the expedition
            // state is null or empty, a call to the expedition state api is done. 
            if (this.expeditionsState == null ||
                this.expeditionsState.Deck?.Any() == false)
            {
                var expeditionsState = await this.gameClient.GetExpeditionsStateAsync(cancellationToken);
                if (expeditionsState.IsActive == true)
                {
                    this.UpdateState(expeditionsState);
                }
            }

            return this.GetDeckCode();
        }

        private string GetDeckCode()
        {
            if (this.expeditionsState != null &&
                this.expeditionsState.Deck?.Any() == true)
            {
                var cards = new List<CardCodeAndCount>();
                foreach (var cardCode in this.expeditionsState.Deck)
                {
                    var card = cards.Find(c => c.CardCode == cardCode);
                    if (card == null)
                    {
                        cards.Add(new CardCodeAndCount { CardCode = cardCode, Count = 1 });
                    }
                    else
                    {
                        card.Count++;
                    }
                }

                // TODO: This sort and comparer may not be needed.
                cards.Sort(new CardComparer());

                var deckCode = LoRDeckEncoder.GetCodeFromDeck(cards);

                this.logger.Debug($"Retrieved active expedition deck code: {deckCode}");

                return deckCode;
            }

            return null;
        }
    }
}
