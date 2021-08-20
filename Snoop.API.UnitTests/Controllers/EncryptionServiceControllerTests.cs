using System;
using System.Net;
using System.Threading.Tasks;
using Xunit;
using AutoFixture;
using AutoFixture.Xunit2;
using AutoFixture.AutoMoq;
using FluentAssertions;
using System.Linq;
using System.Text;
using System.Security.Cryptography;
using Moq;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using System.Net.Http;
using System.Net.Http.Headers;
using Snoop.API.EncryptionService.Services.Interfaces;
using Snoop.API.EncryptionService.Services;
using Snoop.API.EncryptionService.Models;
using Snoop.Common.Models;

namespace Snoop.API.UnitTests.Controllers
{
    // Unit tests for EncryptionServiceController checking its message passing to it dependencies (IEncyptor)

    public class EncryptionServiceControllerTests : TestBase,  IClassFixture<WebApplicationFactory<Snoop.API.EncryptionService.Startup>>
    {
        private WebApplicationFactory<Snoop.API.EncryptionService.Startup> _factory;
        private readonly HttpClient _httpClient;
        private Mock<IEncrypter> _encrypterMock;

        public EncryptionServiceControllerTests(WebApplicationFactory<Snoop.API.EncryptionService.Startup> factory)
        {
            _factory = factory;

            _encrypterMock = Mock.Get<IEncrypter>(this.Fixture.Freeze<IEncrypter>());

            _httpClient = _factory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureTestServices(services =>
                {
                    services.AddSingleton<IEncrypter>(_encrypterMock.Object);
                });
            }).CreateClient(new WebApplicationFactoryClientOptions());
        }

        [Fact]
        public async Task RotateKeys()
        {
            var response = await _httpClient.GetAsync("/EncryptionService/RotateKey");
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            _encrypterMock.Verify(e => e.RotateKeys(), Times.Once);
        }

        [Fact]
        public async Task RotateKeys_EncryptorException()
        {
            var exception = Fixture.Create<Exception>();
            _encrypterMock.Setup(e => e.RotateKeys()).Throws(exception);

            var response = await _httpClient.GetAsync("/EncryptionService/RotateKey");
            response.StatusCode.Should().Be(HttpStatusCode.InternalServerError);
            (await response.Content.ReadAsStringAsync()).Should().Be($"Unable to create new key in key store {exception.Message}");

            _encrypterMock.Verify(e => e.RotateKeys(), Times.Once);
        }

        [Fact]
        public async Task Encrypt()
        {
            var textToEncode = Fixture.Create<string>();
            var encodedText = Fixture.Create<string>();

            var content = new StringContent($"\"{textToEncode}\"", Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync("/EncryptionService/Encrypt", content);
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            _encrypterMock.Verify(e => e.TryEncrypt(It.Is<string>(s => s == textToEncode), out It.Ref<string>.IsAny), Times.Once);
        }

        [Fact]
        public async Task Encrypt_NoKeys()
        {
            var textToEncode = Fixture.Create<string>();
            var encodedText = Fixture.Create<string>();

            _encrypterMock.Setup(e => e.TryEncrypt(It.IsAny<string>(), out It.Ref<string>.IsAny)).Returns(false);

            var content = new StringContent($"\"{textToEncode}\"", Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync("/EncryptionService/Encrypt", content);
            response.StatusCode.Should().Be(HttpStatusCode.ServiceUnavailable);
            (await response.Content.ReadAsStringAsync()).Should().Be("Service Unavailable - No keys have been defined");

            _encrypterMock.Verify(e => e.TryEncrypt(It.Is<string>(s => s == textToEncode), out It.Ref<string>.IsAny), Times.Once);
        }

    }
}
