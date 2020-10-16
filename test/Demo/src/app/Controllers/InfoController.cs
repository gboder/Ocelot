using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace app.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class InfoController : ControllerBase
    {
        private readonly ILogger<InfoController> logger;

        public InfoController(ILogger<InfoController> logger)
        {
            if (logger is null)
            {
                throw new ArgumentNullException(nameof(logger));
            }

            this.logger = logger;
        }

        [HttpGet("header")]
        public IActionResult GetHeader()
        {
            var headers = Request.Headers;
            foreach (var header in headers)
            {
                logger.LogInformation($"{header.Key}::\t{string.Join(',', header.Value)}");
            }
            return Ok(headers);
        }

        [HttpGet("env")]
        public IActionResult GetEnv()
        {
            var headers = Environment.GetEnvironmentVariables();
            foreach (var header in headers)
            {
                logger.LogInformation($"{header}");
            }
            return Ok(headers);
        }

        [HttpGet("health")]
        public IActionResult GetHealth()
        {
            return Ok(new
            {
                Environment.MachineName,
                DateTime.Now
            });
        }
    }
}
