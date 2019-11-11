namespace LoRWatcher.Caches
{
    public class MatchReport
    {
        public static MatchReport Create(MatchReport matchReport)
        {
            return new MatchReport
            {
                PlayerName = matchReport.PlayerName,
                OpponentName = matchReport.OpponentName
            };
        }

        public string PlayerName { get; set; }

        public string OpponentName { get; set; }
    }
}
