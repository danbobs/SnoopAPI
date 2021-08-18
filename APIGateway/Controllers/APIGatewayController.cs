using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Snoop.APIGateway.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class APIGatewayController : ControllerBase
    {
        private readonly ILogger<APIGatewayController> _logger;

        public APIGatewayController(ILogger<APIGatewayController> logger)
        {
            _logger = logger;
        }

        [HttpPost]
        [Route("Encrypt")]
        public string Encrypt([FromBody] string stringToEncrypt)
        {
            return $"{stringToEncrypt} - encrypted";
        }

        [HttpPost]
        [Route("Decrypt")]
        public string Decrypt([FromBody] string stringToDecrypt)
        {
            return $"{stringToDecrypt} - decrypted";
        }


        [HttpGet]
        [Route("HealthCheck")]
        public IActionResult HealthCheck()
        {
            return Ok();
        }
    }
}
