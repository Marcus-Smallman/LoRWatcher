using LoRWatcher.Clients;

namespace LoRWatcher.Caches
{
    public interface IGameStateCache
    {
        void SetGameState(GameState? gameState);

        GameState GetGameState();
    }
}
