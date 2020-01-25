using LiteDB;
using System;

namespace LoRWatcher.Stores
{
    public class LiteDBConnection
        : IConnection<LiteDatabase>
    {
        private const string DatabaseName = "LoRWatcherDB";

        public LiteDatabase GetConnection()
        {
            return new LiteDatabase($@"{Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)}\{DatabaseName}");
        }
    }
}
