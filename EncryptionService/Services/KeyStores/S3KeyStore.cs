using Amazon.S3;
using Amazon.S3.Transfer;
using Amazon.S3.Model;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Snoop.API.EncryptionService.Models;
using Snoop.API.EncryptionService.Services.Interfaces;
using Snoop.Common.Models;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Snoop.API.EncryptionService.Services
{
    public class S3KeyStore<T> : IKeyStore<T> where T : Key
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<FileKeyStore<T>> _logger;
        private readonly IAmazonS3 _s3Client;

        public S3KeyStore(IConfiguration configuration, ILogger<FileKeyStore<T>> logger, IAmazonS3 s3Client)
        {
            _configuration = configuration;
            _logger = logger;
            _s3Client = s3Client;
        }

        private string S3Bucket => _configuration.GetValue<string>("S3KeyStore:BucketName");
        private string KeyType => typeof(T).Name.ToString();
        private string Blobname => Path.Combine($"{this.KeyType}_Store.json");
        private int KeyRetentionMax => _configuration.GetValue<int>("S3KeyStore:KeyRetentionMax");

        public T GetActiveKey()
        {
            return this.GetKeys().FirstOrDefault();
        }

        public IEnumerable<T> GetKeys()
        {
            if (!this.BlobExists())
            {
                _logger.LogInformation("GetKeys: S3Store not found at {Blobname} in {Bucket}", this.Blobname, this.S3Bucket);
                return new List<T>();
            }

            var json = this.DownloadS3Blob();
            return JsonConvert.DeserializeObject<List<T>>(json).OrderByDescending(k => k.Created).Take(this.KeyRetentionMax);
        }

        public void StoreNewKey(T newKey)
        {

            var currentKeys = this.GetKeys().Take(this.KeyRetentionMax - 1).ToList();
            currentKeys.Add(newKey);
            _logger.LogInformation("StoreNewKey: Storing new {Type} key. Currently {KeyCount} keys. Retention Max is {KeyRetentionMax}", typeof(T).Name, currentKeys.Count(), this.KeyRetentionMax);

            var json = JsonConvert.SerializeObject(currentKeys);
            this.UploadS3Blob(json);
            _logger.LogInformation("StoreNewKey: Keys successfully written to {Blobname} in {Bucket}", this.Blobname, this.S3Bucket);
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

        private void UploadS3Blob(string json)
        {
            TransferUtility utility = new TransferUtility(_s3Client);
            using var memStream = GenerateStreamFromString(json);
            TransferUtilityUploadRequest request = new TransferUtilityUploadRequest() { BucketName = this.S3Bucket, Key = this.Blobname, InputStream = memStream};
            utility.Upload(request);
        }

        private string DownloadS3Blob()
        {
            var response = _s3Client.GetObjectAsync(this.S3Bucket, this.Blobname).Result;
            MemoryStream memoryStream = new MemoryStream();

            using var responseStream = response.ResponseStream;
            responseStream.CopyTo(memoryStream);
            return Encoding.UTF8.GetString(memoryStream.ToArray());
        }

        private bool BlobExists()
        {
            ListObjectsV2Request request = new ListObjectsV2Request() { BucketName = this.S3Bucket };
            ListObjectsV2Response response = _s3Client.ListObjectsV2Async(request).Result;

            return response.S3Objects.Any(o => o.Key == this.Blobname);
        }

        private static MemoryStream GenerateStreamFromString(string value)
        {
            return new MemoryStream(Encoding.UTF8.GetBytes(value ?? ""));
        }

    }
}
