namespace LoRWatcher.Logger
{
    public interface ILogger
    {
        void Log(LogLevel logLevel, string message);

        void Info(string message);

        void Debug(string message);

        void Warning(string message);

        void Error(string message);
    }
}
