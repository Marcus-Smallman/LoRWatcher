using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using LoRWatcher.Caches;
using LoRWatcher.Configuration;
using Newtonsoft.Json;

namespace LoRWatcher.Clients
{
    public class LoRServiceClient
        : IServiceClient
    {
        private readonly HttpClient httpClient;

        private readonly LoRServiceConfiguration loRServiceConfiguration;

        public LoRServiceClient(HttpClient httpClient, LoRServiceConfiguration loRServiceConfiguration)
        {
            this.httpClient = httpClient;
            this.loRServiceConfiguration = loRServiceConfiguration;
        }

        public async Task<bool> ReportGameAsync(MatchReport matchReport)
        {
            try
            {
                // Add retry
                using var request = new HttpRequestMessage(HttpMethod.Post, $"{this.loRServiceConfiguration.UrlScheme}://{this.loRServiceConfiguration.UrlEndpoint}/api/v1/match/report");
                
                request.Content = new StringContent(JsonConvert.SerializeObject(matchReport), Encoding.UTF8, "application/json");

                var result = await this.httpClient.SendAsync(request);
                if (result.IsSuccessStatusCode == true)
                {
                    return true;
                }
            }
            catch
            {
                // Log error
            }

            // Return errored response
            return false;
        }
    }
}
