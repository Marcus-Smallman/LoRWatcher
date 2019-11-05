using System.Threading.Tasks;

namespace LoRWatcher.Clients
{
    public interface IGameClient
    {
        Task<StaticDecklist> GetActiveDecklistAsync();

        Task<PositionalRectangles> GetCardPositionsAsync();

        Task<GameResult> GetGameResult();
    }
}
