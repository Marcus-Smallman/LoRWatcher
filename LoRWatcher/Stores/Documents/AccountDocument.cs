using LiteDB;

namespace LoRWatcher.Stores.Documents
{
    public class AccountDocument
    {
        [BsonId]
        public string PlayerId { get; set; }

        public string GameName { get; set; }

        public string TagLine { get; set; }
    }
}
