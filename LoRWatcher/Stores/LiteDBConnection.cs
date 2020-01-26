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
            // TODO: Encrypt database with a password e.g. ;Password={Guid.NewGuid().ToString()} and store it
            // in a place you cannot get access to and use that as the password on startup.
            // Generate the database password on first time startup.
            return new LiteDatabase($@"Filename={Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)}\{DatabaseName}");
        }
    }
}
