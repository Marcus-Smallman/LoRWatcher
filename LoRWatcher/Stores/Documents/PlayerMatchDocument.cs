using LiteDB;

namespace LoRWatcher.Stores.Documents
{
    public class PlayerMatchDocument
    {
        [BsonId]
        public string Id { get; set; }

        public MatchMetadataDocument Metadata { get; set; }

        public MatchInfoDocument Info { get; set; }
    }
}
