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

        private readonly LoRConfiguration loRWatcherConfiguration;

        private readonly ILogger logger;

        public LoRClient(HttpClient httpClient, LoRConfiguration loRWatcherConfiguration, ILogger logger)
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

                    this.logger.Error($"Unsuccessful response getting static decklist|Status code: {result.StatusCode}|Content: {await result.Content.ReadAsStringAsync()}");

                    return null;
                });
            }
            catch (Exception ex)
            {
                this.logger.Error($"Error occurred getting static decklist: {ex.Message}");
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
                        // TODO: Support all ascii characters in deserialization
                        var content = await result.Content.ReadAsStringAsync();
                        var positionalRectangles = JsonConvert.DeserializeObject<PositionalRectangles>(content);

                        return positionalRectangles;
                    }

                    this.logger.Error($"Unsuccessful response getting positional rectangles|Status code: {result.StatusCode}|Content: {await result.Content.ReadAsStringAsync()}");

                    return null;
                });
            }
            catch (Exception ex)
            {
                this.logger.Error($"Error occurred getting positional rectangles: {ex.Message}");
            }

            return null;
        }

        public async Task<ExpeditionsState> GetExpeditionsStateAsync(CancellationToken cancellationToken)
        {
            try
            {
                return await Retry.InvokeAsync(async () =>
                {
                    using var request = new HttpRequestMessage(HttpMethod.Get, $"http://{this.loRWatcherConfiguration.Address}:{this.loRWatcherConfiguration.Port}/expeditions-state");
                    var result = await this.httpClient.SendAsync(request, cancellationToken);
                    if (result.IsSuccessStatusCode == true)
                    {
                        var content = await result.Content.ReadAsStringAsync();
                        var expeditionsState = JsonConvert.DeserializeObject<ExpeditionsState>(content);

                        return expeditionsState;
                    }

                    this.logger.Error($"Unsuccessful response getting expedition state|Status code: {result.StatusCode}|Content: {await result.Content.ReadAsStringAsync()}");

                    return null;
                });
            }
            catch (Exception ex)
            {
                this.logger.Error($"Error occurred getting expedition state: {ex.Message}");
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

                    this.logger.Error($"Unsuccessful response getting game result|Status code: {result.StatusCode}|Content: {await result.Content.ReadAsStringAsync()}");

                    return null;
                });
            }
            catch (Exception ex)
            {
                this.logger.Error($"Error occurred getting game result: {ex.Message}");
            }

            return null;
        }

        public async Task<bool> IsClientActiveAsync(CancellationToken cancellationToken)
        {
            try
            {
                try
                {
                    return await Retry.InvokeAsync(async () =>
                    {
                        using var request = new HttpRequestMessage(HttpMethod.Get, $"http://{this.loRWatcherConfiguration.Address}:{this.loRWatcherConfiguration.Port}/game-result");
                        var result = await this.httpClient.SendAsync(request, cancellationToken);
                        if (result.IsSuccessStatusCode == true)
                        {
                            return true;
                        }

                        return false;
                    });
                }
                catch (HttpRequestException ex)
                {
                    if (ex.Message != "No connection could be made because the target machine actively refused it.")
                    {
                        throw;
                    }

                    this.logger.Warning($"Game client not active");
                }
            }
            catch (Exception ex)
            {
                this.logger.Error($"Error checking if client is active: {ex.Message}");
            }

            return false;
        }
    }
}
