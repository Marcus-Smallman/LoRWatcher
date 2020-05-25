namespace LoRWatcher.Stores
{
    public class MatchReportMetadata
    {
        public int TotalWins { get; set; }

        public int TotalLosses { get; set; }

        public int TotalGames { get; set; }

        public string MostPlayedRegions { get; set; }

        public int MostPlayedRegionsCount { get; set; }

        public string HighestWinRateRegions { get; set; }

        public int HighestWinRateRegionsPercentage { get; set; }
    }
}
