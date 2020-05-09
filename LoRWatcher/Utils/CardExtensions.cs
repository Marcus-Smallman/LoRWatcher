using LoRDeckCodes;
using LoRWatcher.Logger;
using System;
using System.Collections.Generic;

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
    }
}
