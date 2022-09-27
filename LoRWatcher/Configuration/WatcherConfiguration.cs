namespace LoRWatcher.Configuration
{
    public class WatcherConfiguration
    {
        public WatcherConfiguration(string configurationFilePath)
        {
            this.ConfigurationFilePath = configurationFilePath;
        }

        public string ConfigurationFilePath { get; }

        public string Address { get; set; }

        public int Port { get; set; }

        public bool StartWithWindows { get; set; }
    }
}
