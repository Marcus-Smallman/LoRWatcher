using LiteDB;

namespace LoRWatcher.Stores.Documents
{
    public class MatchReportMetadataDocument
    {
        [BsonId]
        public string PlayerName { get; set; }

        public int TotalWins { get; set; }

        public int TotalLosses { get; set; }
    }
}
