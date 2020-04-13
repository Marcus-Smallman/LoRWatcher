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

namespace LoRWatcher
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var host = CreateHostBuilder(args).Build();

            var configuration = host.Services.GetService<IConfiguration>();
            var logger = host.Services.GetService<ILogger>();

            Task.Factory.StartNew(() => ConfigureTrayIcon(configuration, logger));

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

        public static void ConfigureTrayIcon(IConfiguration configuration, ILogger logger)
        {
            logger.Debug("Configuring tray icon");

            try
            {
                var icon = new NotifyIcon();
                icon.Icon = new Icon("./wwwroot/favicon.ico");
                icon.Visible = true;
                icon.ShowBalloonTip(2000, "LoR Watcher", "Running", ToolTipIcon.None);

                var browserItem = new ToolStripMenuItem();
                browserItem.Text = "Browser";
                browserItem.Click += new EventHandler((s, e) =>
                {
                    var url = $"http://{configuration["Client:Address"]}:{configuration["Client:Port"]}";
                    Process.Start(new ProcessStartInfo("cmd", $"/c start {url}"));
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
                contextMenuStrip.Items.AddRange(new[] { browserItem, exitItem });

                icon.ContextMenuStrip = contextMenuStrip;

                Application.Run();
            }
            catch (Exception ex)
            {
                logger.Error(ex.Message);
            }
        }
    }
}
