using System.Threading;

namespace LoRWatcher.Tray
{
    public interface ITrayIcon
    {
        void Configure(CancellationTokenSource tokenSource = null);
    }
}
