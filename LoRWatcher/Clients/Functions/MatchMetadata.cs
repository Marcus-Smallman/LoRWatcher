using Newtonsoft.Json;
using System.Collections.Generic;

namespace LoRWatcher.Clients.Functions
{
    public class MatchMetadata
    {
        [JsonProperty("data_version")]
        public string DataVerison { get; set; }

        [JsonProperty("match_id")]
        public string MatchId { get; set; }

        [JsonProperty("participants")]
        public IEnumerable<string> Participants { get; set; }
    }
}
