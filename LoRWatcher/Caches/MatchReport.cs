using System;

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
                Result = matchReport.Result,
                FinishTime = matchReport.FinishTime
            };
        }

        public string Id { get; set; }

        public string PlayerName { get; set; }

        public string PlayerDeckCode { get; set; }

        public string OpponentName { get; set; }

        public bool Result { get; set; }

        public DateTimeOffset FinishTime { get; set; }
    }
}
