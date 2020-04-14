using Microsoft.Extensions.Hosting;
using Microsoft.AspNetCore.Hosting;
using System;
using LoRWatcher.Logger;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using System.Diagnostics;
using Microsoft.Extensions.Configuration;
using System.IO;
using System.Windows.Forms;
using System.Drawing;
using LoRWatcher.Caches;
using LoRWatcher.Clients;

namespace LoRWatcher
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var host = CreateHostBuilder(args).Build();

            var configuration = host.Services.GetService<IConfiguration>();
            var gameStateCache = host.Services.GetService<IGameStateCache>();
            var logger = host.Services.GetService<ILogger>();

            Task.Factory.StartNew(() => ConfigureTrayIcon(configuration, gameStateCache, logger));

            host.Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args)
        {
            var configuration = new ConfigurationBuilder()
                    .SetBasePath(Directory.GetCurrentDirectory())
                    .AddJsonFile("appsettings.json", optional: false)
                    .Build();

            return Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseConfiguration(configuration);
                    webBuilder.UseUrls($"http://{configuration["Client:Address"]}:{configuration["Client:Port"]}");
                    webBuilder.UseStartup<Startup>();
                });
        }

        public static void ConfigureTrayIcon(IConfiguration configuration, IGameStateCache gameStateCache, ILogger logger)
        {
            logger.Debug("Configuring tray icon");

            try
            {
                var icon = new NotifyIcon();
                icon.Icon = new Icon("./wwwroot/favicon.ico");
                icon.Visible = true;
                icon.ShowBalloonTip(2000, "LoR Watcher", "Running", ToolTipIcon.None);

                var statusItem = new ToolStripLabel();
                statusItem.DisplayStyle = ToolStripItemDisplayStyle.ImageAndText;

                var browserItem = new ToolStripMenuItem();
                browserItem.Text = "Browser";
                browserItem.Click += new EventHandler((s, e) =>
                {
                    var url = $"http://{configuration["Client:Address"]}:{configuration["Client:Port"]}";
                    Process.Start(new ProcessStartInfo(url) { UseShellExecute = true });
                });

                var exitItem = new ToolStripMenuItem();
                exitItem.Text = "Exit";
                exitItem.Click += new EventHandler((s, e) =>
                {
                    logger.Info("Exiting application");

                    icon.Dispose();
                    Application.Exit();
                    Environment.Exit(0);
                });

                var contextMenuStrip = new ContextMenuStrip();
                contextMenuStrip.Items.Add(statusItem);
                contextMenuStrip.Items.Add(new ToolStripSeparator());
                contextMenuStrip.Items.AddRange(new[] { browserItem, exitItem });

                icon.ContextMenuStrip = contextMenuStrip;
                icon.Click += (s, e) =>
                {
                    var gameState = gameStateCache.GetGameState();
                    statusItem.Text = $"{GameStateCacheExtensions.GetHumanReadableGameState(gameState)}";
                    
                    switch (gameState)
                    {
                        // TODO: Figure out how to get the image in the left box
                        case GameState.InProgress:
                            statusItem.Image = GetGameStateIcon(Color.Blue);
                            break;
                        case GameState.Menus:
                            statusItem.Image = GetGameStateIcon(Color.Green);
                            break;
                        default:
                            statusItem.Image = GetGameStateIcon(Color.Red);
                            break;
                    }
                };

                Application.Run();
            }
            catch (Exception ex)
            {
                logger.Error(ex.Message);
            }
        }

        private static Image GetGameStateIcon(Color colour)
        {
            var length = 64;
            var bitmap = new Bitmap(length, length);
            var graphics = Graphics.FromImage(bitmap);
            var brush = new SolidBrush(colour);
            var x = length / 4;
            var y = length / 4;
            var width = length / 2;
            var height = length / 2;
            var diameter = Math.Min(width, height);

            graphics.FillEllipse(brush, x, y, diameter, diameter);

            return bitmap;
        }
    }
}
