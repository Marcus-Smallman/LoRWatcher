using LoRService.Models;
using System.Collections.Generic;
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

        /// <summary>
        /// Gets all match reports for a player.
        /// </summary>
        /// <param name="playerName">The name of the player.</param>
        /// <returns>An enumerable of match reports for a player; else null with an error message.</returns>
        Task<(IEnumerable<MatchReport>, string)> GetPlayerMatchReportsAsync(string playerName);
    }
}
