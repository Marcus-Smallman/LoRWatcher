using LoRDeckCodes;
using System.Collections.Generic;

namespace LoRWatcher.Utils
{
    public class CardData
        : CardCodeAndCount
    {
        public string Region { get; set; }

        public string RegionRef { get; set; }

        public int Attack { get; set; }

        public int Cost { get; set; }

        public int Health { get; set; }

        public string Description { get; set; }

        public string LevelupDescription { get; set; }

        public string FlavorText { get; set; }

        public string Name { get; set; }

        public IEnumerable<string> Keywords { get; set; }

        public string SpellSpeed { get; set; }

        public string Rarity { get; set; }

        public string Subtype { get; set; }

        public IEnumerable<string> Subtypes { get; set; }

        public string Supertype { get; set; }

        public string Type { get; set; }

        public bool Collectible { get; set; }
    }
}
