using Newtonsoft.Json;

namespace LoRWatcher.Clients.GitHub
{
    public class GitHubAsset
    {
        [JsonProperty("browser_download_url")]
        public string DownloadUrl { get; set; }
    }
}
