using LoRWatcher.Clients;

namespace LoRWatcher.Caches
{
    public static class GameStateCacheExtensions
    {
        public static string GetHumanReadableGameState(GameState gameState)
        {
            return gameState switch
            {
                GameState.Offline => "Offline",
                GameState.Menus => "Menus",
                GameState.InProgress => "In Progress",
                _ => "Unknown",
            };
        }

        public static string GetHumanReadableGameState(this IGameStateCache gameStateCache)
        {
            var gameState = gameStateCache.GetGameState();
            return GetHumanReadableGameState(gameState);
        }
    }
}
