using Newtonsoft.Json;
using System.Collections.Generic;

namespace LoRWatcher.Clients.GitHub
{
    public class GitHubRelease
    {
        [JsonProperty("tag_name")]
        public string TagName { get; set; }

        [JsonProperty("assets")]
        public IEnumerable<GitHubAsset> Assets { get; set; }
    }
}
