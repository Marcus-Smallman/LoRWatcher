﻿using Blazorise;
using Blazorise.Icons.FontAwesome;
using Blazorise.Material;
using LiteDB;
using LoRWatcher.Caches;
using LoRWatcher.Clients;
using LoRWatcher.Configuration;
using LoRWatcher.Logger;
using LoRWatcher.Stores;
using LoRWatcher.Tray;
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

            services.AddSingleton<LoRConfiguration>(s => new LoRConfiguration
            {
                Address = this.Configuration["LoR:Address"],
                Port = int.Parse(this.Configuration["LoR:Port"])
            });

            services.AddSingleton<WatcherConfiguration>(s => new WatcherConfiguration(Program.GetConfigurationFilePath())
            {
                Address = this.Configuration["Client:Address"],
                Port = int.Parse(this.Configuration["Client:Port"])
            });

            services.AddSingleton<LoggerSettings>(s => new LoggerSettings
            {
                WriteToFile = bool.Parse(this.Configuration["LoggerSettings:WriteToFile"]),
                FileDirectory = this.Configuration["LoggerSettings:FileDirectory"],
                CleanupPeriodMinutes = double.Parse(this.Configuration["LoggerSettings:CleanupPeriodMinutes"])
            });

            services.AddSingleton<ILogger>(s => new FileLogger(s.GetRequiredService<LoggerSettings>()));

            services.AddSingleton<ITrayIcon, TrayIcon>();

            services.AddTransient<IGameClient, LoRClient>();

            services.AddTransient<IWatcherDataStore, LiteDBWatcherDataStore>();

            services.AddSingleton<IConnection<LiteDatabase>, LiteDBConnection>();

            services.AddSingleton<IActiveGameCache, ActiveGameCache>();
            services.AddSingleton<IActiveExpeditionCache, ActiveExpeditionCache>();
            services.AddSingleton<IGameStateCache, GameStateCache>();

            services.AddHostedService<LoRPollWatcher>();

            // Add razor.
            services.AddRazorPages();
            services.AddServerSideBlazor();

            services
                .AddBlazorise(options =>
                {
                    options.ChangeTextOnKeyPress = true;
                })
                .AddMaterialProviders()
                .AddFontAwesomeIcons();
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

            app.ApplicationServices
                  .UseMaterialProviders()
                  .UseFontAwesomeIcons();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapBlazorHub();
                endpoints.MapFallbackToPage("/_Host");
            });
        }
    }
}
