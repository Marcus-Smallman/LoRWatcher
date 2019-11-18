using LoRWatcher.Clients;
using System.Threading.Tasks;

namespace LoRWatcher.Caches
{
    public interface ICache
    {
        bool IsEmpty { get; }

        Task<MatchReport> GetMatchReportAsync();

        Task UpdateActiveMatchAsync(PositionalRectangles positionalRectangles);
    }
}
