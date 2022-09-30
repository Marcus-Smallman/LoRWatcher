using LoRWatcher.Clients;
using LoRWatcher.Events;
using System;
using System.Threading;

namespace LoRWatcher.Caches
{
    public class GameStateCache
        : IGameStateCache
    {
        private readonly IWatcherEventHandler watcherEventHandler;

        private int gameState;

        public GameStateCache(
            IWatcherEventHandler watcherEventHandler)
        {
            this.watcherEventHandler = watcherEventHandler;

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
            if (updatedGameState != currentGameState)
            {
                Interlocked.CompareExchange(ref this.gameState, updatedGameState, currentGameState);

                this.watcherEventHandler.InvokeEvent(WatcherEvents.ClientStatusChanged);
            }
        }
    }
}
