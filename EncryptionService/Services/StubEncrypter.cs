using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Snoop.API.EncryptionService.Models;
using Snoop.API.EncryptionService.Services.Interfaces;
using Snoop.Common.Model;

namespace Snoop.API.EncryptionService.Services
{
    /// <summary>
    /// A very stupid stub implementation of encryption that just appends the key onto string and then tries to strip it off when decrypting
    /// </summary>
    public class StubEncrypter : IEncrypter
    {
        private readonly IKeyStore<SimpleKey> _keystore;
        private readonly IKeyGenerator _keyGenerator;
        private const int KEY_LENGTH = 10;

        public StubEncrypter(IKeyStore<SimpleKey> keyStore, IKeyGenerator keyGenerator)
        {
            _keystore = keyStore;
            _keyGenerator = keyGenerator;
        }
        public bool TryEncrypt(string stringToEncrypt, out string encrypted)
        {
            encrypted = null;
            var activeKey = _keystore.GetActiveKey();

            if (activeKey == null)
            {
                return false;
            }

            encrypted = $"{stringToEncrypt}{GetEncryptionSuffix(activeKey.Value)}";

            return true;
        }

        public bool TryDecrypt(string stringToDecrypt, out string decrypted)
        {
            decrypted = null;
            var keys = _keystore.GetKeys();

            if (!keys.Any())
            {
                return false;
            }

            foreach (var key in keys)
            {
                if (stringToDecrypt.EndsWith(GetEncryptionSuffix(key.Value)))
                {
                    decrypted = stringToDecrypt.Replace(GetEncryptionSuffix(key.Value), string.Empty);
                    return true;
                }
            }

            return false;
        }

        public void RotateKeys()
        {
            var key = new SimpleKey() { Created = DateTime.Now, Value = _keyGenerator.GetUniqueKey(KEY_LENGTH) };
            _keystore.StoreNewKey(key);
        }

        public HealthStatus GetStatus()
        {
            try
            {
                var keys = _keystore.GetKeys();

                if (!keys.Any())
                {
                    return new HealthStatus() { Available = false, NewestKey = "No keys in key store", OldestKey = "No keys in key store" };
                }

                var newestKeyStr = keys.First().Created.ToString();
                var oldestKeyStr = keys.Last().Created.ToString();

                return new HealthStatus() { Available = true, NewestKey = newestKeyStr, OldestKey = oldestKeyStr };

            }
            catch
            {
                return new HealthStatus() { Available = false, NewestKey = "Unable to contact key store", OldestKey = "Unable to contact key store" };
            }
        }

        private string GetEncryptionSuffix(string key)
        {
            return $" - encrypted with {key}";
        }
    }
}
