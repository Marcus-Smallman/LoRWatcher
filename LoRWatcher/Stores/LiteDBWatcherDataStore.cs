﻿using LiteDB;
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
                        var collection = connection.GetCollection<MatchReportDocument>(MatchReportsCollectionName);

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

                        var result = new MatchReportMetadata();
                        if (metadata != null)
                        {
                            this.logger.Debug("Match report metadata retrieved");

                            result.PlayerName = metadata.PlayerName;
                            result.TotalGames = metadata.TotalWins + metadata.TotalLosses;
                            result.TotalWins = metadata.TotalWins;
                            result.TotalLosses = metadata.TotalLosses;
                        }

                        return result;
                    }

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
    }
}
