using LiteDB;
using LoRWatcher.Caches;
using LoRWatcher.Clients;
using LoRWatcher.Configuration;
using LoRWatcher.Logger;
using LoRWatcher.Stores;
using LoRWatcher.Watchers;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Net.Http;

namespace LoRWatcher
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton<HttpClient>();

            services.AddSingleton<ILogger, ConsoleLogger>();

            services.AddSingleton<LoRWatcherConfiguration>(s => new LoRWatcherConfiguration
            {
                Address = "localhost",
                Port = 21337
            });

            services.AddSingleton<LoRServiceConfiguration>(s => new LoRServiceConfiguration
            {
                UrlScheme = "http",
                UrlEndpoint = "localhost:5000"
            });

            services.AddTransient<IGameClient, LoRClient>();
            //serviceCollection.AddTransient<IServiceClient, LoRServiceClient>();

            services.AddTransient<IWatcherDataStore, LiteDBWatcherDataStore>();

            services.AddSingleton<IConnection<LiteDatabase>, LiteDBConnection>();

            services.AddSingleton<ICache, ActiveGameCache>();

            services.AddHostedService<LoRPollWatcher>();

            // Add razor.
            services.AddRazorPages();
            services.AddServerSideBlazor();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Error");
            }

            app.UseStaticFiles();

            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapBlazorHub();
                endpoints.MapFallbackToPage("/_Host");
            });
        }
    }
}
