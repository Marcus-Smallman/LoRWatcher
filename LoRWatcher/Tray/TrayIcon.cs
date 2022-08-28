using LoRWatcher.Caches;
using LoRWatcher.Clients;
using LoRWatcher.Configuration;
using LoRWatcher.Logger;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Diagnostics;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;

namespace LoRWatcher.Tray
{
    public class TrayIcon
        : ITrayIcon
    {
        private readonly IServiceProvider serviceProvider;

        private readonly IGameStateCache gameStateCache;

        private readonly ILogger logger;

        public TrayIcon(IServiceProvider serviceProvider, IGameStateCache gameStateCache, ILogger logger)
        {
            this.serviceProvider = serviceProvider;
            this.gameStateCache = gameStateCache;
            this.logger = logger;
        }

        public void Configure(CancellationTokenSource tokenSource)
        {
            logger.Debug("Configuring tray icon");

            Application.EnableVisualStyles();

            try
            {
                var icon = new NotifyIcon();
                icon.Icon = new Icon("./wwwroot/favicon.ico");
                icon.Visible = true;

                var versionItem = new ToolStripMenuItem();
                versionItem.Text = Application.ProductVersion;
                versionItem.Enabled = false;

                var statusItem = new ToolStripMenuItem();

                var browserItem = new ToolStripMenuItem();
                browserItem.Text = "Browser";
                browserItem.Click += new EventHandler((s, e) =>
                {
                    var watcherConfiguration = serviceProvider.GetRequiredService<WatcherConfiguration>();
                    var url = $"http://{watcherConfiguration.Address}:{watcherConfiguration.Port}";
                    Process.Start(new ProcessStartInfo(url) { UseShellExecute = true });
                });

                var settingsItem = new ToolStripMenuItem();
                settingsItem.Text = "Settings";
                settingsItem.Click += new EventHandler((s, e) =>
                {
                    var configurationForm = new SettingsForm(
                        serviceProvider.GetRequiredService<LoRConfiguration>(),
                        serviceProvider.GetRequiredService<WatcherConfiguration>(),
                        serviceProvider.GetRequiredService<LoggerSettings>(),
                        tokenSource,
                        icon);

                    configurationForm.Show();
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
                contextMenuStrip.Items.Add(versionItem);
                contextMenuStrip.Items.Add(new ToolStripSeparator());
                contextMenuStrip.Items.Add(statusItem);
                contextMenuStrip.Items.Add(new ToolStripSeparator());
                contextMenuStrip.Items.AddRange(new[] { browserItem, settingsItem, exitItem });

                icon.ContextMenuStrip = contextMenuStrip;
                icon.Click += (s, e) =>
                {
                    var gameState = gameStateCache.GetGameState();
                    statusItem.Text = $"{GameStateCacheExtensions.GetHumanReadableGameState(gameState)}";

                    switch (gameState)
                    {
                        case GameState.Startup:
                            statusItem.Image = GetGameStateIcon(Color.Yellow);
                            break;
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

        private Image GetGameStateIcon(Color colour)
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
