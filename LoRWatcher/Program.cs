using LoRWatcher.Configuration;
using LoRWatcher.Clients;
using LoRWatcher.Watchers;
using Microsoft.Extensions.DependencyInjection;
using System.Net.Http;
using System.Threading.Tasks;
using LoRWatcher.Caches;
using LoRWatcher.Logger;

namespace LoRWatcher
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var serviceCollection = new ServiceCollection();

            serviceCollection.AddSingleton<HttpClient>();

            serviceCollection.AddSingleton<ILogger, ConsoleLogger>();

            serviceCollection.AddSingleton<LoRWatcherConfiguration>(s => new LoRWatcherConfiguration
            {
                Address = "localhost",
                Port = 21337
            });

            serviceCollection.AddSingleton<LoRServiceConfiguration>(s => new LoRServiceConfiguration
            {
                UrlScheme = "http",
                UrlEndpoint = "localhost:5000"
            });

            serviceCollection.AddTransient<IGameClient, LoRClient>();
            serviceCollection.AddTransient<IServiceClient, LoRServiceClient>();

            serviceCollection.AddSingleton<ICache, ActiveGameCache>();
            serviceCollection.AddSingleton<IWatcher, LoRPollWatcher>();

            var services = serviceCollection.BuildServiceProvider();

            var watcher = services.GetRequiredService<IWatcher>();

            await watcher.StartAsync();
        }
    }
}
