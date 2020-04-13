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

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton<HttpClient>();

            services.AddSingleton<ILogger, ConsoleLogger>();

            services.AddSingleton<LoRWatcherConfiguration>(s => new LoRWatcherConfiguration
            {
                Address = this.Configuration["LoR:Address"],
                Port = int.Parse(this.Configuration["LoR:Port"])
            });

            services.AddTransient<IGameClient, LoRClient>();

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
