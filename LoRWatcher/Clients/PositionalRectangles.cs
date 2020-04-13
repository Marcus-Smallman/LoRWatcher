using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Collections.Generic;

namespace LoRWatcher.Clients
{
    public class PositionalRectangles
    {
        public string PlayerName { get; set; }

        public string OpponentName { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        public GameState? GameState { get; set; }

        public Screen Screen { get; set; }

        public IEnumerable<CardPosition> Rectangles { get; set; }
    }
}
