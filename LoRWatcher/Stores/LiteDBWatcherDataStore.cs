using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using LiteDB;
using LoRWatcher.Caches;
using LoRWatcher.Logger;
using LoRWatcher.Stores.Documents;
using LoRWatcher.Utils;

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
                            Result = matchReport.Result
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

        public async Task<IEnumerable<MatchReport>> GetMatchReportsAsync(CancellationToken cancellationToken)
        {
            await Task.Yield();

            return Retry.Invoke<IEnumerable<MatchReport>>(() =>
            {
                try
                {
                    using (var connection = this.connection.GetConnection())
                    {
                        var collection = connection.GetCollection<MatchReportDocument>("matchreports");

                        var matchReportDocs = collection.FindAll();

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
                                Result = matchReportDoc.Result
                            });
                        }

                        return matchReports;
                    }

                }
                catch (Exception ex)
                {
                    this.logger.Error($"Error occurred retrieving match reports: {ex.Message}");

                    return null;
                }
            });
        }
    }
}
