using LoRDeckCodes;
using LoRWatcher.Logger;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace LoRWatcher.Utils
{
    public static class CardExtensions
    {
        public static void Print(this List<CardCodeAndCount> cards, ILogger logger = null)
        {
            logger ??= new FileLogger();
            foreach (var card in cards)
            {
                logger.Debug($"\tCardCode: {card.CardCode}|Count: {card.Count}");
            }
        }

        public static IEnumerable<string> GetRegions(this List<CardCodeAndCount> cards, ILogger logger = null)
        {
            logger ??= new FileLogger();

            var regions = cards.Select(c => c.CardCode.Substring(2, 2)).Distinct();

            logger.Debug($"Deck regions: {string.Join(", ", regions)}");

            return regions;
        }

        public static string GetTimeSince(this DateTime dateTime)
        {
            TimeSpan timeSinceTime = DateTime.UtcNow.Subtract(dateTime);
            string timeSince;
            if (timeSinceTime.TotalMinutes < 1)
            {
                timeSince = "< 1 minute ago";
            }
            else if (timeSinceTime.TotalHours < 1)
            {
                timeSince = string.Format("{0} minute(s) ago", (int)timeSinceTime.TotalMinutes);
            }
            else if (timeSinceTime.TotalDays < 1)
            {
                timeSince = string.Format("{0} hour(s) ago", (int)timeSinceTime.TotalHours);
            }
            else
            {
                timeSince = string.Format("{0} day(s) ago", (int)timeSinceTime.TotalDays);
            }

            return timeSince;
        }

        public static IEnumerable<CardData> GetCardsFromCode(this string deckCode, ILogger logger = null)
        {
            logger ??= new FileLogger();

            var cards = new List<CardData>();

            // Gather all card data to map to current deck.
            try
            {
                var allCardData = new List<CardData>();

                var cardDataFilePaths = Directory.GetFiles(@$"{Directory.GetCurrentDirectory()}\wwwroot\assets", "set*-en_us.json", SearchOption.AllDirectories);
                foreach (var cardDataFilePath in cardDataFilePaths)
                {
                    var cardData = JsonConvert.DeserializeObject<IEnumerable<CardData>>(File.ReadAllText(cardDataFilePath));

                    allCardData.AddRange(cardData);
                }

                var cardCodeAndCounts = LoRDeckEncoder.GetDeckFromCode(deckCode);
                foreach (var cardCodeAndCount in cardCodeAndCounts)
                {
                    var card = allCardData.FirstOrDefault(c => c.CardCode == cardCodeAndCount.CardCode);
                    if (card != null)
                    {
                        cards.Add(new CardData
                        {
                            CardCode = cardCodeAndCount.CardCode,
                            Count = cardCodeAndCount.Count,
                            Name = card.Name,
                            Description = card.Description,
                            LevelupDescription = card.LevelupDescription,
                            Region = card.Region,
                            RegionRef = card.RegionRef,
                            Regions = card.Regions,
                            RegionRefs = card.RegionRefs,
                            Cost = card.Cost,
                            Attack = card.Attack,
                            Health = card.Health,
                            Type = card.Type,
                            FlavorText = card.FlavorText,
                            Keywords = card.Keywords,
                            SpellSpeed = card.SpellSpeed,
                            Subtype = card.Subtype,
                            Subtypes = card.Subtypes,
                            Supertype = card.Supertype,
                            Rarity = card.Rarity,
                            Collectible = card.Collectible,
                            Set = card.Set
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Error($"Error occurred getting card data from assets: {ex.Message}");
            }

            return cards.OrderBy(c => c.Cost);
        }
    }
}
