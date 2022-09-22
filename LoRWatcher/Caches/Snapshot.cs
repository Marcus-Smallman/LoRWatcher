using LoRWatcher.Clients;
using System.Collections.Generic;

namespace LoRWatcher.Caches
{
    public class Snapshot
    {
        public int ScreenWidth { get; set; }

        public int ScreenHeight { get; set; }

        public IEnumerable<CardPosition> Rectangles { get; set; }
    }
}
