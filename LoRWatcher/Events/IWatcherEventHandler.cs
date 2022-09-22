using System;

namespace LoRWatcher.Events
{
    public interface IWatcherEventHandler
    {
        bool TryAddEvent(WatcherEvents key, string source, Action action);

        bool TryRemoveEvent(WatcherEvents key, string source);

        void InvokeEvent(WatcherEvents key);

        void InvokeAllEvents();
    }
}
