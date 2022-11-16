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
        private const string MatchReportsCollectionName = "matchreports";

        private const string MatchReportsMetadataCollectionName = "matchreportsmetadata";

        private const string MatchReplaysCollectionName = "matchreplays";

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
                        var collection = connection.GetCollection<MatchReportDocument>(MatchReportsCollectionName);

                        var doc = new MatchReportDocument
                        {
                            Id = matchReport.Id,
                            PlayerDeckCode = matchReport.PlayerDeckCode,
                            PlayerName = matchReport.PlayerName,
                            OpponentName = matchReport.OpponentName,
                            Regions = matchReport.Regions,
                            RegionsText = matchReport.RegionsText,
                            Result = matchReport.Result,
                            ResultText = matchReport.ResultText,
                            Type = matchReport.Type,
                            StartTime = matchReport.StartTime.UtcDateTime,
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

        public async Task<bool> ClearMatchReplayAsync(MatchReport matchReport, CancellationToken cancellationToken)
        {
            await Task.Yield();

            return Retry.Invoke(() =>
            {
                try
                {
                    using (var connection = this.connection.GetConnection())
                    {
                        var collection = connection.GetCollection<MatchReportDocument>(MatchReportsCollectionName);

                        var doc = new MatchReportDocument
                        {
                            Id = matchReport.Id,
                            PlayerDeckCode = matchReport.PlayerDeckCode,
                            PlayerName = matchReport.PlayerName,
                            OpponentName = matchReport.OpponentName,
                            Regions = matchReport.Regions,
                            RegionsText = matchReport.RegionsText,
                            Snapshots = null,
                            Result = matchReport.Result,
                            ResultText = matchReport.ResultText,
                            Type = matchReport.Type,
                            FinishTime = matchReport.FinishTime.UtcDateTime,
                            StartTime = matchReport.StartTime.UtcDateTime
                        };

                        collection.Update(doc);

                        this.logger.Info("Match report replay cleared");

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

        public async Task<bool> UpdateGameTypeAsync(string id, string gameType, CancellationToken cancellationToken = default)
        {
            await Task.Yield();

            return Retry.Invoke(() =>
            {
                try
                {
                    using var connection = this.connection.GetConnection();
                    var collection = connection.GetCollection<MatchReportDocument>(MatchReportsCollectionName);

                    var matchReportDoc = collection.FindById(id);
                    matchReportDoc.Type = gameType;

                    var result = collection.Update(matchReportDoc);
                    if (result == true)
                    {
                        this.logger.Info("Game type updated");
                    }
                    else
                    {
                        this.logger.Warning("Match not found");
                    }

                    return result;

                }
                catch (Exception ex)
                {
                    this.logger.Error($"Error occurred updating match ids: {ex.Message}");

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
                        var collection = connection.GetCollection<MatchReportDocument>(MatchReportsCollectionName);

                        var matchReportDoc = collection.FindById(id);

                        this.logger.Debug($"Match report with id '{id}' retrieved");

                        var matchReport = new MatchReport
                        {
                            Id = matchReportDoc.Id,
                            PlayerDeckCode = matchReportDoc.PlayerDeckCode,
                            PlayerName = matchReportDoc.PlayerName,
                            OpponentName = matchReportDoc.OpponentName,
                            Regions = matchReportDoc.Regions,
                            RegionsText = matchReportDoc.RegionsText,
                            Result = matchReportDoc.Result,
                            ResultText = matchReportDoc.ResultText,
                            Type = matchReportDoc.Type,
                            Snapshots = matchReportDoc.Snapshots.Adapt<SortedList<string, Snapshot>>(),
                            StartTime = matchReportDoc.StartTime,
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

        public async Task<MatchReports> GetMatchReportsAsync(
            int skip,
            int limit,
            string opponentNameFilter = null,
            int opponentNameSortDirection = 0,
            string resultFilter = null,
            int resultSortDirection = 0,
            string regionsFilter = null,
            int regionsSortDirection = 0,
            string gameTypeFilter = null,
            int gameTypeSortDirection = 0,
            CancellationToken cancellationToken = default)
        {
            await Task.Yield();

            return Retry.Invoke<MatchReports>(() =>
            {
                try
                {
                    using (var connection = this.connection.GetConnection())
                    {
                        var collection = connection.GetCollection<MatchReportDocument>(MatchReportsCollectionName);

                        var query = collection.Query();
                        if (string.IsNullOrWhiteSpace(opponentNameFilter) == false)
                        {
                            query = query
                                .Where(doc => doc.OpponentName.Contains(opponentNameFilter, StringComparison.OrdinalIgnoreCase));
                        }
                        if (string.IsNullOrWhiteSpace(resultFilter) == false)
                        {
                            query = query
                                .Where(doc => doc.ResultText.Contains(resultFilter, StringComparison.OrdinalIgnoreCase));
                        }
                        if (string.IsNullOrWhiteSpace(regionsFilter) == false)
                        {
                            query = query
                                .Where(doc => doc.RegionsText.Contains(regionsFilter, StringComparison.OrdinalIgnoreCase));
                        }
                        if (string.IsNullOrWhiteSpace(gameTypeFilter) == false)
                        {
                            query = query
                                .Where(doc => doc.Type.Contains(gameTypeFilter, StringComparison.OrdinalIgnoreCase));
                        }

                        if (opponentNameSortDirection > 0)
                        {
                            if (opponentNameSortDirection == 1)
                            {
                                query = query
                                    .OrderBy(doc => doc.OpponentName.ToLower());
                            }
                            else if (opponentNameSortDirection == 2)
                            {
                                query = query
                                    .OrderByDescending(doc => doc.OpponentName.ToLower());
                            }
                        }
                        else if (resultSortDirection > 0)
                        {
                            if (resultSortDirection == 1)
                            {
                                query = query
                                    .OrderBy(doc => doc.ResultText.ToLower());
                            }
                            else if (resultSortDirection == 2)
                            {
                                query = query
                                    .OrderByDescending(doc => doc.ResultText.ToLower());
                            }
                        }
                        else if (regionsSortDirection > 0)
                        {
                            if (regionsSortDirection == 1)
                            {
                                query = query
                                    .OrderBy(doc => doc.RegionsText.ToLower());
                            }
                            else if (regionsSortDirection == 2)
                            {
                                query = query
                                    .OrderByDescending(doc => doc.RegionsText.ToLower());
                            }
                        }
                        else if (gameTypeSortDirection > 0)
                        {
                            if (gameTypeSortDirection == 1)
                            {
                                query = query
                                    .OrderBy(doc => doc.Type.ToLower());
                            }
                            else if (gameTypeSortDirection == 2)
                            {
                                query = query
                                    .OrderByDescending(doc => doc.Type.ToLower());
                            }
                        }
                        else
                        {
                            query = query
                                .OrderByDescending(doc => doc.FinishTime);
                        }

                        var matchReportCount = query.Count();

                        var matchReportDocs = query
                            .Skip(skip)
                            .Limit(limit)
                            .ToList();

                        this.logger.Debug("Match reports retrieved");

                        var matchReports = new MatchReports
                        {
                            Count = matchReportCount,
                            Matches = matchReportDocs.Adapt<IEnumerable<MatchReport>>()
                        };

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

        public async Task<IEnumerable<MatchReport>> GetAllMatchReportsAsync(CancellationToken cancellationToken)
        {
            await Task.Yield();

            return Retry.Invoke<IEnumerable<MatchReport>>(() =>
            {
                try
                {
                    using (var connection = this.connection.GetConnection())
                    {
                        var collection = connection.GetCollection<MatchReportDocument>(MatchReportsCollectionName);

                        var query = Query.All(nameof(MatchReportDocument.FinishTime), Query.Descending);
                        var matchReportDocs = collection.FindAll().ToArray();

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
                                RegionsText = matchReportDoc.RegionsText,
                                Result = matchReportDoc.Result,
                                ResultText = matchReportDoc.ResultText,
                                Type = matchReportDoc.Type,
                                Snapshots = matchReportDoc.Snapshots.Adapt<SortedList<string, Snapshot>>(),
                                StartTime = matchReportDoc.StartTime,
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

        public async Task<MatchReportMetadata> SetMatchReportsMetadataAsync(MatchReportMetadata matchReportMetadata, CancellationToken cancellationToken)
        {
            await Task.Yield();

            return Retry.Invoke<MatchReportMetadata>(() =>
            {
                try
                {
                    using (var connection = this.connection.GetConnection())
                    {
                        var collection = connection.GetCollection<MatchReportMetadataDocument>(MatchReportsMetadataCollectionName);

                        var metadata = collection.FindOne(Query.All());
                        if (metadata != null)
                        {
                            metadata.TotalWins = matchReportMetadata.TotalWins;
                            metadata.TotalLosses = matchReportMetadata.TotalLosses;

                            collection.Update(metadata);

                            this.logger.Debug("Set match report metadata updated");
                        }
                        else
                        {
                            metadata = new MatchReportMetadataDocument
                            {
                                PlayerName = matchReportMetadata.PlayerName,
                                TotalWins = matchReportMetadata.TotalWins,
                                TotalLosses = matchReportMetadata.TotalLosses
                            };

                            collection.Insert(metadata);

                            this.logger.Debug("Set match report metadata added");
                        }

                        return new MatchReportMetadata
                        {
                            PlayerName = metadata.PlayerName,
                            TagLine = metadata.TagLine,
                            TotalWins = metadata.TotalWins,
                            TotalLosses = metadata.TotalLosses,
                            TotalGames = metadata.TotalWins + metadata.TotalLosses
                        };
                    }

                }
                catch (Exception ex)
                {
                    this.logger.Error($"Error occurred setting match report metadata: {ex.Message}");

                    return null;
                }
            });
        }

        public async Task<MatchReportMetadata> SetTagLineAsync(string tagLine, CancellationToken cancellationToken)
        {
            await Task.Yield();

            return Retry.Invoke<MatchReportMetadata>(() =>
            {
                try
                {
                    using (var connection = this.connection.GetConnection())
                    {
                        var collection = connection.GetCollection<MatchReportMetadataDocument>(MatchReportsMetadataCollectionName);

                        var metadata = collection.FindOne(Query.All());
                        if (metadata != null)
                        {
                            metadata.TagLine = tagLine;

                            collection.Update(metadata);

                            this.logger.Debug("Tag line in metadata updated");

                            return new MatchReportMetadata
                            {
                                PlayerName = metadata.PlayerName,
                                TagLine = metadata.TagLine,
                                TotalWins = metadata.TotalWins,
                                TotalLosses = metadata.TotalLosses,
                                TotalGames = metadata.TotalWins + metadata.TotalLosses
                            };
                        }

                        return null;
                    }

                }
                catch (Exception ex)
                {
                    this.logger.Error($"Error occurred setting match report metadata: {ex.Message}");

                    return null;
                }
            });
        }

        public async Task<MatchReportMetadata> GetMatchReportsMetadataAsync(CancellationToken cancellationToken)
        {
            await Task.Yield();

            return Retry.Invoke<MatchReportMetadata>(() =>
            {
                try
                {
                    using (var connection = this.connection.GetConnection())
                    {
                        var collection = connection.GetCollection<MatchReportDocument>(MatchReportsCollectionName);

                        var allDocs = collection.FindAll().ToArray();

                        this.logger.Debug("Match report metadata retrieved");

                        var totalGames = allDocs.Count();
                        var totalWins = allDocs.Where(doc => doc.Result == true).Count();

                        var matchReportMetadata = new MatchReportMetadata
                        {
                            PlayerName = allDocs.First().PlayerName,
                            TotalGames = totalGames,
                            TotalWins = totalWins,
                            TotalLosses = totalGames - totalWins
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

        public async Task<MatchReportMetadata> GetMatchReportsMetadataV2Async(CancellationToken cancellationToken)
        {
            await Task.Yield();

            return Retry.Invoke<MatchReportMetadata>(() =>
            {
                try
                {
                    using (var connection = this.connection.GetConnection())
                    {
                        var collection = connection.GetCollection<MatchReportMetadataDocument>(MatchReportsMetadataCollectionName);

                        var metadata = collection.FindOne(Query.All());
                        if (metadata != null)
                        {
                            this.logger.Debug("Match report metadata retrieved");

                            var result = new MatchReportMetadata();
                            result.PlayerName = metadata.PlayerName;
                            result.TagLine = metadata.TagLine;
                            result.TotalGames = metadata.TotalWins + metadata.TotalLosses;
                            result.TotalWins = metadata.TotalWins;
                            result.TotalLosses = metadata.TotalLosses;

                            return result;
                        }
                    }

                    return null;

                }
                catch (Exception ex)
                {
                    this.logger.Error($"Error occurred retrieving match report metadata: {ex.Message}");

                    return null;
                }
            });
        }

        public async Task<MatchReportMetadata> UpdateMatchReportsMetadataAsync(MatchReport matchReport, CancellationToken cancellationToken)
        {
            await Task.Yield();

            return Retry.Invoke<MatchReportMetadata>(() =>
            {
                try
                {
                    using (var connection = this.connection.GetConnection())
                    {
                        var collection = connection.GetCollection<MatchReportMetadataDocument>(MatchReportsMetadataCollectionName);

                        var metadata = collection.FindOne(Query.All());
                        if (metadata != null)
                        {
                            metadata.TotalWins = metadata.TotalWins += matchReport.Result ? 1 : 0;
                            metadata.TotalLosses = metadata.TotalLosses += matchReport.Result ? 0 : 1;

                            collection.Update(metadata);

                            this.logger.Debug("Match report metadata updated");
                        }
                        else
                        {
                            metadata = new MatchReportMetadataDocument
                            {
                                PlayerName = matchReport.PlayerName,
                                TotalWins = matchReport.Result ? 1 : 0,
                                TotalLosses = matchReport.Result ? 0 : 1
                            };

                            collection.Insert(metadata);

                            this.logger.Debug("Match report metadata added");
                        }

                        return new MatchReportMetadata
                        {
                            PlayerName = metadata.PlayerName,
                            TotalWins = metadata.TotalWins,
                            TotalLosses = metadata.TotalLosses,
                            TotalGames = metadata.TotalWins + metadata.TotalLosses
                        };
                    }

                }
                catch (Exception ex)
                {
                    this.logger.Error($"Error occurred retrieving match report metadata: {ex.Message}");

                    return null;
                }
            });
        }

        public async Task<bool> ReportReplayAsync(MatchReport matchReport, CancellationToken cancellationToken)
        {
            await Task.Yield();

            return Retry.Invoke(() =>
            {
                try
                {
                    using (var connection = this.connection.GetConnection())
                    {
                        var collection = connection.GetCollection<ReplayDocument>(MatchReplaysCollectionName);

                        var doc = new ReplayDocument
                        {
                            Id = matchReport.Id,
                            Snapshots = matchReport.Snapshots.Adapt<SortedList<string, SnapshotDocument>>()
                        };

                        collection.Insert(doc);

                        this.logger.Info("Match replay stored");

                        return true;
                    }

                }
                catch (Exception ex)
                {
                    this.logger.Error($"Error occurred storing match replay: {ex.Message}");

                    return false;
                }
            });
        }

        public async Task<SortedList<string, Snapshot>> GetReplayByIdAsync(string id, CancellationToken cancellationToken)
        {
            await Task.Yield();

            return Retry.Invoke<SortedList<string, Snapshot>>(() =>
            {
                try
                {
                    using (var connection = this.connection.GetConnection())
                    {
                        var collection = connection.GetCollection<ReplayDocument>(MatchReplaysCollectionName);

                        var replayDoc = collection.FindById(id);
                        if (replayDoc == null)
                        {
                            this.logger.Debug($"No replay found for match with id '{id}'");

                            return new SortedList<string, Snapshot>();
                        }

                        this.logger.Debug($"Replay with id '{id}' retrieved");

                        return replayDoc.Snapshots?.Adapt<SortedList<string, Snapshot>>();
                    }

                }
                catch (Exception ex)
                {
                    this.logger.Error($"Error occurred retrieving replay with id '{id}': {ex.Message}");

                    return null;
                }
            });
        }
    }
}
