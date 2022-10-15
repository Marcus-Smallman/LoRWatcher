using Newtonsoft.Json;

namespace LoRWatcher.Clients.Functions
{
    public class Account
    {
        [JsonProperty("puuid", Required = Required.Always)]
        public string PlayerId { get; set; }

        [JsonProperty("gameName", Required = Required.Always)]
        public string GameName { get; set; }

        [JsonProperty("tagLine", Required = Required.Always)]
        public string TagLine { get; set; }
    }
}
