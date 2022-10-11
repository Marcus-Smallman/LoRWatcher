using Newtonsoft.Json;

namespace LoRWatcher.Clients.Functions
{
    public class AccountRequest
    {
        [JsonProperty("gameName")]
        public string GameName { get; set; }

        [JsonProperty("tagLine")]
        public string TagLine { get; set; }

        [JsonProperty("region")]
        public string Region { get; set; }
    }
}
