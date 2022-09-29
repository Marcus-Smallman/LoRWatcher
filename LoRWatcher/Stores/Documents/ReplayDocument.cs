using System.Collections.Generic;

namespace LoRWatcher.Stores.Documents
{
    public class ReplayDocument
    {
        public string Id { get; set; }

        public SortedList<string, SnapshotDocument> Snapshots { get; set; }
    }
}
