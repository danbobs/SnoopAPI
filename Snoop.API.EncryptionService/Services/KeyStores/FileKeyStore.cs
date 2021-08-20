using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Snoop.API.EncryptionService.Models;
using Snoop.API.EncryptionService.Services.Interfaces;
using Snoop.Common.Models;

namespace Snoop.API.EncryptionService.Services
{
    /// <summary>
    /// Dumb implementation of a key store which stores a list of keys of a given type in a local json file
    /// Not fit for scaling
    /// </summary>
    public class FileKeyStore<T> : IKeyStore<T> where T : Key
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<FileKeyStore<T>> _logger;

        public FileKeyStore(IConfiguration configuration, ILogger<FileKeyStore<T>> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        private string FileStoreDir => _configuration.GetValue<string>("FileKeyStore:FileDir");
        private string KeyType => typeof(T).Name.ToString();
        private string FileStorePath => Path.Combine(this.FileStoreDir, $"{this.KeyType}_Store.json");
        private int KeyRetentionMax => _configuration.GetValue<int>("FileKeyStore:KeyRetentionMax");

        public T GetActiveKey()
        {
            return this.GetKeys().FirstOrDefault();
        }

        public IEnumerable<T> GetKeys()
        {

            if (!File.Exists(this.FileStorePath))
            {
                _logger.LogInformation("GetKeys: FileStore not found at {FilePath}", this.FileStorePath);
                return new List<T>();
            }

            var json = File.ReadAllText(this.FileStorePath);
            return JsonConvert.DeserializeObject<List<T>>(json).OrderByDescending(k => k.Created).Take(this.KeyRetentionMax);
        }

        public void StoreNewKey(T newKey)
        {
           
            var currentKeys = this.GetKeys().Take(this.KeyRetentionMax - 1).ToList();
            currentKeys.Add(newKey);
            _logger.LogInformation("StoreNewKey: Storing new {Type} key. Currently {KeyCount} keys. Retention Max is {KeyRetentionMax}", typeof(T).Name , currentKeys.Count(), this.KeyRetentionMax);
            var json = JsonConvert.SerializeObject(currentKeys);
            File.WriteAllText(this.FileStorePath, json);
            _logger.LogInformation("StoreNewKey: Keys successfully written to {FilePath}", this.FileStorePath);
        }

        public HealthStatus GetStatus()
        {
            try
            {
                var keys = this.GetKeys();

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
    }
}
