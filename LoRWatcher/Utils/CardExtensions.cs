using LoRDeckCodes;
using LoRWatcher.Caches;
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

            var regions = cards
                .Select(c1 =>
                {
                    var regionRefs = GetCardData().FirstOrDefault(c2 => c2.CardCode == c1.CardCode).RegionRefs.OrderBy(s => s);
                    if (regionRefs.Count() >= 2)
                    {
                        return $"[{string.Join(", ", regionRefs)}]";
                    }

                    return regionRefs.FirstOrDefault();
                })
                .Distinct();

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
                if (timeSinceTime.TotalMinutes < 2)
                {
                    timeSince = string.Format("{0} minute ago", (int)timeSinceTime.TotalMinutes);
                }
                else
                {
                    timeSince = string.Format("{0} minutes ago", (int)timeSinceTime.TotalMinutes);
                }
            }
            else if (timeSinceTime.TotalDays < 1)
            {
                if (timeSinceTime.TotalHours < 2)
                {
                    timeSince = string.Format("{0} hour ago", (int)timeSinceTime.TotalHours);
                }
                else
                {
                    timeSince = string.Format("{0} hours ago", (int)timeSinceTime.TotalHours);
                }
            }
            else
            {
                if (timeSinceTime.TotalDays < 2)
                {
                    timeSince = string.Format("{0} day ago", (int)timeSinceTime.TotalDays);
                }
                else
                {
                    timeSince = string.Format("{0} days ago", (int)timeSinceTime.TotalDays);
                }
            }

            return timeSince;
        }

        private static List<CardData> CardData;

        public static List<CardData> GetCardData(ILogger logger = null)
        {
            if (CardData != null)
            {
                return CardData;
            }

            var allCardData = new List<CardData>();

            try
            {
                var cardDataFilePaths = Directory.GetFiles(@$"{Directory.GetCurrentDirectory()}\wwwroot\assets", "set*-en_us.json", SearchOption.AllDirectories);
                foreach (var cardDataFilePath in cardDataFilePaths)
                {
                    var cardData = JsonConvert.DeserializeObject<IEnumerable<CardData>>(File.ReadAllText(cardDataFilePath));

                    allCardData.AddRange(cardData);
                }

                CardData = allCardData;
            }
            catch (Exception ex)
            {
                logger.Error($"Error occurred getting card data from assets: {ex.Message}");
            }

            return CardData;
        }

        public static IEnumerable<CardData> GetCardsFromCode(this string deckCode, ILogger logger = null)
        {
            logger ??= new FileLogger();

            var cards = new List<CardData>();

            // Gather all card data to map to current deck.
            try
            {
                var allCardData = GetCardData();

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
                logger.Error($"Error occurred getting cards from deckcode: {ex.Message}");
            }

            return cards.OrderBy(c => c.Cost);
        }

        public static string GetCardNameOpponentName(this string opponentName, ILogger logger = null)
        {
            logger ??= new FileLogger();

            var name = opponentName;

            if (string.IsNullOrEmpty(name) == false)
            {
                try
                {
                    if (opponentName.StartsWith("card_", StringComparison.OrdinalIgnoreCase) == true &&
                        opponentName.EndsWith("_name", StringComparison.OrdinalIgnoreCase) == true)
                    {
                        var opponentNameArr = opponentName.Split('_');
                        if (opponentNameArr.Length >= 3 &&
                            string.IsNullOrEmpty(opponentNameArr[1]) == false)
                        {
                            var cardCode = opponentNameArr[1];

                            var allCardData = GetCardData();

                            var opponentCardData = allCardData.FirstOrDefault(c => c.CardCode.Equals(cardCode, StringComparison.OrdinalIgnoreCase) == true);
                            if (opponentCardData != null)
                            {
                                name = opponentCardData.Name;
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    logger.Error($"Error occurred getting card name from opponent name: {ex.Message}");
                }
            }

            return name;
        }

        public static bool CardEquals(this Snapshot currentSnapshot, Snapshot otherSnapshot)
        {
            if (currentSnapshot.Rectangles.Count() !=
                otherSnapshot.Rectangles.Count())
            {
                return false;
            }

            foreach (var rectangle in currentSnapshot.Rectangles.ToArray())
            {
                if (otherSnapshot.Rectangles.Any(r =>
                {
                    return r.CardId == rectangle.CardId &&
                           r.CardCode == rectangle.CardCode &&
                           r.Width == rectangle.Width &&
                           r.Height == rectangle.Height &&
                           r.TopLeftX == rectangle.TopLeftX &&
                           r.TopLeftY == rectangle.TopLeftY &&
                           r.LocalPlayer == rectangle.LocalPlayer;
                }) == false)
                {
                    return false;
                }
            }

            return true;
        }
    }
}
