using Newtonsoft.Json;
using System.Collections.Generic;

namespace LoRWatcher.Clients.Functions
{
    public class MatchInfo
    {
        [JsonProperty("game_mode")]
        public string GameMode { get; set; }

        [JsonProperty("game_type")]
        public string GameType { get; set; }

        [JsonProperty("game_start_time_utc")]
        public string GameStartTimeUTC { get; set; }

        [JsonProperty("game_version")]
        public string GameVersion { get; set; }

        [JsonProperty("players")]
        public IEnumerable<Player> Players { get; set; }

        [JsonProperty("total_turn_count")]
        public int TotalTurnCount { get; set; }
    }
}
