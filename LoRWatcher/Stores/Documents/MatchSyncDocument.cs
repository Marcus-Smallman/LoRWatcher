using LiteDB;

namespace LoRWatcher.Stores.Documents
{
    public class MatchSyncDocument
    {
        [BsonId]
        public string WatcherMatchId { get; set; }

        public string PlayerMatchId { get; set; }
    }
}
