namespace LoRWatcher.Logger
{
    public class LoggerSettings
    {
        public bool WriteToFile { get; set; }

        public string FileDirectory { get; set; }

        public double CleanupPeriodMinutes { get; set; }
    }
}
