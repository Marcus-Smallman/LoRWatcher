using System;
using System.Collections.Generic;

namespace LoRWatcher.Stores.Documents
{
    public class MatchReportDocument
    {
        public string Id { get; set; }

        public string PlayerName { get; set; }

        public string PlayerDeckCode { get; set; }

        public IEnumerable<string> Regions { get; set;}

        public string OpponentName { get; set; }

        public bool Result { get; set; }

        public SortedList<string, SnapshotDocument> Snapshots { get; set; }

        public DateTime StartTime { get; set; }

        public DateTime FinishTime { get; set; }

        public GameType? Type { get; set; }
    }
}
