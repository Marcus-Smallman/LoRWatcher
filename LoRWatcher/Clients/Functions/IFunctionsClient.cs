namespace LoRWatcher.Clients.Functions
{
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;

    public interface IFunctionsClient
    {
        Task<Account> GetAccountAsync(CancellationToken cancellationToken = default);

        Task<IEnumerable<string>> GetMatchIdsAsync(CancellationToken cancellationToken = default);

        Task<Match> GetMatchAsync(CancellationToken cancellationToken = default);
    }
}
