using LoRWatcher.Caches;
using LoRWatcher.Clients;
using LoRWatcher.Clients.GitHub;
using LoRWatcher.Configuration;
using LoRWatcher.Logger;
using LoRWatcher.Utils;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
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

                var updatesItem = new ToolStripMenuItem();
                updatesItem.Text = "Check For Updates";
                updatesItem.Click += new EventHandler(async (s, e) =>
                {
                    try
                    {
                        var githubClient = serviceProvider.GetRequiredService<IGitHubClient>();
                        var latestRelease = await githubClient.GetLatestVersionAsync();
                        if (latestRelease != null)
                        {
                            if (latestRelease.TagName == Program.GetVersion().ToString(3))
                            {
                                MessageBox.Show("No new version found.", "Update Watcher");
                            }
                            else
                            {
                                var confirmResult = MessageBox.Show($"New version found: {latestRelease.TagName}\n\nDownload update and start installation?", "Update Watcher", MessageBoxButtons.YesNo, MessageBoxIcon.Information);
                                if (confirmResult == DialogResult.Yes)
                                {
                                    var installerNameExe = $"LoR.Watcher.Installer.{latestRelease.TagName}.exe";
                                    var installerFilePath = await this.DownloadInstallerAsync(latestRelease.Assets.FirstOrDefault(a => a.DownloadUrl.EndsWith(installerNameExe)).DownloadUrl, installerNameExe);

                                    var installerStartInfo = new ProcessStartInfo();
                                    installerStartInfo.CreateNoWindow = false;
                                    installerStartInfo.UseShellExecute = false;
                                    installerStartInfo.FileName = installerFilePath;
                                    installerStartInfo.WindowStyle = ProcessWindowStyle.Normal;

                                    try
                                    {
                                        using var installerExe = Process.Start(installerStartInfo);
                                    }
                                    catch (Exception ex)
                                    {
                                        logger.Error($"Failed to wait for installer to complete: {ex.Message}");
                                    }
                                }
                            }
                        }
                        else
                        {
                            MessageBox.Show("Failed to check for new version.", "Update Watcher", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
                    catch (Exception ex)
                    {
                        logger.Error($"Error occurred checking for updates: {ex.Message}");
                    }
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
                contextMenuStrip.Items.AddRange(new[] { browserItem, settingsItem });
                contextMenuStrip.Items.Add(new ToolStripSeparator());
                contextMenuStrip.Items.AddRange(new[] { updatesItem, exitItem });

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

        private async Task<string> DownloadInstallerAsync(string downloadUrl, string installerNameExe)
        {
            var tempPath = Path.GetTempPath();
            var httpClient = serviceProvider.GetRequiredService<HttpClient>();

            return await Retry.InvokeAsync(async () =>
            {
                try
                {
                    var installerFilePath = $@"{tempPath}{installerNameExe}";
                    using var result = await httpClient.GetStreamAsync(downloadUrl);
                    using var fileStream = new FileStream(installerFilePath, FileMode.Create);

                    await result.CopyToAsync(fileStream);

                    return installerFilePath;
                }
                catch (Exception ex)
                {
                    this.logger.Error($"Failed to download LoR Watcher installer: {ex.Message}");
                }

                return null;
            });
        }
    }
}
