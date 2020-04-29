using LoRWatcher.Clients;
using System.Threading;
using System.Threading.Tasks;

namespace LoRWatcher.Caches
{
    public interface IActiveExpeditionCache
    {
        void UpdateState(ExpeditionsState expeditionsState);

        string GetDeckCode();
    }
}
