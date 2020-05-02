using LoRDeckCodes;
using LoRWatcher.Clients;
using LoRWatcher.Logger;
using LoRWatcher.Utils;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;

namespace LoRWatcher.Caches
{
    public class ActiveExpeditionCache
        : IActiveExpeditionCache
    {
        private readonly ILogger logger;

        private ExpeditionsState expeditionsState;

        public ActiveExpeditionCache(ILogger logger)
        {
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

        public string GetDeckCode()
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

                // TODO: This comparer may not be needed.
                cards.Sort(new CardComparer());

                var deckCode = LoRDeckEncoder.GetCodeFromDeck(cards);

                this.logger.Debug($"Getting active expedition deck code: {deckCode}");

                return deckCode;
            }

            return null;
        }
    }
}
