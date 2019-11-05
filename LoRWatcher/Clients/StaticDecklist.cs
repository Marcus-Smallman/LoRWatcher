using System.Collections.Generic;

namespace LoRWatcher.Clients
{
    public class StaticDecklist
    {
        public string DeckCode { get; set; }

        public Dictionary<string, int> CardsInDeck { get; set; }
    }
}
