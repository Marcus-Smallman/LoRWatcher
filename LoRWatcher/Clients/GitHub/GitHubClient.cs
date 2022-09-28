using LoRWatcher.Logger;
using LoRWatcher.Utils;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace LoRWatcher.Clients.GitHub
{
    public class GitHubClient
        : IGitHubClient
    {
        private const string BaseUrl = "https://api.github.com/repos/Marcus-Smallman/LoRWatcher";

        private readonly HttpClient httpClient;

        private readonly ILogger logger;

        public GitHubClient(HttpClient httpClient, ILogger logger)
        {
            this.httpClient = httpClient;
            this.logger = logger;
        }

        public async Task<GitHubRelease> GetLatestVersionAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                var result = await Retry.InvokeAsync(async () =>
                {
                    using var request = new HttpRequestMessage(HttpMethod.Get, $"{BaseUrl}/releases");
                    request.Headers.Add("User-Agent", $"LoRWatcher/{Program.GetVersion().ToString(3)}");
                    var result = await this.httpClient.SendAsync(request, cancellationToken);
                    if (result.IsSuccessStatusCode == true)
                    {
                        var content = await result.Content.ReadAsStringAsync();
                        var releases = JsonConvert.DeserializeObject<IEnumerable<GitHubRelease>>(content);

                        return releases;
                    }

                    this.logger.Error($"Unsuccessful response getting GitHub releases for LoR Watcher|Status code: {result.StatusCode}|Content: {await result.Content.ReadAsStringAsync()}");

                    return null;
                });

                if (result != null)
                {
                    var latestRelease = result.FirstOrDefault();

                    return latestRelease;
                }
            }
            catch (Exception ex)
            {
                this.logger.Error($"Error occurred getting GitHub releases for LoR Watcher: {ex.Message}");
            }

            return null;
        }
    }
}
