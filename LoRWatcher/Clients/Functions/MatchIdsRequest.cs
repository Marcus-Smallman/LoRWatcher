using Newtonsoft.Json;

namespace LoRWatcher.Clients.Functions
{
    public class MatchIdsRequest
    {
        [JsonProperty("playerId")]
        public string PlayerId { get; set; }

        [JsonProperty("region")]
        public string Region { get; set; }
    }
}
