using LoRWatcher.Clients;
using System;
using System.Threading;

namespace LoRWatcher.Caches
{
    public class GameStateCache
        : IGameStateCache
    {
        private int gameState;

        public GameStateCache()
        {
            this.gameState = 0;
        }

        public GameState GetGameState()
        {
            return (GameState)this.gameState;
        }

        public void SetGameState(GameState? gameState)
        {
            var currentGameState = this.gameState;

            gameState ??= GameState.Offline;
            int updatedGameState = Convert.ToInt32(gameState.Value);

            Interlocked.CompareExchange(ref this.gameState, updatedGameState, currentGameState);
        }
    }
}
