using LoRWatcher.Caches;

namespace LoRWatcher.Services
{
    public class ServiceMatchReport
        : MatchReport
    {
        public string GameMode { get; set; }

        public string GameType { get; set; }
    }
}
