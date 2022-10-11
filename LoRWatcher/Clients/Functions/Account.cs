using Newtonsoft.Json;

namespace LoRWatcher.Clients.Functions
{
    public class Account
    {
        [JsonProperty("puuid")]
        public string PlayerId { get; set; }

        [JsonProperty("gameName")]
        public string GameName { get; set; }

        [JsonProperty("tagLine")]
        public string TagLine { get; set; }
    }
}
