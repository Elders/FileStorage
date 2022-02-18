using Microsoft.Extensions.Options;
using System.Text.RegularExpressions;
using Azure.Storage.Blobs;

namespace FileStorage.Azure
{
    public class AzureStorageClient
    {
        private readonly AzureStorageOptions _options;

        private readonly Regex containerRegex = new Regex("^(?!-)(?!.*--)[a-z0-9-]{3,63}(?<!-)$");

        public AzureStorageClient(IOptionsMonitor<AzureStorageOptions> options)
        {
            _options = options.CurrentValue;

            if (string.IsNullOrEmpty(_options.ConnectionString))
                throw new ArgumentNullException(nameof(_options.ConnectionString));

            if (containerRegex.IsMatch(_options.Container) == false)
                throw new FormatException("Not supported Azure container name. Check https://blogs.msdn.microsoft.com/jmstall/2014/06/12/azure-storage-naming-rules/"); // TODO: new version?

            BlobServiceClient blobServiceClient = new BlobServiceClient(_options.ConnectionString);
            var userMetaData = blobServiceClient.GetPropertiesAsync().GetAwaiter().GetResult();
            var response = userMetaData.GetRawResponse(); // TODO: do not do this in the ctor but in the repo
            if (response.Status != 200)
                throw new ArgumentNullException(nameof(blobServiceClient), "Unable to create blob service");
            var blobContainerClient = blobServiceClient.GetBlobContainerClient(_options.Container);
            if (blobContainerClient is null)
                throw new ArgumentNullException(nameof(blobContainerClient), "Unable to get blob container client");
            //TODO: CreateIfNotExistsAsync(PublicAccessType, IDictionary<String,String>, BlobContainerEncryptionScopeOptions, CancellationToken) in the repo
            blobContainerClient.CreateIfNotExistsAsync();
            BlobContainerClient = blobContainerClient;

            UrlExpiration = new UrlExpiration(_options.UrlTtlInSeconds);
            CacheControlExpiration = new AzureCacheControlExpiration(_options.CacheControlExpiration);
        }

        public BlobContainerClient BlobContainerClient { get; set; }

        public UrlExpiration UrlExpiration { get; set; }

        public AzureCacheControlExpiration CacheControlExpiration { get; set; }
    }
}
