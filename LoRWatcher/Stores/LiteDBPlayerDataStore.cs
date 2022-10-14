using LiteDB;
using LoRWatcher.Clients.Functions;
using LoRWatcher.Logger;
using LoRWatcher.Stores.Documents;
using LoRWatcher.Utils;
using Mapster;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace LoRWatcher.Stores
{
    public class LiteDBPlayerDataStore
        : IPlayerDataStore
    {
        private const string AccountsCollectionName = "accounts";

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
            }, 100);
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
                                Query.Contains(nameof(AccountDocument.GameName), gameName),
                                Query.Contains(nameof(AccountDocument.TagLine), tagLine)));

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
    }
}
