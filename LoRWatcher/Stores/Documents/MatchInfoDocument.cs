using System;
using System.Collections.Generic;

namespace LoRWatcher.Stores.Documents
{
    public class MatchInfoDocument
    {
        public string GameMode { get; set; }

        public string GameType { get; set; }

        public string GameStartTimeUTC { get; set; }

        public string GameVersion { get; set; }

        public IEnumerable<PlayerDocument> Players { get; set; }

        public int TotalTurnCount { get; set; }
    }
}
