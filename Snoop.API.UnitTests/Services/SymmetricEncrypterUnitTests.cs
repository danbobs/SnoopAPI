using System.Security.Cryptography;
using System.Text;
using AutoFixture;
using FluentAssertions;
using Moq;
using Snoop.API.EncryptionService.Models;
using Snoop.API.EncryptionService.Services;
using Snoop.API.EncryptionService.Services.Interfaces;
using Snoop.Common.Models;
using Xunit;

namespace Snoop.API.UnitTests
{
    public class SymmetricEncrypterUnitTests : TestBase
    {

        private readonly Mock<IKeyGenerator> _keyGeneratorMock;
        private readonly Mock<IKeyStore<SymmetricKey>> _keyStoreMock;
        private readonly Mock<SymmetricAlgorithm> _symmetricAlgorithmMock;

        public SymmetricEncrypterUnitTests()
        {
            // create manually with Moq. Autofixture has trouble with this abstract class
            _symmetricAlgorithmMock = new Mock<SymmetricAlgorithm>();
            Fixture.Inject<SymmetricAlgorithm>(_symmetricAlgorithmMock.Object);

            _keyGeneratorMock = Mock.Get<IKeyGenerator>(this.Fixture.Freeze<IKeyGenerator>());
            _keyStoreMock = Mock.Get<IKeyStore<SymmetricKey>>(this.Fixture.Freeze<IKeyStore<SymmetricKey>>());
        }

        [Fact]
        public void RotateKeysTest()
        {
            var uniqueKey1 = Fixture.Create<string>();
            var uniqueKey2 = Fixture.Create<string>();

            _keyGeneratorMock.SetupSequence(kg => kg.GetUniqueKey(It.IsAny<int>()))
                .Returns(uniqueKey1)
                .Returns(uniqueKey2);

            var symmetricKeyEncrypter = this.Fixture.Create<SymmetricKeyEncrypter>();
            symmetricKeyEncrypter.RotateKeys();

            // check the key generator was asked for a couple of keys of the correct length
            _keyGeneratorMock.Verify(kg => kg.GetUniqueKey(It.Is<int>(i => i == SymmetricKeyEncrypter.KEY_LENGTH)), Times.Exactly(2));

            // check the key store was written to
            _keyStoreMock.Verify(ks => ks.StoreNewKey(It.Is<SymmetricKey>(sk => sk.Key == uniqueKey1 && sk.InitializationVector == uniqueKey2)), Times.Once);
        }
        
        [Fact]
        public void GetStatus()
        {
            var expectedStatus = this.Fixture.Freeze<HealthStatus>();

            var symmetricKeyEncrypter = this.Fixture.Create<SymmetricKeyEncrypter>();
            var status = symmetricKeyEncrypter.GetStatus();

            status.Available.Should().Be(expectedStatus.Available);
            status.NewestKey.Should().Be(expectedStatus.NewestKey);
            status.OldestKey.Should().Be(expectedStatus.OldestKey);

            _keyStoreMock.Verify(ks => ks.GetStatus(), Times.Once);
        }


        [Fact]
        public void TryEncrypt()
        {
            var textToEncrypt = Fixture.Create<string>();
            var activeKey = Fixture.Create<SymmetricKey>();
            byte[] keyBytes = null;
            byte[] ivBytes = null;

            _keyStoreMock.Setup(ks => ks.GetActiveKey()).Returns(activeKey);
            _symmetricAlgorithmMock.Setup(sa => sa.CreateEncryptor(It.IsAny<byte[]>(), It.IsAny<byte[]>()))
                .Returns(Fixture.Create<ICryptoTransform>())
                .Callback<byte[], byte[]>((k, iv) => { keyBytes = k; ivBytes = iv; });

            var symmetricKeyEncrypter = this.Fixture.Create<SymmetricKeyEncrypter>();
            bool success = symmetricKeyEncrypter.TryEncrypt(textToEncrypt, out string decrypted);

            success.Should().BeTrue();
            decrypted.Should().NotBe(textToEncrypt);    // more work to do here

            _keyStoreMock.Verify(ks => ks.GetActiveKey(), Times.Once);
            _symmetricAlgorithmMock.Verify((sa => sa.CreateEncryptor(It.IsAny<byte[]>(), It.IsAny<byte[]>())), Times.Once);

            // Check an encrypter the active key was used for the encryption
            keyBytes.Should().BeEquivalentTo(GetBytes(activeKey.Key));
            ivBytes.Should().BeEquivalentTo(GetBytes(activeKey.InitializationVector));

        }

        private byte[] GetBytes(string text)
        {
            return Encoding.UTF8.GetBytes(text);
        }


        // And so on...

    }
}
