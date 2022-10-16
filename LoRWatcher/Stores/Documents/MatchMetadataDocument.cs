using System.Collections.Generic;

namespace LoRWatcher.Stores.Documents
{
    public class MatchMetadataDocument
    {
        public string DataVerison { get; set; }

        public string MatchId { get; set; }

        public IEnumerable<string> Participants { get; set; }
    }
}
