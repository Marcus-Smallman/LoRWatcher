namespace LoRWatcher.Clients.Functions
{
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;

    public interface IFunctionsClient
    {
        Task<Account> GetAccountAsync(string gameName, string tagLine, string region, CancellationToken cancellationToken = default);

        Task<IEnumerable<string>> GetMatchIdsAsync(string playerId, string region, CancellationToken cancellationToken = default);

        Task<Match> GetMatchAsync(string matchId, string region, CancellationToken cancellationToken = default);
    }
}
