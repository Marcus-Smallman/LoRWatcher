using LoRWatcher.Logger;
using LoRWatcher.Utils;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
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
                return await Retry.InvokeAsync(async () =>
                {
                    // TODO: Have possibly other functions on other regions for faster responses for users over the world
                    var getAccountFunctionUrl = "https://faas-lon1-917a94a7.doserverless.co/api/v1/web/fn-b47060c4-27f8-4dfb-8463-8bb3b963c7f6/default/riot-apis-get-account";

                    using var request = new HttpRequestMessage(HttpMethod.Post, getAccountFunctionUrl);
                    request.Headers.Add("Content-Type", "application/json");
                    request.Content = new StringContent(JsonConvert.SerializeObject(new AccountRequest
                    {
                        GameName = gameName,
                        TagLine = tagLine,
                        Region = region
                    }));
                    var result = await this.httpClient.SendAsync(request, cancellationToken);
                    if (result.IsSuccessStatusCode == true)
                    {
                        var content = await result.Content.ReadAsStringAsync();
                        try
                        {
                            return JsonConvert.DeserializeObject<Account>(content);
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
                });
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
                return await Retry.InvokeAsync(async () =>
                {
                    // TODO: Have possibly other functions on other regions for faster responses for users over the world
                    var getMatchFunctionUrl = "https://faas-lon1-917a94a7.doserverless.co/api/v1/web/fn-b47060c4-27f8-4dfb-8463-8bb3b963c7f6/default/riot-apis-get-match";

                    using var request = new HttpRequestMessage(HttpMethod.Post, getMatchFunctionUrl);
                    request.Headers.Add("Content-Type", "application/json");
                    request.Content = new StringContent(JsonConvert.SerializeObject(new MatchRequest
                    {
                        MatchId = matchId,
                        Region = region
                    }));
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
                });
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
                return await Retry.InvokeAsync(async () =>
                {
                    // TODO: Have possibly other functions on other regions for faster responses for users over the world
                    var getMatchIdsFunctionUrl = "https://faas-lon1-917a94a7.doserverless.co/api/v1/web/fn-b47060c4-27f8-4dfb-8463-8bb3b963c7f6/default/riot-apis-get-match-ids";

                    using var request = new HttpRequestMessage(HttpMethod.Post, getMatchIdsFunctionUrl);
                    request.Headers.Add("Content-Type", "application/json");
                    request.Content = new StringContent(JsonConvert.SerializeObject(new MatchIdsRequest
                    {
                        PlayerId = playerId,
                        Region = region
                    }));
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
                });
            }
            catch (Exception ex)
            {
                this.logger.Error($"Error occurred getting match ids from cloud function: {ex.Message}");
            }

            return null;
        }
    }
}
