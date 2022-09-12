using LiteDB;
using LoRWatcher.Caches;
using LoRWatcher.Logger;
using LoRWatcher.Stores.Documents;
using LoRWatcher.Utils;
using Mapster;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace LoRWatcher.Stores
{
    public class LiteDBWatcherDataStore
        : IWatcherDataStore
    {
        private const string CollectionName = "matchreports";

        private readonly IConnection<LiteDatabase> connection;

        private readonly ILogger logger;

        public LiteDBWatcherDataStore(IConnection<LiteDatabase> connection, ILogger logger)
        {
            this.connection = connection;
            this.logger = logger;
        }

        public async Task<bool> ReportGameAsync(MatchReport matchReport, CancellationToken cancellationToken)
        {
            await Task.Yield();

            return Retry.Invoke(() =>
            {
                try
                {
                    using (var connection = this.connection.GetConnection())
                    {
                        var collection = connection.GetCollection<MatchReportDocument>(CollectionName);

                        var doc = new MatchReportDocument
                        {
                            Id = matchReport.Id,
                            PlayerDeckCode = matchReport.PlayerDeckCode,
                            PlayerName = matchReport.PlayerName,
                            OpponentName = matchReport.OpponentName,
                            Regions = matchReport.Regions,
                            Snapshots = matchReport.Snapshots.Adapt<SortedList<string, SnapshotDocument>>(),
                            Result = matchReport.Result,
                            FinishTime = matchReport.FinishTime.UtcDateTime
                        };

                        collection.Insert(doc);

                        this.logger.Info("Match report stored");

                        return true;
                    }

                }
                catch (Exception ex)
                {
                    this.logger.Error($"Error occurred storing match report: {ex.Message}");

                    return false;
                }
            });
        }

        public async Task<MatchReport> GetMatchReportByIdAsync(string id, CancellationToken cancellationToken)
        {
            await Task.Yield();

            return Retry.Invoke<MatchReport>(() =>
            {
                try
                {
                    using (var connection = this.connection.GetConnection())
                    {
                        var collection = connection.GetCollection<MatchReportDocument>(CollectionName);

                        var matchReportDoc = collection.FindById(id);

                        this.logger.Debug($"Match report with id '{id}' retrieved");

                        var matchReport = new MatchReport
                        {
                            Id = matchReportDoc.Id,
                            PlayerDeckCode = matchReportDoc.PlayerDeckCode,
                            PlayerName = matchReportDoc.PlayerName,
                            OpponentName = matchReportDoc.OpponentName,
                            Regions = matchReportDoc.Regions,
                            Snapshots = matchReportDoc.Snapshots.Adapt<SortedList<string, Snapshot>>(),
                            Result = matchReportDoc.Result,
                            FinishTime = matchReportDoc.FinishTime
                        };

                        return matchReport;
                    }

                }
                catch (Exception ex)
                {
                    this.logger.Error($"Error occurred retrieving match report with id '{id}': {ex.Message}");

                    return null;
                }
            });
        }

        public async Task<IEnumerable<MatchReport>> GetMatchReportsAsync(int skip, int limit, CancellationToken cancellationToken)
        {
            await Task.Yield();

            return Retry.Invoke<IEnumerable<MatchReport>>(() =>
            {
                try
                {
                    using (var connection = this.connection.GetConnection())
                    {
                        var collection = connection.GetCollection<MatchReportDocument>(CollectionName);

                        var query = Query.All(nameof(MatchReportDocument.FinishTime), Query.Descending);
                        var matchReportDocs = collection.Find(query, skip, limit);

                        this.logger.Debug("Match reports retrieved");

                        var matchReports = new List<MatchReport>();
                        foreach (var matchReportDoc in matchReportDocs)
                        {
                            matchReports.Add(new MatchReport
                            {
                                Id = matchReportDoc.Id,
                                PlayerDeckCode = matchReportDoc.PlayerDeckCode,
                                PlayerName = matchReportDoc.PlayerName,
                                OpponentName = matchReportDoc.OpponentName,
                                Regions = matchReportDoc.Regions,
                                Snapshots = matchReportDoc.Snapshots.Adapt<SortedList<string, Snapshot>>(),
                                Result = matchReportDoc.Result,
                                FinishTime = matchReportDoc.FinishTime
                            });
                        }

                        return matchReports.OrderByDescending(mr => mr.FinishTime).ToList();
                    }

                }
                catch (Exception ex)
                {
                    this.logger.Error($"Error occurred retrieving match reports: {ex.Message}");

                    return null;
                }
            });
        }

        public async Task<MatchReportMetadata> GetMatchReportMetadataAsync(CancellationToken cancellationToken)
        {
            await Task.Yield();

            return Retry.Invoke<MatchReportMetadata>(() =>
            {
                try
                {
                    using (var connection = this.connection.GetConnection())
                    {
                        var collection = connection.GetCollection<MatchReportDocument>(CollectionName);

                        var totalGames = collection.Count();
                        var totalWins = collection.Count((doc) => doc.Result == true);
                        var totalLosses = totalGames - totalWins;

                        // Retrieving all documents and the performing LINQ queries would be very
                        // memory intensive, esecially if thousands of match reports have been stored.
                        // Performing the following SQL queries directly into the DB reduces and
                        // improves speed and in-memory usage.
                        var regionsMetadataResult = connection.Execute($"SELECT FIRST(*.{nameof(MatchReportDocument.Regions)}) AS Regions," +                                                                                       // Get the first element from the regions array. As distinct does not work with arrays within arrays, this is fine.
                                                                       $"DISTINCT(FILTER(*.{nameof(MatchReportDocument.Type)}=> @ != null)) AS TotalTypes, " +                                                                      // Get the game types that have been played with the respective regions.
                                                                       $"COUNT(*.{nameof(MatchReportDocument.Type)}) AS TotalGames, " +                                                                                             // Get the total amount of game played.
                                                                       $"COUNT(FILTER(* => @.{nameof(MatchReportDocument.Type)} = '{nameof(GameType.Normal)}')) AS NormalGames, " +                                                 // Get a count of all normal games that have been played.
                                                                       $"COUNT(FILTER(* => @.{nameof(MatchReportDocument.Result)} = true AND @.{nameof(MatchReportDocument.Type)} = '{nameof(GameType.Normal)}')) AS NormalWins " + // Get a count of all the normal game wins.
                                                                       $"FROM {CollectionName} " +                                                                                                                                  // Query documents against the match report collection.
                                                                       $"GROUP BY $.{nameof(MatchReportDocument.Regions)}");                                                                                                        // Group by regions.
                        var mostPlayedRegions = string.Empty;
                        var mostPlayedRegionsCount = 0;
                        var highestRegionsWinRate = string.Empty;
                        var highestRegionsWinRatePercentage = 0;
                        if (regionsMetadataResult.HasValues)
                        {
                            var highestNormalGames = 0;
                            var highestWinRate = 0d;
                            var regionsMetadata = regionsMetadataResult.ToEnumerable();
                            foreach (var regionMetadata in regionsMetadata)
                            {
                                var regions = regionMetadata["Regions"].AsArray;
                                var normalGames = regionMetadata["NormalGames"].AsInt32;
                                if (regions?.Any() == true &&
                                    normalGames > 0)
                                {
                                    // Calculate most played region
                                    if (normalGames > highestNormalGames)
                                    {
                                        mostPlayedRegions = $"{string.Join(", ", regions.ToList()).Replace("\"", string.Empty)}";
                                        mostPlayedRegionsCount = normalGames;
                                        highestNormalGames = normalGames;
                                    }

                                    // Calculate highest region win rate
                                    var normalWins = regionMetadata["NormalWins"].AsInt32;
                                    var winRate = (double)(100 / normalGames) * normalWins;
                                    if (winRate > highestWinRate)
                                    {
                                        highestRegionsWinRate = $"{string.Join(", ", regions.ToList()).Replace("\"", string.Empty)}";
                                        highestRegionsWinRatePercentage = (int)Math.Round(winRate, 0);
                                    }
                                }
                            }
                        }

                        this.logger.Debug("Match report metadata retrieved");

                        var matchReportMetadata = new MatchReportMetadata
                        {
                            TotalGames = totalGames,
                            TotalWins = totalWins,
                            TotalLosses = totalLosses,
                            MostPlayedRegions = mostPlayedRegions,
                            MostPlayedRegionsCount = mostPlayedRegionsCount,
                            HighestWinRateRegions = highestRegionsWinRate,
                            HighestWinRateRegionsPercentage = highestRegionsWinRatePercentage
                        };

                        return matchReportMetadata;
                    }

                }
                catch (Exception ex)
                {
                    this.logger.Error($"Error occurred retrieving match report metadata: {ex.Message}");

                    return null;
                }
            });
        }
    }
}
