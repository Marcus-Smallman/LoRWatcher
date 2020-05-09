using LoRDeckCodes;
using LoRWatcher.Logger;
using System.Collections.Generic;
using System.Linq;

namespace LoRWatcher.Utils
{
    public static class CardExtensions
    {
        public static void Print(this List<CardCodeAndCount> cards, ILogger logger = null)
        {
            logger ??= new ConsoleLogger();
            foreach (var card in cards)
            {
                logger.Debug($"\tCardCode: {card.CardCode}|Count: {card.Count}");
            }
        }

        public static IEnumerable<string> GetRegions(this List<CardCodeAndCount> cards, ILogger logger = null)
        {
            logger ??= new ConsoleLogger();

            var regions = cards.Select(c => c.CardCode.Substring(2, 2)).Distinct();

            logger.Debug($"Deck regions: {string.Join(", ", regions)}");

            return regions;
        }
    }
}
