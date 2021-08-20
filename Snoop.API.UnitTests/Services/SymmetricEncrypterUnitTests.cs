using System;
using Xunit;
using AutoFixture;
using AutoFixture.Xunit2;
using AutoFixture.AutoMoq;
using FluentAssertions;
using System.Security.Cryptography;
using Moq;
using Snoop.API.EncryptionService.Services.Interfaces;
using Snoop.API.EncryptionService.Services;
using Snoop.API.EncryptionService.Models;

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

            // check the key generator was asked for a couple of keys of the same length
            _keyGeneratorMock.Verify(kg => kg.GetUniqueKey(It.Is<int>(i => i == SymmetricKeyEncrypter.KEY_LENGTH)), Times.Exactly(2));

            // check the key store was written to
            _keyStoreMock.Verify(ks => ks.StoreNewKey(It.Is<SymmetricKey>(sk => sk.Key == uniqueKey1 && sk.InitializationVector == uniqueKey2)), Times.Once);

        }

    }
}
