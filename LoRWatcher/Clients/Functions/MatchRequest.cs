using Newtonsoft.Json;

namespace LoRWatcher.Clients.Functions
{
    public class MatchRequest
    {
        [JsonProperty("matchId")]
        public string MatchId { get; set; }

        [JsonProperty("region")]
        public string Region { get; set; }
    }
}
