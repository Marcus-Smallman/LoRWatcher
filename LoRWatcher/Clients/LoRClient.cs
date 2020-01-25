using LoRWatcher.Configuration;
using LoRWatcher.Logger;
using LoRWatcher.Utils;
using Newtonsoft.Json;
using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace LoRWatcher.Clients
{
    public class LoRClient
        : IGameClient
    {
        private readonly HttpClient httpClient;

        private readonly LoRWatcherConfiguration loRWatcherConfiguration;

        private readonly ILogger logger;

        public LoRClient(HttpClient httpClient, LoRWatcherConfiguration loRWatcherConfiguration, ILogger logger)
        {
            this.httpClient = httpClient;
            this.loRWatcherConfiguration = loRWatcherConfiguration;
            this.logger = logger;
        }

        public async Task<StaticDecklist> GetActiveDecklistAsync(CancellationToken cancellationToken)
        {
            try
            {
                return await Retry.InvokeAsync(async () =>
                {
                    using var request = new HttpRequestMessage(HttpMethod.Get, $"http://{this.loRWatcherConfiguration.Address}:{this.loRWatcherConfiguration.Port}/static-decklist");
                    var result = await this.httpClient.SendAsync(request, cancellationToken);
                    if (result.IsSuccessStatusCode == true)
                    {
                        var content = await result.Content.ReadAsStringAsync();
                        var staticDecklist = JsonConvert.DeserializeObject<StaticDecklist>(content);

                        return staticDecklist;
                    }

                    return null;
                });
            }
            catch (Exception ex)
            {
                this.logger.Error($"Error occurred getting static decklist: {ex.Message}");

                // Return errored response
            }

            return null;
        }

        public async Task<PositionalRectangles> GetCardPositionsAsync(CancellationToken cancellationToken)
        {
            try
            {
                return await Retry.InvokeAsync(async () =>
                {
                    using var request = new HttpRequestMessage(HttpMethod.Get, $"http://{this.loRWatcherConfiguration.Address}:{this.loRWatcherConfiguration.Port}/positional-rectangles");
                    var result = await this.httpClient.SendAsync(request, cancellationToken);
                    if (result.IsSuccessStatusCode == true)
                    {
                        var content = await result.Content.ReadAsStringAsync();
                        var positionalRectangles = JsonConvert.DeserializeObject<PositionalRectangles>(content);

                        return positionalRectangles;
                    }

                    return null;
                });
            }
            catch (Exception ex)
            {
                this.logger.Error($"Error occurred getting positional rectangles: {ex.Message}");

                // Return errored response
            }

            return null;
        }

        public async Task<GameResult> GetGameResultAsync(CancellationToken cancellationToken)
        {
            try
            {
                return await Retry.InvokeAsync(async () =>
                {
                    using var request = new HttpRequestMessage(HttpMethod.Get, $"http://{this.loRWatcherConfiguration.Address}:{this.loRWatcherConfiguration.Port}/game-result");
                    var result = await this.httpClient.SendAsync(request, cancellationToken);
                    if (result.IsSuccessStatusCode == true)
                    {
                        var content = await result.Content.ReadAsStringAsync();
                        var gameResult = JsonConvert.DeserializeObject<GameResult>(content);

                        return gameResult;
                    }

                    return null;
                });
            }
            catch (Exception ex)
            {
                this.logger.Error($"Error occurred getting game result: {ex.Message}");

                // Return errored response
            }

            return null;
        }
    }
}
