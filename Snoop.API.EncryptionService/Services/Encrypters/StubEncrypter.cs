using System;
using System.Linq;
using Microsoft.Extensions.Logging;
using Snoop.API.EncryptionService.Models;
using Snoop.API.EncryptionService.Services.Interfaces;
using Snoop.Common.Models;

namespace Snoop.API.EncryptionService.Services
{
    /// <summary>
    /// A very stupid stub implementation of encryption that just appends the key onto string and then tries to strip it off when decrypting
    /// </summary>
    public class StubEncrypter : IEncrypter
    {
        private readonly IKeyStore<SimpleKey> _keystore;
        private readonly IKeyGenerator _keyGenerator;
        private readonly ILogger<StubEncrypter> _logger;
        private const int KEY_LENGTH = 10;

        public StubEncrypter(IKeyStore<SimpleKey> keyStore, IKeyGenerator keyGenerator, ILogger<StubEncrypter> logger)
        {
            _keystore = keyStore;
            _keyGenerator = keyGenerator;
            _logger = logger;
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

            _logger.LogInformation("TryEncrypt: encryption {status}", "succeeded");
            encrypted = $"{textToEncrypt}{GetEncryptionSuffix(activeKey.Value)}";

            return true;
        }

        public bool TryDecrypt(string textToDecrypt, out string decrypted)
        {
            decrypted = null;
            var keys = _keystore.GetKeys();

            if (!keys.Any())
            {
                return false;
            }

            foreach (var key in keys)
            {
                if (textToDecrypt.EndsWith(GetEncryptionSuffix(key.Value)))
                {
                    _logger.LogInformation("TryDecrypt: decryption {status}", "succeeded");
                    decrypted = textToDecrypt.Replace(GetEncryptionSuffix(key.Value), string.Empty);
                    return true;
                }
            }

            _logger.LogInformation("TryDecrypt: decryption {status}. {reason}", "failed", "No matching key found");
            return false;
        }

        public void RotateKeys()
        {
            var key = new SimpleKey() { Created = DateTime.Now, Value = _keyGenerator.GetUniqueKey(KEY_LENGTH) };
            _keystore.StoreNewKey(key);
        }

        public HealthStatus GetStatus()
        {
            return _keystore.GetStatus();
        }

        private string GetEncryptionSuffix(string key)
        {
            return $" - encrypted with {key}";
        }
    }
}
