using LoRWatcher.Clients.Functions;
using System.Threading;
using System.Threading.Tasks;

namespace LoRWatcher.Stores
{
    public interface IPlayerDataStore
    {
        Task<Account> GetAccountAsync(string gameName, string tagLine, CancellationToken cancellationToken = default);

        Task<Account> AddAccountAsync(string playerId, string gameName, string tagLine, CancellationToken cancellationToken = default);
    }
}
