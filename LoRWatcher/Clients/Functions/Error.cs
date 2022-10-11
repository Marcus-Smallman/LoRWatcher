using Newtonsoft.Json;

namespace LoRWatcher.Clients.Functions
{
    public class Error
    {
        [JsonProperty("status")]
        public Status Status { get; set; }
    }
}
