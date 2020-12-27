using Microsoft.Extensions.Hosting;
using Microsoft.AspNetCore.Hosting;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using System.IO;
using LoRWatcher.Tray;
using System.Threading;
using System.Diagnostics;
using System;
using LoRWatcher.Logger;

namespace LoRWatcher
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var host = CreateHostBuilder(args).Build();

            var trayIcon = host.Services.GetService<ITrayIcon>();

            var tokenSource = new CancellationTokenSource();

            Task.Factory.StartNew(() => trayIcon.Configure(tokenSource));

            try
            {
                host.RunAsync(tokenSource.Token).Wait();
            }
            catch
            {
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
                    var logger = new FileLogger();

                    logger.Error($"Error occurred restarting Watcher: {ex.Message}");
                }
            }
        }

        public static IHostBuilder CreateHostBuilder(string[] args)
        {
            var configuration = new ConfigurationBuilder()
                    .SetBasePath(Directory.GetCurrentDirectory())
                    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                    .Build();

            return Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseConfiguration(configuration);
                    webBuilder.UseUrls($"http://{configuration["Client:Address"]}:{configuration["Client:Port"]}");
                    webBuilder.UseStartup<Startup>();
                });
        }
    }
}
