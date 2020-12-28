using LoRWatcher.Configuration;
using LoRWatcher.Logger;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Drawing;
using System.IO;
using System.Threading;
using System.Windows.Forms;

namespace LoRWatcher.Tray
{
    public partial class SettingsForm : Form
    {
        protected LoRConfiguration lorConfiguration;

        protected WatcherConfiguration watcherConfiguration;

        protected LoggerSettings loggerSettings;

        protected CancellationTokenSource cancellationTokenSource;

        protected NotifyIcon trayIcon;

        public SettingsForm(
            LoRConfiguration lorConfiguration,
            WatcherConfiguration watcherConfiguration,
            LoggerSettings loggerSettings,
            CancellationTokenSource tokenSource,
            NotifyIcon trayIcon)
        {
            this.lorConfiguration = lorConfiguration;
            this.watcherConfiguration = watcherConfiguration;
            this.loggerSettings = loggerSettings;
            this.cancellationTokenSource = tokenSource;
            this.trayIcon = trayIcon;

            InitializeComponent();

            using (var stream = File.OpenRead($@"{Directory.GetCurrentDirectory()}\wwwroot\favicon.ico"))
            {
                this.Icon = new Icon(stream);
            }
        }

        private void button2_Click(object sender, System.EventArgs e)
        {
            this.Close();
        }

        private void button1_Click(object sender, System.EventArgs e)
        {
            // TODO: Write a better configuration writer.
            if (ValidateConfiguration(out string errorMessage) == false)
            {
                MessageBox.Show(errorMessage, "Invalid configuration", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            else
            {
                try
                {
                    var configJson = JsonConvert.DeserializeObject<JObject>(File.ReadAllText(watcherConfiguration.ConfigurationFilePath));

                    configJson["LoR"]["Address"] = this.textBox1.Text;
                    configJson["LoR"]["Port"] = int.Parse(this.textBox2.Text);

                    configJson["Client"]["Port"] = int.Parse(this.textBox3.Text);

                    configJson["LoggerSettings"]["WriteToFile"] = this.checkBox1.Checked;
                    configJson["LoggerSettings"]["FileDirectory"] = this.textBox4.Text;
                    configJson["LoggerSettings"]["CleanupPeriodMinutes"] = int.Parse(this.textBox5.Text);

                    File.WriteAllText(watcherConfiguration.ConfigurationFilePath, JsonConvert.SerializeObject(configJson, Formatting.Indented));

                    var confirmResult = MessageBox.Show("Restart for changes to take affect", "Restart Watcher", MessageBoxButtons.YesNo, MessageBoxIcon.Information);
                    if (confirmResult == DialogResult.Yes)
                    {
                        this.trayIcon.Dispose();
                        this.cancellationTokenSource.Cancel();

                        this.Close();
                    }
                }
                catch (Exception ex)
                {
                    var logger = new FileLogger();

                    logger.Error($"Error occurred saving configuration: {ex.Message}");
                }
            }
        }

        private bool ValidateConfiguration(out string errorMessage)
        {
            errorMessage = null;

            var lorPortResult = int.TryParse(this.textBox2.Text, out int lorPort);
            if (lorPortResult == false)
            {
                errorMessage = "Invalid LoR Port";

                return false;
            }
            else if (lorPort < 0 ||
                     lorPort > 65535)
            {
                errorMessage = "LoR Port must be within the range 0 and 65535";

                return false;
            }

            var watcherPortResult = int.TryParse(this.textBox3.Text, out int watcherPort);
            if (watcherPortResult == false)
            {
                errorMessage = "Invalid Watcher Port";

                return false;
            }
            else if (watcherPort < 0 ||
                     watcherPort > 65535)
            {
                errorMessage = "Watcher Port must be within the range 0 and 65535";

                return false;
            }

            var cleanupPeriodMinutesResult = int.TryParse(this.textBox5.Text, out _);
            if (cleanupPeriodMinutesResult == false)
            {
                errorMessage = "Invalid Cleanup Period Minutes Value";

                return false;
            }

            return true;
        }
    }
}
