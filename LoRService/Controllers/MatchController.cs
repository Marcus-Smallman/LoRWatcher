using System.Net;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace LoRService.Controllers
{
    [Route("api/v1/match")]
    [ApiController]
    public class MatchController : ControllerBase
    {
        [HttpPost("report")]
        public IActionResult Report(string value)
        {
            return new ContentResult
            {
                Content = JsonConvert.SerializeObject(value),
                ContentType = "application/json",
                StatusCode = (int)HttpStatusCode.Created
            };
        }
    }
}
