using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Threading.Tasks;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Logging;
using Snoop.API.EncryptionService.Models;
using Snoop.API.EncryptionService.Services.Interfaces;
using Snoop.Common.Models;

namespace Snoop.API.EncryptionService.Services
{
    public class SymmetricKeyEncrypter : IEncrypter
    {
        public const int KEY_LENGTH = 16; // 16 bytes * 8 bits = 128 bit encryption - todo: use SymmetricAlgorithm.LegalKeySizes
        private readonly SymmetricAlgorithm _symmetricAlgorithm;
        private readonly IKeyStore<SymmetricKey> _keystore;
        private readonly IKeyGenerator _keyGenerator;
        private readonly ILogger<SymmetricAlgorithm> _logger;
        private const PaddingMode PADDING_MODE = PaddingMode.PKCS7; 

        public SymmetricKeyEncrypter(IKeyStore<SymmetricKey> keyStore, IKeyGenerator keyGenerator, ILogger<SymmetricAlgorithm> logger, SymmetricAlgorithm symmetricAlgorithm)
        {
            _keystore = keyStore;
            _keyGenerator = keyGenerator;
            _logger = logger;
            _symmetricAlgorithm = symmetricAlgorithm;
        }
        public bool TryEncrypt(string textToEncrypt, out string encrypted)
        {
            encrypted = null;
            var activeKey = _keystore.GetActiveKey();

            if (activeKey == null)
            {
                _logger.LogInformation("TryEncrypt: encryption {status}. {reason}.", "failed", "No active key");
                return false;
            }

            var privateKeyBytes = GetBytes(activeKey.InitializationVector);
            var publicKeyBytes = GetBytes(activeKey.Key);
            var bytesToEncrypt = GetBytes(textToEncrypt);

            _symmetricAlgorithm.Padding = PADDING_MODE;
            using var memStream = new MemoryStream();
            using var cryptoStream = new CryptoStream(memStream, _symmetricAlgorithm.CreateEncryptor(publicKeyBytes, privateKeyBytes), CryptoStreamMode.Write);
            cryptoStream.Write(bytesToEncrypt, 0, bytesToEncrypt.Length);
            cryptoStream.FlushFinalBlock();
            encrypted = Convert.ToBase64String(memStream.ToArray());

            _logger.LogInformation("TryEncrypt: encryption {status}", "succeeded");       

            return true;
        }

        public bool TryDecrypt(string textToDecrypt, out string decrypted)
        {
            decrypted = null;
            var keys = _keystore.GetKeys();
            int count = 1;

            if (!keys.Any())
            {
                return false;
            }

            var bytesToDecrypt = Convert.FromBase64String(textToDecrypt);

            foreach (var key in keys)
            {
                var privateKeyBytes = GetBytes(key.InitializationVector);
                var publicKeyBytes = GetBytes(key.Key);

                try
                {
                    _symmetricAlgorithm.Padding = PADDING_MODE;
                    using var memStream = new MemoryStream();
                    using var cryptoStream = new CryptoStream(memStream, _symmetricAlgorithm.CreateDecryptor(publicKeyBytes, privateKeyBytes), CryptoStreamMode.Write);
                    cryptoStream.Write(bytesToDecrypt, 0, bytesToDecrypt.Length);
                    cryptoStream.FlushFinalBlock();
                    decrypted = Encoding.UTF8.GetString(memStream.ToArray());
                    _logger.LogInformation("TryDecrypt: key {count} decryption {status}", count, "succeeded");
                    return true;
                }
                catch (Exception ex)
                {
                    _logger.LogInformation("TryDecrypt: key {count} decryption {status} - {reason}", count, "failed", ex.Message);
                }
                finally
                {
                    count++;
                }
            }

            _logger.LogInformation("TryDecrypt: decryption {status}. {reason}", "failed", "No matching key found");
            return false;
        }

        public void RotateKeys()
        {
            var key = new SymmetricKey() { Created = DateTime.Now, Key = _keyGenerator.GetUniqueKey(KEY_LENGTH), InitializationVector = _keyGenerator.GetUniqueKey(KEY_LENGTH)};
            _keystore.StoreNewKey(key);
        }

        public HealthStatus GetStatus()
        {
            return _keystore.GetStatus();
        }

        private byte[] GetBytes(string text)
        {
            return Encoding.UTF8.GetBytes(text);
        }
    }
}
