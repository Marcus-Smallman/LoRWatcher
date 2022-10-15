using Newtonsoft.Json;

namespace LoRWatcher.Clients.Functions
{
    public class Error
    {
        [JsonProperty("status", Required = Required.Always)]
        public Status Status { get; set; }
    }
}
