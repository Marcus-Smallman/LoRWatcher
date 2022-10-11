using Newtonsoft.Json;
using System.Collections.Generic;

namespace LoRWatcher.Clients.Functions
{
    public class Player
    {
        [JsonProperty("puuid")]
        public string PlayerId { get; set; }

        [JsonProperty("deck_id")]
        public string DeckId { get; set; }

        [JsonProperty("deck_code")]
        public string DeckCode { get; set; }

        [JsonProperty("factions")]
        public IEnumerable<string> Factions { get; set; }

        [JsonProperty("game_outcome")]
        public string GameOutcome { get; set; }

        [JsonProperty("order_of_play")]
        public int OrderOfPlay { get; set; }
    }
}
