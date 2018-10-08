using System;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Configuration;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using Newtonsoft.Json;

namespace FileStorage.Azure
{
    public class AzureCdnSettings
    {
        public AzureCdnSettings()
        {
            Enabled = false;
            UrlExpirationInSeconds = 259200; // Default time is 7 days. Minumum time is 300 seconds
        }

        [JsonProperty("filestorage_azure_cdn_enabled")]
        public bool Enabled { get; set; }

        [JsonProperty("filestorage_azure_cdn_url")]
        public string Url { get; set; }

        [JsonProperty("filestorage_azure_cdn_url_expiration_in_seconds")]
        public uint UrlExpirationInSeconds { get; set; }
    }

    public class AzureStorageSettings
    {
        public CloudBlobContainer Container { get; set; }
        public UrlExpiration UrlExpiration { get; set; }
        public AzureCdnSettings Cdn { get; set; }
        public AzureCacheControlExpiration CacheControlExpiration { get; set; }

        readonly Regex containerRegex = new Regex("^(?!-)(?!.*--)[a-z0-9-]{3,63}(?<!-)$");


        public AzureStorageSettings(IConfiguration configuration) : this(
            configuration["filestorage_azure_connectionstring"],
            configuration["filestorage_azure_container"],
            configuration.GetValue<uint>("filestorage_azure_url_expiration_in_seconds", 0),
            configuration.GetValue<uint>("filestorage_azure_cache_control_expiration", 259200),
            JsonConvert.DeserializeObject(configuration["filestorage_azure_cdn"], typeof(AzureCdnSettings)) as AzureCdnSettings ?? new AzureCdnSettings())
        {

        }

        public AzureStorageSettings(string connectionString, string containerName, uint urlExpirationInSeconds, ulong cacheControlExpiration, AzureCdnSettings cdn)
        {
            if (string.IsNullOrEmpty(connectionString)) throw new ArgumentNullException(nameof(connectionString));
            if (containerRegex.IsMatch(containerName) == false) throw new FormatException("Not supported Azure container name. Check https://blogs.msdn.microsoft.com/jmstall/2014/06/12/azure-storage-naming-rules/");
            var storageAccount = CloudStorageAccount.Parse(connectionString);

            if (storageAccount is null) throw new ArgumentNullException(nameof(storageAccount), "Unable to parse and get storage account using `CloudStorageAccount.Parse(connectionString)`");
            var blobClient = storageAccount.CreateCloudBlobClient();

            if (blobClient is null) throw new ArgumentNullException(nameof(blobClient), "Unable to get blobClient using `storageAccount.CreateCloudBlobClient()`");
            Container = blobClient.GetContainerReference(containerName);

            if (Container is null) throw new ArgumentNullException(nameof(Container), "Unable to get Container using `blobClient.GetContainerReference(containerName)`");
            bool created = Container.CreateIfNotExistsAsync().Result;

            UrlExpiration = new UrlExpiration(urlExpirationInSeconds);
            CacheControlExpiration = new AzureCacheControlExpiration(cacheControlExpiration);
            Cdn = cdn;
        }
    }
}
