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
using Snoop.Common.Model;

namespace Snoop.API.EncryptionService.Services
{
    public class AESEncrypter : IEncrypter
    {
        private readonly IKeyStore<AsymmetricKey> _keystore;
        private readonly IKeyGenerator _keyGenerator;
        private readonly ILogger<AESEncrypter> _logger;
        private const int KEY_LENGTH = 16;
        private const PaddingMode PADDING_MODE = PaddingMode.PKCS7; 

        public AESEncrypter(IKeyStore<AsymmetricKey> keyStore, IKeyGenerator keyGenerator, ILogger<AESEncrypter> logger)
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

            var privateKeyBytes = GetBytes(activeKey.Private);
            var publicKeyBytes = GetBytes(activeKey.Public);
            var bytesToEncrypt = GetBytes(textToEncrypt);

            using AesCryptoServiceProvider aes = new AesCryptoServiceProvider();
            aes.Padding = PADDING_MODE;
            using var memStream = new MemoryStream();
            using var cryptoStream = new CryptoStream(memStream, aes.CreateEncryptor(publicKeyBytes, privateKeyBytes), CryptoStreamMode.Write);
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
                var privateKeyBytes = GetBytes(key.Private);
                var publicKeyBytes = GetBytes(key.Public);

                try
                {
                    using AesCryptoServiceProvider aes = new AesCryptoServiceProvider();
                    aes.Padding = PADDING_MODE;
                    using var memStream = new MemoryStream();
                    using var cryptoStream = new CryptoStream(memStream, aes.CreateDecryptor(publicKeyBytes, privateKeyBytes), CryptoStreamMode.Write);
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
            var key = new AsymmetricKey() { Created = DateTime.Now, Public = _keyGenerator.GetUniqueKey(KEY_LENGTH), Private = _keyGenerator.GetUniqueKey(KEY_LENGTH)};
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

        private byte[] GetBytes(string text)
        {
            return Encoding.UTF8.GetBytes(text);
        }
    }
}
