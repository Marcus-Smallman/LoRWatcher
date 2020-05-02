using LoRDeckCodes;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace LoRWatcher.Utils
{
    public class CardComparer
        : IComparer<CardCodeAndCount>
    {
        public int Compare([AllowNull] CardCodeAndCount x, [AllowNull] CardCodeAndCount y)
        {
            if (x == null)
            {
                if (y == null)
                {
                    return 0;
                }
                else
                {
                    return 1;
                }
            }
            else
            {
                if (y == null)
                {
                    return -1;
                }
                else
                {
                    if (x.Count > y.Count)
                    {
                        return -1;
                    }
                    else if (y.Count > x.Count)
                    {
                        return 1;
                    }
                    else
                    {
                        var xSet = int.Parse(x.CardCode.Substring(0, 2));
                        var ySet = int.Parse(y.CardCode.Substring(0, 2));
                        if (xSet > ySet)
                        {
                            return -1;
                        }
                        else if (ySet > xSet)
                        {
                            return 1;
                        }
                        else
                        {
                            var xFaction = Enum.Parse<Factions>(x.CardCode.Substring(2, 2));
                            var yFaction = Enum.Parse<Factions>(y.CardCode.Substring(2, 2));
                            if (xFaction > yFaction)
                            {
                                return -1;
                            }
                            else if (yFaction > xFaction)
                            {
                                return 1;
                            }
                            else
                            {
                                var xCardNumber = int.Parse(x.CardCode.Substring(4, 3));
                                var yCardNumber = int.Parse(y.CardCode.Substring(4, 3));
                                if (xCardNumber > yCardNumber)
                                {
                                    return -1;
                                }
                                else if (yCardNumber < xCardNumber)
                                {
                                    return 1;
                                }
                                else
                                {
                                    return 0;
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}
