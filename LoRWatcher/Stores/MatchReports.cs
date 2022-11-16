using LoRWatcher.Caches;
using System.Collections.Generic;

namespace LoRWatcher.Stores
{
    public class MatchReports
    {
        public int Count { get; set; }

        public IEnumerable<MatchReport> Matches { get; set; }
    }
}
