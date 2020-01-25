using System;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using LoRWatcher.Caches;
using LoRWatcher.Configuration;
using LoRWatcher.Logger;
using LoRWatcher.Utils;
using Newtonsoft.Json;

namespace LoRWatcher.Clients
{
    public class LoRServiceClient
        : IServiceClient
    {
        private readonly HttpClient httpClient;

        private readonly LoRServiceConfiguration loRServiceConfiguration;

        private readonly ILogger logger;

        public LoRServiceClient(HttpClient httpClient, LoRServiceConfiguration loRServiceConfiguration, ILogger logger)
        {
            this.httpClient = httpClient;
            this.loRServiceConfiguration = loRServiceConfiguration;
            this.logger = logger;
        }

        public async Task<bool> ReportGameAsync(MatchReport matchReport, CancellationToken cancellationToken)
        {
            try
            {
                return await Retry.InvokeAsync(async () =>
                {
                    using var request = new HttpRequestMessage(HttpMethod.Post, $"{this.loRServiceConfiguration.UrlScheme}://{this.loRServiceConfiguration.UrlEndpoint}/api/v1/match/report");

                    request.Content = new StringContent(JsonConvert.SerializeObject(matchReport), Encoding.UTF8, "application/json");

                    var result = await this.httpClient.SendAsync(request, cancellationToken);
                    if (result.IsSuccessStatusCode == true)
                    {
                        return true;
                    }

                    return false;
                });
            }
            catch (Exception ex)
            {
                this.logger.Error($"Error occurred reporting match: {ex.Message}");

                // Return errored response
            }

            return false;
        }
    }
}
