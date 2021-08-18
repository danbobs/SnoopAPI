using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using Snoop.API.EncryptionService.Services.Interfaces;

namespace Snoop.API.EncryptionService.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class EncryptionServiceController : ControllerBase
    {
        private readonly ILogger<EncryptionServiceController> _logger;
        private readonly IEncrypter _encrypter;

        public EncryptionServiceController(ILogger<EncryptionServiceController> logger, IEncrypter encrypter)
        {
            _logger = logger;
            _encrypter = encrypter;
        }

        [HttpPost]
        [Route("Encrypt")]
        public IActionResult Encrypt([FromBody] string stringToEncrypt)
        {
            try
            {
                if (_encrypter.TryEncrypt(stringToEncrypt, out string encrypted))
                {
                    return new OkObjectResult(encrypted);
                }
                else
                {
                    return StatusCode(503, $"Service Unavailable - No keys have been defined");
                }
            }
            catch (Exception ex)
            {
                //_logger.LogError()
                return StatusCode(500, $"Enable to encrypt using active from key store - {ex.Message}");
            }
        }

        [HttpPost]
        [Route("Decrypt")]
        public IActionResult Decrypt([FromBody] string stringToDecrypt)
        {
            try
            {
                if (_encrypter.TryDecrypt(stringToDecrypt, out string decrypted))
                {
                    return new OkObjectResult(decrypted);
                }
                else
                {
                    return StatusCode(500, $"Unable to decrypt with keys in keystore");
                }
            }
            catch (Exception ex)
            {
                //_logger.LogError()
                return StatusCode(500, $"Unable to decrypt using keys from key store - {ex.Message}");
            }
        }

        [HttpGet]
        [Route("RotateKey")]
        public IActionResult RotateKey()
        {
            try
            {
                _encrypter.RotateKeys();
            }
            catch (Exception ex)
            {
                //_logger.LogError()
                return StatusCode(500, $"Unable to create new key in key store {ex.Message}");
            }

            return Ok();
        }

        [HttpGet]
        [Route("HealthCheck")]
        public IActionResult HealthCheck()
        {
            var status = _encrypter.GetStatus();

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
