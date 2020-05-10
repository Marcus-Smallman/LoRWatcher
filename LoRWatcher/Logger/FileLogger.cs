using System;
using System.IO;
using System.Linq;
using System.Threading;

namespace LoRWatcher.Logger
{
    public class FileLogger
        : ILogger
    {
        private DateTime cleanupTimeUtc;

        private readonly ReaderWriterLock readerWriterLock;

        private readonly LoggerSettings loggerSettings;

        public FileLogger(LoggerSettings loggerSettings = null)
        {
            this.cleanupTimeUtc = DateTime.UtcNow;
            this.readerWriterLock = new ReaderWriterLock();

            loggerSettings ??= new LoggerSettings
            {
                WriteToFile = true,
                CleanupPeriodMinutes = 180 // 180 minutes == 3 hours
            };

            if (string.IsNullOrWhiteSpace(loggerSettings.FileDirectory) == true)
            {
                loggerSettings.FileDirectory = @$"{Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)}\LoR Watcher";
            }

            if (loggerSettings.WriteToFile == true)
            {
                if (Directory.Exists(loggerSettings.FileDirectory) == false)
                {
                    Directory.CreateDirectory(loggerSettings.FileDirectory);
                }
            }

            this.loggerSettings = loggerSettings;
        }

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
            this.Log(LogLevel.Warn, message);
        }

        public void Error(string message)
        {
            this.Log(LogLevel.Error, message);
        }

        public void Log(LogLevel logLevel, string message)
        {
            if (this.loggerSettings.WriteToFile == true)
            {
                this.CleanupLogs();

                this.WriteLog($"|{DateTime.UtcNow.ToString("dd-MM-yyyy HH:mm:ss.fff")}|{logLevel.ToString()}|{message}|");
            }
        }

        private void CleanupLogs()
        {
            var now = DateTime.UtcNow;
            if (now > this.cleanupTimeUtc)
            {
                var filePaths = Directory.GetFiles(this.loggerSettings.FileDirectory, "*.log", SearchOption.TopDirectoryOnly);
                if (filePaths.Any() == true)
                {
                    var oldLogFilePaths = filePaths.Where(filePath => DateTime.Parse(Path.GetFileNameWithoutExtension(filePath).Replace("_", ":")) < 
                                                                     now.Subtract(TimeSpan.FromMinutes(this.loggerSettings.CleanupPeriodMinutes)));
                    foreach (var oldLogFilePath in oldLogFilePaths)
                    {
                        File.Delete(oldLogFilePath);
                    }

                }

                this.cleanupTimeUtc = now.AddMinutes(30);
            }
        }

        private void WriteLog(string message)
        {
            var fileName = DateTime.UtcNow.ToString("dd-MM-yyyy HH_mm_ss");
            var filePath = @$"{this.loggerSettings.FileDirectory}\{fileName}.log";
            var files = Directory.GetFiles(this.loggerSettings.FileDirectory, $"{DateTime.UtcNow.ToString("dd-MM-yyyy HH")}_*.log", SearchOption.TopDirectoryOnly);
            if (files.Any() == true)
            {
                filePath = files.First();
            }

            try
            {
                this.readerWriterLock.AcquireWriterLock(100);
                try
                {
                    File.AppendAllText(filePath, message + Environment.NewLine);
                }
                finally
                {
                    readerWriterLock.ReleaseWriterLock();
                }
            }
            catch
            {
                // There is not much we can do here, so we just have to carry on without the log being written. We could potentially
                // hold failed logs in memory for a while and retry later at some point but this may not be worth adding at this point.
            }
        }
    }
}
