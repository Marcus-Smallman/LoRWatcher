using LoRWatcher.Logger;
using LoRWatcher.Utils;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace LoRWatcher.Clients.Functions
{
    public class FunctionsClient
        : IFunctionsClient
    {
        private readonly HttpClient httpClient;

        private readonly ILogger logger;

        public FunctionsClient(HttpClient httpClient, ILogger logger)
        {
            this.httpClient = httpClient;
            this.logger = logger;
        }

        public async Task<Account> GetAccountAsync(string gameName, string tagLine, string region, CancellationToken cancellationToken = default)
        {
            try
            {
                // TODO: Have possibly other functions on other regions for faster responses for users over the world
                using var request = new HttpRequestMessage(HttpMethod.Post, FunctionUrls.GetAccount);
                request.Content = new StringContent(
                    JsonConvert.SerializeObject(
                        new AccountRequest
                        {
                            GameName = gameName,
                            TagLine = tagLine,
                            Region = region
                        }),
                    Encoding.UTF8,
                    "application/json");
                var result = await this.httpClient.SendAsync(request, cancellationToken);
                if (result.IsSuccessStatusCode == true)
                {
                    var content = await result.Content.ReadAsStringAsync();
                    try
                    {
                        return JsonConvert.DeserializeObject<Account>(content); ;
                    }
                    catch
                    {
                        var error = JsonConvert.DeserializeObject<Error>(content);

                        this.logger.Error($"Error returned from function: {error?.Status?.Message} - Status Code: {error?.Status.StatusCode}");

                        return null;
                    }
                }

                this.logger.Error($"Unsuccessful response getting account from cloud function|Status code: {result.StatusCode}|Content: {await result.Content.ReadAsStringAsync()}");

                return null;
            }
            catch (Exception ex)
            {
                this.logger.Error($"Error occurred getting account from cloud function: {ex.Message}");
            }

            return null;
        }

        public async Task<Match> GetMatchAsync(string matchId, string region, CancellationToken cancellationToken = default)
        {
            try
            {
                // TODO: Have possibly other functions on other regions for faster responses for users over the world
                using var request = new HttpRequestMessage(HttpMethod.Post, FunctionUrls.GetMatch);
                request.Content = new StringContent(
                    JsonConvert.SerializeObject(new MatchRequest
                    {
                        MatchId = matchId,
                        Region = region
                    }),
                    Encoding.UTF8,
                    "application/json");
                var result = await this.httpClient.SendAsync(request, cancellationToken);
                if (result.IsSuccessStatusCode == true)
                {
                    var content = await result.Content.ReadAsStringAsync();
                    try
                    {
                        return JsonConvert.DeserializeObject<Match>(content);
                    }
                    catch
                    {
                        var error = JsonConvert.DeserializeObject<Error>(content);

                        this.logger.Error($"Error returned from function: {error?.Status?.Message} - Status Code: {error?.Status.StatusCode}");

                        return null;
                    }
                }

                this.logger.Error($"Unsuccessful response getting match from cloud function|Status code: {result.StatusCode}|Content: {await result.Content.ReadAsStringAsync()}");

                return null;
            }
            catch (Exception ex)
            {
                this.logger.Error($"Error occurred getting match from cloud function: {ex.Message}");
            }

            return null;
        }

        public async Task<IEnumerable<string>> GetMatchIdsAsync(string playerId, string region, CancellationToken cancellationToken = default)
        {
            try
            {
                // TODO: Have possibly other functions on other regions for faster responses for users over the world
                using var request = new HttpRequestMessage(HttpMethod.Post, FunctionUrls.GetMatchIds);
                request.Content = new StringContent(
                    JsonConvert.SerializeObject(new MatchIdsRequest
                    {
                        PlayerId = playerId,
                        Region = region
                    }),
                    Encoding.UTF8,
                    "application/json");
                var result = await this.httpClient.SendAsync(request, cancellationToken);
                if (result.IsSuccessStatusCode == true)
                {
                    var content = await result.Content.ReadAsStringAsync();
                    try
                    {
                        return JsonConvert.DeserializeObject<IEnumerable<string>>(content);
                    }
                    catch
                    {
                        var error = JsonConvert.DeserializeObject<Error>(content);

                        this.logger.Error($"Error returned from function: {error?.Status?.Message} - Status Code: {error?.Status.StatusCode}");

                        return null;
                    }
                }

                this.logger.Error($"Unsuccessful response getting match ids from cloud function|Status code: {result.StatusCode}|Content: {await result.Content.ReadAsStringAsync()}");

                return null;
            }
            catch (Exception ex)
            {
                this.logger.Error($"Error occurred getting match ids from cloud function: {ex.Message}");
            }

            return null;
        }
    }
}
