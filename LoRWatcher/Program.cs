﻿using LoRWatcher.Logger;
using LoRWatcher.Tray;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Diagnostics;
using System.IO;
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
    }
}
