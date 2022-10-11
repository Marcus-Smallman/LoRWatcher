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

        public async Task<Account> GetAccountAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                return await Retry.InvokeAsync(async () =>
                {
                    var getAccountFunctionUrl = "https://faas-lon1-917a94a7.doserverless.co/api/v1/web/fn-b47060c4-27f8-4dfb-8463-8bb3b963c7f6/default/riot-apis-get-account";

                    using var request = new HttpRequestMessage(HttpMethod.Get, getAccountFunctionUrl);
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

        public async Task<Match> GetMatchAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                return await Retry.InvokeAsync(async () =>
                {
                    var getMatchFunctionUrl = "https://faas-lon1-917a94a7.doserverless.co/api/v1/web/fn-b47060c4-27f8-4dfb-8463-8bb3b963c7f6/default/riot-apis-get-match";

                    using var request = new HttpRequestMessage(HttpMethod.Get, getMatchFunctionUrl);
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

        public async Task<IEnumerable<string>> GetMatchIdsAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                return await Retry.InvokeAsync(async () =>
                {
                    var getMatchIdsFunctionUrl = "https://faas-lon1-917a94a7.doserverless.co/api/v1/web/fn-b47060c4-27f8-4dfb-8463-8bb3b963c7f6/default/riot-apis-get-match-ids";

                    using var request = new HttpRequestMessage(HttpMethod.Get, getMatchIdsFunctionUrl);
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
