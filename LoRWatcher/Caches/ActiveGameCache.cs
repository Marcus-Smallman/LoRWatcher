using LoRWatcher.Clients;
using System;

namespace LoRWatcher.Caches
{
    public class ActiveGameCache
        : ICache
    {
        private MatchReport currentMatch;

        public bool IsEmpty { get => currentMatch == null; }

        public MatchReport GetMatchReport()
        {
            var matchReport = MatchReport.Create(this.currentMatch);

            this.currentMatch = null;

            return matchReport;
        }

        public void UpdateActiveMatch(PositionalRectangles positionalRectangles)
        {
            if (currentMatch != null)
            {
                // Update match data
                return;
            }

            this.currentMatch = new MatchReport
            {
                PlayerName = positionalRectangles.PlayerName,
                OpponentName = positionalRectangles.OpponentName
            };
        }
    }
}
