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
        public IActionResult Encrypt([FromBody] string textToEncrypt)
        {
            try
            {
                if (_encrypter.TryEncrypt(textToEncrypt, out string encrypted))
                {
                    return new OkObjectResult(encrypted);
                }
                else
                {
                    _logger.LogWarning("Encrypt failed. {status}. {reason}.","failed","No keys defined");
                    return StatusCode(503, "Service Unavailable - No keys have been defined");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Encrypt: Exception thrown");
                return StatusCode(500, $"Unable to encrypt using active key from key store - {ex.Message}");
            }
        }

        [HttpPost]
        [Route("Decrypt")]
        public IActionResult Decrypt([FromBody] string textToDecrypt)
        {
            try
            {
                if (_encrypter.TryDecrypt(textToDecrypt, out string decrypted))
                {
                    return new OkObjectResult(decrypted);
                }
                else
                {
                    _logger.LogWarning("Decrypt {status}. {reason}.","failed", "Unable to decrypt with keys in keystore");
                    return StatusCode(500, $"Unable to decrypt with keys in keystore");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Decrypt: Exception thrown");
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
                _logger.LogError(ex, "RotateKey: Exception thrown");
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
