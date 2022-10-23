using LoRWatcher.Stores.Documents;
using System;
using System.Collections.Generic;

namespace LoRWatcher.Caches
{
    public class MatchReport
    {
        public static MatchReport Create(MatchReport matchReport)
        {
            return new MatchReport
            {
                Id = Guid.NewGuid().ToString(),
                PlayerName = matchReport.PlayerName,
                OpponentName = matchReport.OpponentName,
                PlayerDeckCode = matchReport.PlayerDeckCode,
                Regions = matchReport.Regions,
                Result = matchReport.Result,
                StartTime = matchReport.StartTime,
                FinishTime = matchReport.FinishTime,
                Snapshots = matchReport.Snapshots
            };
        }

        public string Id { get; set; }

        public string PlayerName { get; set; }

        public IEnumerable<string> Regions { get; set; }

        public string PlayerDeckCode { get; set; }

        public string OpponentName { get; set; }

        public bool Result { get; set; }

        public DateTimeOffset StartTime { get; set; }

        public DateTimeOffset FinishTime { get; set; }

        public SortedList<string, Snapshot> Snapshots { get; set; }
    }
}
