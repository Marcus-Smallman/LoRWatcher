using System.Collections.Generic;

namespace LoRWatcher.Stores.Documents
{
    public class SnapshotDocument
    {
        public int ScreenWidth { get; set; }

        public int ScreenHeight { get; set; }

        public IEnumerable<CardPositionDocument> Rectangles { get; set; }
    }
}
