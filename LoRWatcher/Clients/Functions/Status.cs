using Newtonsoft.Json;

namespace LoRWatcher.Clients.Functions
{
    public class Status
    {
        [JsonProperty("message")]
        public string Message { get; set; }

        [JsonProperty("status_code")]
        public string StatusCode { get; set; }
    }
}
