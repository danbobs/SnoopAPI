using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Snoop.API.APIGateway.Interfaces;
using Snoop.API.APIGateway.Models;

namespace Snoop.API.APIGateway.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class APIGatewayController : ControllerBase
    {
        private readonly ILogger<APIGatewayController> _logger;
        private readonly IEncryptionServiceWrapper _encryptionServiceWrapper;

        public APIGatewayController(ILogger<APIGatewayController> logger, IEncryptionServiceWrapper encryptionServiceWrapper)
        {
            _logger = logger;
            _encryptionServiceWrapper = encryptionServiceWrapper;
        }

        [HttpPost]
        [Route("Encrypt")]
        public async Task<IActionResult> Encrypt([FromBody] string stringToEncrypt)
        {
            EncryptDecryptResult result = await _encryptionServiceWrapper.InvokeEncrypt(stringToEncrypt);

            return StatusCode(result.StatusCode, result.Result);
        }

        [HttpPost]
        [Route("Decrypt")]
        public async Task<IActionResult> Decrypt([FromBody] string stringToDecrypt)
        {
            EncryptDecryptResult result = await _encryptionServiceWrapper.InvokeDecrypt(stringToDecrypt);

            return StatusCode(result.StatusCode, result.Result);
        }


        [HttpGet]
        [Route("HealthCheck")]
        public async Task<IActionResult> HealthCheck()
        {
            var status = await _encryptionServiceWrapper.InvokeHealthCheck();

            if (status.Available)
            {
                return new OkObjectResult(status);
            }
            else
            {
                return StatusCode(503, status);
            }
        }
    }
}
