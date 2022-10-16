using System.Collections.Generic;

namespace LoRWatcher.Stores.Documents
{
    public class PlayerDocument
    {
        public string PlayerId { get; set; }

        public string DeckId { get; set; }

        public string DeckCode { get; set; }

        public IEnumerable<string> Factions { get; set; }

        public string GameOutcome { get; set; }

        public int OrderOfPlay { get; set; }
    }
}
