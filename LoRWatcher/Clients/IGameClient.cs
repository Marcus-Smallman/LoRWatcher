using System.Threading;
using System.Threading.Tasks;

namespace LoRWatcher.Clients
{
    public interface IGameClient
    {
        Task<bool> IsClientActiveAsync(CancellationToken cancellationToken);

        Task<StaticDecklist> GetActiveDecklistAsync(CancellationToken cancellationToken);

        Task<PositionalRectangles> GetCardPositionsAsync(CancellationToken cancellationToken);

        Task<ExpeditionsState> GetExpeditionsStateAsync(CancellationToken cancellationToken);

        Task<GameResult> GetGameResultAsync(CancellationToken cancellationToken);
    }
}
