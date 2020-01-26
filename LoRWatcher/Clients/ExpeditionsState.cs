using System.Collections.Generic;

namespace LoRWatcher.Clients
{
    public class ExpeditionsState
    {
        public bool IsActive { get; set; }

        public ExpeditionState State { get; set; }

        public IEnumerable<string> Record { get; set; }

        public IEnumerable<string> DraftPicks { get; set; }

        public IEnumerable<string> Deck { get; set; }

        public string Games { get; set; }

        public string Wins { get; set; }

        public string Losses { get; set; }
    }
}
