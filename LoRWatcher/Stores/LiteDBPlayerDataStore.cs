using LiteDB;
using LoRWatcher.Caches;
using LoRWatcher.Clients.Functions;
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
    public class LiteDBPlayerDataStore
        : IPlayerDataStore
    {
        private const string AccountsCollectionName = "accounts";

        private const string MatchIdsCollectionName = "matchids";

        private const string MatchSyncCollectionName = "matchsync";

        private const string PlayerMatchesCollectionName = "playermatches";

        private readonly IConnection<LiteDatabase> connection;

        private readonly ILogger logger;

        public LiteDBPlayerDataStore(IConnection<LiteDatabase> connection, ILogger logger)
        {
            this.connection = connection;
            this.logger = logger;
        }

        public async Task<Account> AddAccountAsync(string playerId, string gameName, string tagLine, CancellationToken cancellationToken = default)
        {
            await Task.Yield();

            return Retry.Invoke(() =>
            {
                try
                {
                    using var connection = this.connection.GetConnection();
                    var collection = connection.GetCollection<AccountDocument>(AccountsCollectionName);

                    var doc = new AccountDocument
                    {
                        PlayerId = playerId,
                        GameName = gameName,
                        TagLine = tagLine
                    };

                    collection.Insert(doc);

                    this.logger.Info("Account added");

                    return doc.Adapt<Account>();

                }
                catch (Exception ex)
                {
                    this.logger.Error($"Error occurred adding account: {ex.Message}");

                    return null;
                }
            });
        }

        public async Task<bool> AddMatchAsync(Match playerMatch, CancellationToken cancellationToken = default)
        {
            await Task.Yield();

            return Retry.Invoke(() =>
            {
                try
                {
                    using var connection = this.connection.GetConnection();
                    var collection = connection.GetCollection<PlayerMatchDocument>(PlayerMatchesCollectionName);

                    var doc = playerMatch.Adapt<PlayerMatchDocument>();
                    doc.Id = playerMatch.Metadata.MatchId;

                    collection.Insert(doc);

                    this.logger.Info("Player match added");

                    return true;

                }
                catch (Exception ex)
                {
                    this.logger.Error($"Error occurred adding player match: {ex.Message}");

                    return false;
                }
            });
        }

        public async Task<Account> GetAccountAsync(string gameName, string tagLine, CancellationToken cancellationToken = default)
        {
            await Task.Yield();

            return Retry.Invoke<Account>(() =>
            {
                try
                {
                    using (var connection = this.connection.GetConnection())
                    {
                        var collection = connection.GetCollection<AccountDocument>(AccountsCollectionName);

                        var accountDoc = collection.FindOne(
                            Query.And(
                                Query.EQ(nameof(AccountDocument.GameName), gameName),
                                Query.EQ(nameof(AccountDocument.TagLine), tagLine)));

                        this.logger.Debug($"Account with name '{gameName}' and tag line '{tagLine}' retrieved");

                        return accountDoc?.Adapt<Account>();
                    }

                }
                catch (Exception ex)
                {
                    this.logger.Error($"Error occurred retrieving account with name '{gameName}' and tag line '{tagLine}': {ex.Message}");

                    return null;
                }
            }, 100);
        }

        public async Task<IEnumerable<string>> GetMatchIdsAsync(bool includeSynced = true, CancellationToken cancellationToken = default)
        {
            await Task.Yield();

            return Retry.Invoke<IEnumerable<string>>(() =>
            {
                try
                {
                    using (var connection = this.connection.GetConnection())
                    {
                        var collection = connection.GetCollection<MatchIdDocument>(MatchIdsCollectionName);

                        var matchIds = Enumerable.Empty<MatchIdDocument>();
                        if (includeSynced == false)
                        {
                            matchIds = collection.Find(doc => doc.Synced == false).ToArray();
                        }
                        else
                        {
                            matchIds = collection.Find(Query.All()).ToArray();
                        }

                        this.logger.Debug($"Match ids retrieved. Include synced: {includeSynced}");

                        return matchIds.Select(mId => mId.Id);
                    }

                }
                catch (Exception ex)
                {
                    this.logger.Error($"Error occurred retrieving match ids. Include synced: {includeSynced} | {ex.Message}");

                    return null;
                }
            });
        }

        public async Task<PlayerMatch> GetPlayerMatchAsync(string playerId, MatchReport matchReport, CancellationToken cancellationToken = default)
        {
            await Task.Yield();

            return Retry.Invoke<PlayerMatch>(() =>
            {
                try
                {
                    using (var connection = this.connection.GetConnection())
                    {
                        var startTime = DateTimeOffset.Parse(matchReport.Snapshots.First().Key);
                        var lowerTime = startTime.Subtract(TimeSpan.FromSeconds(10));
                        var upperTime = startTime.Add(TimeSpan.FromSeconds(10));

                        var result = connection.Execute($@"SELECT $
                                                           FROM {PlayerMatchesCollectionName}
                                                           WHERE DATETIME_UTC($.Info.GameStartTimeUTC) >= DATETIME_UTC('{lowerTime}')
                                                             AND DATETIME_UTC($.Info.GameStartTimeUTC) <= DATETIME_UTC('{upperTime}')
                                                           LIMIT 1");

                        //TODO: Improve this..
                        if (result.HasValues == true)
                        {
                            var docs = result.ToArray();
                            if (docs.Length == 1)
                            {
                                var doc = docs[0];
                                var playerMatchDocument = BsonMapper.Global.Deserialize<PlayerMatchDocument>(doc);
                                var player = playerMatchDocument.Info.Players.FirstOrDefault(player => player.PlayerId == playerId);
                                if (player != null &&
                                    player.GameOutcome == (matchReport.Result ? "win" : "loss"))
                                {
                                    return playerMatchDocument.Adapt<PlayerMatch>();
                                }
                            }
                        }

                        return null;
                    }

                }
                catch (Exception ex)
                {
                    this.logger.Error($"Error occurred retrieving player match with player id '{playerId}': {ex.Message}");

                    return null;
                }
            }, 100);
        }

        public async Task<bool> IsMatchSyncedAsync(string watcherMatchId, CancellationToken cancellationToken = default)
        {
            await Task.Yield();

            try
            {
                using (var connection = this.connection.GetConnection())
                {
                    var collection = connection.GetCollection<MatchSyncDocument>(MatchSyncCollectionName);

                    var syncDocument = collection.FindOne(doc => doc.WatcherMatchId == watcherMatchId);
                    if (syncDocument != null)
                    {
                        this.logger.Debug($"Sync match found. Watcher Match Id: {syncDocument.WatcherMatchId} | Player Match Id: {syncDocument.PlayerMatchId}");

                        return true;
                    }
                    else
                    {
                        this.logger.Debug($"Sync match not found. Watcher Match Id: {watcherMatchId}");

                        return false;
                    }
                }

            }
            catch (Exception ex)
            {
                this.logger.Error($"Error occurred retrieving sync match. {ex.Message}");

                return false;
            }
        }

        public async Task<bool> MatchNotFoundAsync(string watcherMatchId, CancellationToken cancellationToken = default)
        {
            await Task.Yield();

            return Retry.Invoke(() =>
            {
                try
                {
                    using var connection = this.connection.GetConnection();
                    var collection = connection.GetCollection<MatchSyncDocument>(MatchSyncCollectionName);

                    var doc = new MatchSyncDocument
                    {
                        WatcherMatchId = watcherMatchId,
                        PlayerMatchId = null
                    };

                    collection.Insert(doc);

                    this.logger.Info("Sync match added for not found");

                    return true;

                }
                catch (Exception ex)
                {
                    this.logger.Error($"Error occurred adding sync match for not found: {ex.Message}");

                    return false;
                }
            });
        }

        public async Task<bool> SyncMatchAsync(string playerMatchId, string watcherMatchId, CancellationToken cancellationToken = default)
        {
            await Task.Yield();

            return Retry.Invoke(() =>
            {
                try
                {
                    using var connection = this.connection.GetConnection();
                    var collection = connection.GetCollection<MatchSyncDocument>(MatchSyncCollectionName);

                    var doc = new MatchSyncDocument
                    {
                        PlayerMatchId = playerMatchId,
                        WatcherMatchId = watcherMatchId
                    };

                    collection.Insert(doc);

                    this.logger.Info("Sync match added");

                    return true;

                }
                catch (Exception ex)
                {
                    this.logger.Error($"Error occurred adding sync match: {ex.Message}");

                    return false;
                }
            });
        }

        public async Task<bool> SyncMatchIdAsync(string matchId, CancellationToken cancellationToken = default)
        {
            await Task.Yield();

            return Retry.Invoke(() =>
            {
                try
                {
                    using var connection = this.connection.GetConnection();
                    var collection = connection.GetCollection<MatchIdDocument>(MatchIdsCollectionName);

                    var doc = new MatchIdDocument
                    {
                        Id = matchId,
                        Synced = true
                    };

                    collection.Update(matchId, doc);

                    this.logger.Info("Match id synced");

                    return true;

                }
                catch (Exception ex)
                {
                    this.logger.Error($"Error occurred updating match id: {ex.Message}");

                    return false;
                }
            });
        }

        public async Task<bool> UpdateMatchIdsAsync(IEnumerable<string> matchIds, CancellationToken cancellationToken = default)
        {
            await Task.Yield();

            return Retry.Invoke(() =>
            {
                try
                {
                    using var connection = this.connection.GetConnection();
                    var collection = connection.GetCollection<MatchIdDocument>(MatchIdsCollectionName);

                    foreach (var matchId in matchIds)
                    {
                        var result = collection.FindById(matchId);
                        if (result == null)
                        {
                            var doc = new MatchIdDocument
                            {
                                Id = matchId,
                                Synced = false
                            };

                            collection.Insert(doc);

                            this.logger.Info("Match id added");
                        }
                    }

                    return true;

                }
                catch (Exception ex)
                {
                    this.logger.Error($"Error occurred updating match ids: {ex.Message}");

                    return false;
                }
            });
        }
    }
}
