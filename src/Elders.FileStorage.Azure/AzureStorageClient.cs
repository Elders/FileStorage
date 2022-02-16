using System;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Options;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;

namespace FileStorage.Azure
{
    public class AzureStorageClient
    {
        readonly Regex containerRegex = new Regex("^(?!-)(?!.*--)[a-z0-9-]{3,63}(?<!-)$");

        public AzureStorageClient(IOptionsMonitor<AzureStorageOptions> options)
        {
            if (string.IsNullOrEmpty(options.CurrentValue.ConnectionString))
                throw new ArgumentNullException(nameof(options.CurrentValue.ConnectionString));

            if (containerRegex.IsMatch(options.CurrentValue.Container) == false)
                throw new FormatException("Not supported Azure container name. Check https://blogs.msdn.microsoft.com/jmstall/2014/06/12/azure-storage-naming-rules/");

            var storageAccount = CloudStorageAccount.Parse(options.CurrentValue.ConnectionString);
            if (storageAccount is null)
                throw new ArgumentNullException(nameof(storageAccount), "Unable to parse and get storage account using `CloudStorageAccount.Parse(connectionString)`");

            var blobClient = storageAccount.CreateCloudBlobClient();
            if (blobClient is null)
                throw new ArgumentNullException(nameof(blobClient), "Unable to get blobClient using `storageAccount.CreateCloudBlobClient()`");

            Container = blobClient.GetContainerReference(options.CurrentValue.Container);
            if (Container is null)
                throw new ArgumentNullException(nameof(Container), "Unable to get Container using `blobClient.GetContainerReference(containerName)`");

            bool created = Container.CreateIfNotExistsAsync().GetAwaiter().GetResult(); //TODO: Think about moving this out of the ctor

            UrlExpiration = new UrlExpiration(options.CurrentValue.UrlTtlInSeconds);
            CacheControlExpiration = new AzureCacheControlExpiration(options.CurrentValue.CacheControlExpiration);
        }

        public CloudBlobContainer Container { get; set; }

        public UrlExpiration UrlExpiration { get; set; }

        public AzureCacheControlExpiration CacheControlExpiration { get; set; }
    }
}
