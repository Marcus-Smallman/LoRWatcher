using System;

namespace LoRWatcher.Logger
{
    public class ConsoleLogger
        : ILogger
    {
        public void Info(string message)
        {
            this.Log(LogLevel.Info, message);
        }

        public void Debug(string message)
        {
            this.Log(LogLevel.Debug, message);
        }

        public void Warning(string message)
        {
            this.Log(LogLevel.Warning, message);
        }

        public void Error(string message)
        {
            this.Log(LogLevel.Error, message);
        }

        public void Log(LogLevel logLevel, string message)
        {
            var timestamp = DateTime.UtcNow.ToString("dd-MM-yyyy HH:mm:ss.fff");

            Console.WriteLine($"|{timestamp}|{logLevel.ToString()}|{message}|");
        }
    }
}
