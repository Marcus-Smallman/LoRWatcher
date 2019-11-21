using MongoDB.Bson.Serialization.Attributes;

namespace LoRService.Services.MongoDB.Documents
{
    public class MatchReportDocument
    {
        public string MatchId { get; set; }

        [BsonId]
        public string PlayerName { get; set; }

        public string PlayerDeckCode { get; set; }

        public string OpponentName { get; set; }

        public bool Result { get; set; }
    }
}
