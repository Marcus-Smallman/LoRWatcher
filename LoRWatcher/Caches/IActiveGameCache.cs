using LoRWatcher.Clients;
using System.Threading;
using System.Threading.Tasks;

namespace LoRWatcher.Caches
{
    public interface IActiveGameCache
    {
        bool IsEmpty { get; }

        Task<MatchReport> GetMatchReportAsync(CancellationToken cancellationToken);

        Task UpdateActiveMatchAsync(PositionalRectangles positionalRectangles, CancellationToken cancellationToken);
    }
}
