﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Microsoft.Extensions.Configuration;
using Snoop.API.APIGateway.Interfaces;
using Snoop.API.APIGateway.Models;
using Snoop.Common.Model;

namespace Snoop.API.APIGateway.Services
{
    public class EncryptionServiceWrapper : IEncryptionServiceWrapper
    {
        private readonly IConfiguration _configuration;
        private HttpClient _httpClient;

        public EncryptionServiceWrapper(IConfiguration configuration)
        {
            _configuration = configuration;
            _httpClient = new HttpClient();
            _httpClient.BaseAddress = new Uri(this.EncryptionServiceBaseUrl);

        }

        private string EncryptionServiceBaseUrl => _configuration.GetValue<string>("APIGateway:EndpointBaseUrl");

        public async Task<EncryptDecryptResult> InvokeEncrypt(string stringToEncrypt)
        {
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, "Encrypt");
            request.Content = new StringContent($"\"{stringToEncrypt}\"", Encoding.UTF8, "application/json");
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
                return new EncryptDecryptResult()
                {
                    StatusCode = 0,
                    Result = ex.Message,
                };
            }
        }

        public async Task<EncryptDecryptResult> InvokeDecrypt(string stringToDecrypt)
        {
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, "Decrypt");
            request.Content = new StringContent($"\"{stringToDecrypt}\"", Encoding.UTF8, "application/json");
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
            catch (Exception)
            {
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