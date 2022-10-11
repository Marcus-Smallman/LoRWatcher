using Newtonsoft.Json;

namespace LoRWatcher.Clients.Functions
{
    public class Match
    {
        [JsonProperty("metadata")]
        public MatchMetadata Metadata { get; set; }

        [JsonProperty("info")]
        public MatchInfo Info { get; set; }
    }
}
