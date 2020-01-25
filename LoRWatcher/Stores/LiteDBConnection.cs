using LiteDB;
using System;

namespace LoRWatcher.Stores
{
    public class LiteDBConnection
        : IConnection<LiteDatabase>,
        IDisposable
    {
        private const string DatabaseName = "LoRWatcherDB";

        private LiteDatabase database;

        public void Dispose()
        {
            this.database.Dispose();
        }

        public LiteDatabase GetConnection()
        {
            if (this.database == null)
            {
                this.database = new LiteDatabase($@"{Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)}\{DatabaseName}");
            }

            return this.database;
        }
    }
}
