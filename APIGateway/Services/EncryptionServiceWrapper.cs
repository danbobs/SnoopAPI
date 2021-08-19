using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Snoop.API.APIGateway.Interfaces;
using Snoop.API.APIGateway.Models;
using Snoop.Common.Models;

namespace Snoop.API.APIGateway.Services
{
    /// <summary>
    /// TODO need to collapse this class into the similarly named class in the Background Key Rotation project and move the result into the common project
    /// Needs a bit of finessing to get the dependency injection working cross-project though
    /// </summary>
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

        private string EncryptionServiceBaseUrl => _configuration.GetValue<string>("APIGateway:EndpointBaseUrl");

        public async Task<EncryptDecryptResult> InvokeEncrypt(string textToEncrypt)
        {
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, "Encrypt");
            request.Content = new StringContent($"\"{textToEncrypt}\"", Encoding.UTF8, "application/json");
            try
            {
                var response = await _httpClient.SendAsync(request);

                return new EncryptDecryptResult()
                {
                    StatusCode = (int)response.StatusCode,
                    Result = await response.Content.ReadAsStringAsync(),
                };

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "InvokeEncrypt: Exception thrown");
                return new EncryptDecryptResult()
                {
                    StatusCode = 0,
                    Result = ex.Message,
                };
            }
        }

        public async Task<EncryptDecryptResult> InvokeDecrypt(string textToDecrypt)
        {
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, "Decrypt");
            request.Content = new StringContent($"\"{textToDecrypt}\"", Encoding.UTF8, "application/json");
            try
            {
                var response = await _httpClient.SendAsync(request);

                return new EncryptDecryptResult()
                {
                    StatusCode = (int)response.StatusCode,
                    Result = await response.Content.ReadAsStringAsync(),
                };

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "InvokeDecrypt: Exception thrown");
                return new EncryptDecryptResult()
                {
                    StatusCode = 0,
                    Result = ex.Message,
                };
            }
        }

        public async Task<HealthStatus> InvokeHealthCheck()
        {
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, "HealthCheck");

            try
            {
                var response = await _httpClient.SendAsync(request);

                return JsonConvert.DeserializeObject<HealthStatus>(await response.Content.ReadAsStringAsync());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "InvokeHealthCheck: Exception thrown");
                return new HealthStatus()
                {
                    Available = false,
                    OldestKey = "Unable to contact encryption service",
                    NewestKey = "Unable to contact encryption service"
                };
            }
        }
    }
}
