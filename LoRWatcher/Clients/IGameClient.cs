using System.Threading;
using System.Threading.Tasks;

namespace LoRWatcher.Clients
{
    public interface IGameClient
    {
        Task<StaticDecklist> GetActiveDecklistAsync(CancellationToken cancellationToken);

        Task<PositionalRectangles> GetCardPositionsAsync(CancellationToken cancellationToken);

        Task<GameResult> GetGameResultAsync(CancellationToken cancellationToken);
    }
}
