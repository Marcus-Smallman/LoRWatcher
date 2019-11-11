using LoRWatcher.Clients;

namespace LoRWatcher.Caches
{
    public interface ICache
    {
        bool IsEmpty { get; }

        MatchReport GetMatchReport();

        void UpdateActiveMatch(PositionalRectangles positionalRectangles);
    }
}
