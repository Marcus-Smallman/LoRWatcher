using LoRWatcher.Caches;
using LoRWatcher.Clients;
using LoRWatcher.Logger;
using Microsoft.Extensions.Configuration;
using System;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;

namespace LoRWatcher.Tray
{
    public class TrayIcon
        : ITrayIcon
    {
        private readonly IConfiguration configuration;

        private readonly IGameStateCache gameStateCache;

        private readonly ILogger logger;

        public TrayIcon(IConfiguration configuration, IGameStateCache gameStateCache, ILogger logger)
        {
            this.configuration = configuration;
            this.gameStateCache = gameStateCache;
            this.logger = logger;
        }

        public void Configure()
        {
            logger.Debug("Configuring tray icon");

            try
            {
                var icon = new NotifyIcon();
                icon.Icon = new Icon("./wwwroot/favicon.ico");
                icon.Visible = true;
                icon.ShowBalloonTip(2000, "LoR Watcher", "Running", ToolTipIcon.None);

                var statusItem = new ToolStripMenuItem();

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
