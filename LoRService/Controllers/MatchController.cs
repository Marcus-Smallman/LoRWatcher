using System.Net;
using System.Threading.Tasks;
using LoRService.Models;
using LoRService.Services;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace LoRService.Controllers
{
    [Route("api/v1/match")]
    [ApiController]
    public class MatchController : ControllerBase
    {
        private readonly IMatchReportDatabase matchReportDatabase;

        public MatchController(IMatchReportDatabase matchReportDatabase)
        {
            this.matchReportDatabase = matchReportDatabase;
        }

        [HttpPost("report")]
        public async Task<IActionResult> ReportAsync(MatchReport matchReport)
        {
            // Add validation on model. Check deck is correct using library?

            var (result, errorMessage) = await this.matchReportDatabase.ReportMatchAsync(matchReport);
            if (result == true)
            {
                return new ContentResult
                {
                    Content = JsonConvert.SerializeObject(matchReport),
                    ContentType = "application/json",
                    StatusCode = (int)HttpStatusCode.Created
                };
            }

            return new ContentResult
            {
                Content = JsonConvert.SerializeObject(new { error = errorMessage }),
                ContentType = "application/json",
                StatusCode = (int)HttpStatusCode.InternalServerError
            };
        }
    }
}
