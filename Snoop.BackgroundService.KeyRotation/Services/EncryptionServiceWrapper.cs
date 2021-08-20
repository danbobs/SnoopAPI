using System;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Snoop.Background.KeyRotation.Interfaces;
using Snoop.Background.KeyRotation.Models;


/// <summary>
/// TODO need to collapse this class into the similarly named class in the APIGateway project and move the result into the common project
/// Needs a bit of finessing to get the dependency injection working cross-project though
/// </summary>
namespace Snoop.Background.KeyRotation.Services
{
    public class EncryptionServiceWrapper : IEncryptionServiceWrapper
    {
        private readonly IConfiguration _configuration;
        private HttpClient _httpClient;
        private readonly ILogger<EncryptionServiceWrapper> _logger;

        public EncryptionServiceWrapper(IConfiguration configuration, ILogger<EncryptionServiceWrapper> logger)
        {
            _configuration = configuration;
            _httpClient = new HttpClient();
            _httpClient.BaseAddress = new Uri(this.EncryptionServiceBaseUrl);
            _logger = logger;
        }

        private string EncryptionServiceBaseUrl => _configuration.GetValue<string>("KeyRotator:EndpointBaseUrl");

        public async Task<KeyRotationResult> InvokeRotateKeys()
        {
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, "RotateKey");

            try
            {
                var response = await _httpClient.SendAsync(request);

                return new KeyRotationResult()
                {
                    Success = response.IsSuccessStatusCode,
                    ErrorMessage = await response.Content.ReadAsStringAsync()
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "InvokeRotateKeys: Exception thrown");
                return new KeyRotationResult()
                {
                    Success = false,
                    ErrorMessage = ex.Message
                };
            }
        }
    }
}
