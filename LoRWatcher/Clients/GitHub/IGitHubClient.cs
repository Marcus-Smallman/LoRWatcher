using System.Threading;
using System.Threading.Tasks;

namespace LoRWatcher.Clients.GitHub
{
    public interface IGitHubClient
    {
        Task<GitHubRelease> GetLatestVersionAsync(CancellationToken cancellationToken = default);
    }
}
