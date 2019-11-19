using LoRService.Models;
using System.Threading.Tasks;

namespace LoRService.Services.MongoDB
{
    public class MongoMatchReportDatabase
        : IMatchReportDatabase
    {
        public async Task<(bool, string)> ReportMatchAsync(MatchReport matchReport)
        {
            var errorMessage = "DB not implemented yet";

            await Task.Yield();

            return (false, errorMessage);
        }
    }
}
