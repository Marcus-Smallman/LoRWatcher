using LoRWatcher.Logger;
using LoRWatcher.Services;
using LoRWatcher.Tray;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace LoRWatcher
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var host = CreateHostBuilder(args).Build();

            var trayIcon = host.Services.GetService<ITrayIcon>();

            var tokenSource = new CancellationTokenSource();
            var logger = new FileLogger();

            Task.Factory.StartNew(() => trayIcon.Configure(tokenSource));
            Task.Factory.StartNew(async () =>
            {
                try
                {
                    var store = host.Services.GetRequiredService<IWatcherService>();

                    await store.InitialiseMetadataAsync(CancellationToken.None);
                    await store.SyncMatchReportsAsync(CancellationToken.None);
                }
                catch (Exception ex)
                {
                    logger.Error($"Error initialising store: {ex.Message}");
                }
            });

            try
            {

                host.RunAsync(tokenSource.Token).Wait();
            }
            catch (Exception ex)
            {
                logger.Error($"Error occurred during host execution: {ex.Message}");
            }

            if (tokenSource.IsCancellationRequested == true)
            {
                try
                {
                    var process = Process.GetCurrentProcess();

                    Process.Start(process.MainModule.FileName);
                }
                catch (Exception ex)
                {
                    logger.Error($"Error occurred restarting Watcher: {ex.Message}");
                }
            }
        }

        public static IHostBuilder CreateHostBuilder(string[] args)
        {
            var configurationFilePath = GetConfigurationFilePath();
            if (string.IsNullOrEmpty(configurationFilePath) == true)
            {
                Environment.Exit(1);
            }

            var configuration = new ConfigurationBuilder()
                    .SetBasePath(Directory.GetCurrentDirectory())
                    .AddJsonFile(configurationFilePath, optional: false, reloadOnChange: true)
                    .Build();

            return Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseConfiguration(configuration);
                    webBuilder.UseUrls($"http://{configuration["Client:Address"]}:{configuration["Client:Port"]}");
                    webBuilder.UseSetting(WebHostDefaults.DetailedErrorsKey, "true");
                    webBuilder.UseStartup<Startup>();
                });
        }

        public static string GetConfigurationFilePath()
        {
            try
            {
                var configurationDirectory = $@"{Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)}\LoR Watcher\Configuration";
                if (Directory.Exists(configurationDirectory) == false)
                {
                    Directory.CreateDirectory(configurationDirectory);
                }

                var configurationFilePath = @$"{configurationDirectory}\appsettings.json";
                if (File.Exists(configurationFilePath) == false)
                {
                    File.Copy($@"{Directory.GetCurrentDirectory()}\appsettings.default.json", configurationFilePath, true);
                }

                return configurationFilePath;
            }
            catch (Exception ex)
            {
                var logger = new FileLogger();

                logger.Error($"Error occurred getting configuration: {ex.Message}");
            }

            return null;
        }

        public static Version GetVersion()
        {
            return Assembly.GetExecutingAssembly().GetName().Version;
        }
    }
}
