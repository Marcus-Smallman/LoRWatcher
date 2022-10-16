using LiteDB;

namespace LoRWatcher.Stores.Documents
{
    public class MatchIdDocument
    {
        [BsonId]
        public string Id { get; set; }

        public bool Synced { get; set; }
    }
}
