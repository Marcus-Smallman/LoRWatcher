using LoRService.Models;
using System.Threading.Tasks;

namespace LoRService.Services
{
    public interface IMatchReportDatabase
    {
        /// <summary>
        /// Stores the match result in the database.
        /// </summary>
        /// <param name="matchReport">The match to report.</param>
        /// <returns>True if successful, else false and an error message why it failed.</returns>
        Task<(bool, string)> ReportMatchAsync(MatchReport matchReport);
    }
}
