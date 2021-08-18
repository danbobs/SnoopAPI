using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Snoop.EncryptionService.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class EncryptionServiceController : ControllerBase
    {
        private readonly ILogger<EncryptionServiceController> _logger;

        public EncryptionServiceController(ILogger<EncryptionServiceController> logger)
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
        [Route("RotateKey")]
        public IActionResult RotateKey()
        {
            return Ok();
        }

        [HttpGet]
        [Route("HealthCheck")]
        public IActionResult HealthCheck()
        {
            return Ok();
        }
    }
}
