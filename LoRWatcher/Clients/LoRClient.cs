using LoRWatcher.Configuration;
using Newtonsoft.Json;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace LoRWatcher.Clients
{
    public class LoRClient
        : IGameClient
    {
        private readonly HttpClient httpClient;

        private readonly LoRWatcherConfiguration loRWatcherConfiguration;

        public LoRClient(HttpClient httpClient, LoRWatcherConfiguration loRWatcherConfiguration)
        {
            this.httpClient = httpClient;
            this.loRWatcherConfiguration = loRWatcherConfiguration;
        }

        public async Task<StaticDecklist> GetActiveDecklistAsync()
        {
            try
            {
                // Add retry
                using var request = new HttpRequestMessage(HttpMethod.Get, $"http://{this.loRWatcherConfiguration.Address}:{this.loRWatcherConfiguration.Port}/static-decklist");
                var result = await this.httpClient.SendAsync(request);
                if (result.IsSuccessStatusCode == true)
                {
                    var content = await result.Content.ReadAsStringAsync();
                    var staticDecklist = JsonConvert.DeserializeObject<StaticDecklist>(content);

                    return staticDecklist;
                }
            }
            catch
            {
                // Log error
            }

            // Return errored response
            return null;
        }

        public async Task<PositionalRectangles> GetCardPositionsAsync()
        {
            try
            {
                // Add retry
                using var request = new HttpRequestMessage(HttpMethod.Get, $"http://{this.loRWatcherConfiguration.Address}:{this.loRWatcherConfiguration.Port}/positional-rectangles");
                var result = await this.httpClient.SendAsync(request);
                if (result.IsSuccessStatusCode == true)
                {
                    var content = await result.Content.ReadAsStringAsync();
                    var positionalRectangles = JsonConvert.DeserializeObject<PositionalRectangles>(content);

                    return positionalRectangles;
                }
            }
            catch
            {
                // Log error
            }

            // Return errored response
            return null;
        }

        public async Task<GameResult> GetGameResult()
        {
            try
            {
                // Add retry
                using var request = new HttpRequestMessage(HttpMethod.Get, $"http://{this.loRWatcherConfiguration.Address}:{this.loRWatcherConfiguration.Port}/game-result");
                var result = await this.httpClient.SendAsync(request);
                if (result.IsSuccessStatusCode == true)
                {
                    var content = await result.Content.ReadAsStringAsync();
                    var gameResult = JsonConvert.DeserializeObject<GameResult>(content);

                    return gameResult;
                }
            }
            catch
            {
                // Log error
            }

            // Return errored response
            return null;
        }
    }
}
