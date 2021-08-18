using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Microsoft.Extensions.Configuration;
using Snoop.API.EncryptionService.Models;
using Snoop.API.EncryptionService.Services.Interfaces;

namespace Snoop.API.EncryptionService.Services
{
    /// <summary>
    /// Dumb implementation of a key store which stores a list of keys of a given type in a json file
    /// Not fit for scaling
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class FileKeyStore<T> : IKeyStore<T> where T : Key
    {
        private readonly IConfiguration _configuration;

        public FileKeyStore(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public string FileStoreDir => _configuration.GetValue<string>("FileKeyStore:FileDir");
        public string KeyType => typeof(T).Name.ToString();
        public string FileStorePath => Path.Combine(this.FileStoreDir, $"{this.KeyType}_Store.json");
        public int KeyRetentionMax => _configuration.GetValue<int>("FileKeyStore:KeyRetentionMax");

        public T GetActiveKey()
        {
            return this.GetKeys().FirstOrDefault();
        }

        public IEnumerable<T> GetKeys()
        {

            if (!File.Exists(this.FileStorePath))
            {
                return new List<T>();
            }

            var json = File.ReadAllText(this.FileStorePath);
            return JsonConvert.DeserializeObject<List<T>>(json).OrderByDescending(k => k.Created).Take(this.KeyRetentionMax);
        }

        public void StoreNewKey(T newKey)
        {
            var currentKeys = this.GetKeys().Take(this.KeyRetentionMax - 1).ToList();
            currentKeys.Add(newKey);
            var json = JsonConvert.SerializeObject(currentKeys);
            File.WriteAllText(this.FileStorePath, json);
        }
    }
}
