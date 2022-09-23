using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace LoRWatcher.Events
{
    public class WatcherEventHandler
        : IWatcherEventHandler
    {
        private ConcurrentDictionary<WatcherEvents, ConcurrentDictionary<string, Action>> events;

        public WatcherEventHandler()
        {
            this.events = new ConcurrentDictionary<WatcherEvents, ConcurrentDictionary<string, Action>>();
        }

        public bool TryAddEvent(WatcherEvents key, string source, Action action)
        {
            if (this.events.ContainsKey(key) == true)
            {
                var events = this.events[key];
                if (events.ContainsKey(source) == false)
                {
                    return events.TryAdd(source, action);
                }
                else
                {
                    events[source] = action;

                    return true;
                }
            }
            else
            {
                var @event = new ConcurrentDictionary<string, Action>();
                if (@event.TryAdd(source, action))
                {
                    return this.events.TryAdd(key, @event);
                }
            }

            return false;
        }

        public bool TryRemoveEvent(WatcherEvents key, string source)
        {
            if (this.events.ContainsKey(key) == true)
            {
                var events = this.events[key];
                if (events.ContainsKey(source) == true)
                {
                    return events.TryRemove(source, out Action _);
                }
            }

            return true;
        }

        public void InvokeEvent(WatcherEvents key)
        {
            if (this.events.ContainsKey(key) == true)
            {
                var events = this.events[key];
                foreach (var @event in events)
                {
                    @event.Value?.Invoke();
                }
            }
        }

        public void InvokeAllEvents()
        {
            var events = this.events.ToArray();
            foreach (var @event in events)
            {
                var sourceEvents = @event.Value.ToArray();
                foreach (var sourceEvent in sourceEvents)
                {
                    sourceEvent.Value?.Invoke();
                }
            }
        }
    }
}
