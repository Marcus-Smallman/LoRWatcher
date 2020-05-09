using LiteDB;
using LoRWatcher.Caches;
using LoRWatcher.Logger;
using LoRWatcher.Stores.Documents;
using LoRWatcher.Utils;
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
                        var collection = connection.GetCollection<MatchReportDocument>("matchreports");

                        var doc = new MatchReportDocument
                        {
                            Id = matchReport.Id,
                            PlayerDeckCode = matchReport.PlayerDeckCode,
                            PlayerName = matchReport.PlayerName,
                            OpponentName = matchReport.OpponentName,
                            Regions = matchReport.Regions,
                            Result = matchReport.Result,
                            FinishTime = matchReport.FinishTime.UtcDateTime,
                            Type = matchReport.Type
                        };

                        collection.Insert(doc);

                        this.logger.Debug("Match report stored");

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

        public async Task<IEnumerable<MatchReport>> GetMatchReportsAsync(int skip, int limit, CancellationToken cancellationToken)
        {
            await Task.Yield();

            return Retry.Invoke<IEnumerable<MatchReport>>(() =>
            {
                try
                {
                    using (var connection = this.connection.GetConnection())
                    {
                        var collection = connection.GetCollection<MatchReportDocument>("matchreports");

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
                                Result = matchReportDoc.Result,
                                FinishTime = matchReportDoc.FinishTime,
                                Type = matchReportDoc.Type
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
                        var collection = connection.GetCollection<MatchReportDocument>("matchreports");

                        var totalGames = collection.Count();
                        var totalWins = collection.Count((doc) => doc.Result == true);
                        var totalLosses = totalGames - totalWins;

                        this.logger.Debug("Match report metadata retrieved");

                        var matchReportMetadata = new MatchReportMetadata
                        {
                            TotalGames = totalGames,
                            TotalWins = totalWins,
                            TotalLosses = totalLosses
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
