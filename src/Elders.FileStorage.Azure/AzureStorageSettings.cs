using System;
using System.Text.RegularExpressions;
using FileStorage.MimeTypes;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;

namespace FileStorage.Azure
{
    public class AzureStorageSettings : IFileStorageSettings<AzureStorageSettings>
    {
        public CloudBlobContainer Container { get; private set; }
        public UrlExpiration UrlExpiration { get; set; }
        public AzureCacheControlExpiration CacheControlExpiration { get; set; }
        public string CdnUrl { get; private set; }
        public IFileGenerator Generator { get; private set; }
        public bool IsGenerationEnabled { get { return ReferenceEquals(Generator, null) == false; } }
        public IMimeTypeResolver MimeTypeResolver { get; private set; }
        public bool IsMimeTypeResolverEnabled { get { return ReferenceEquals(MimeTypeResolver, null) == false; } }
        public int BlockSizeInKB { get; private set; }

        readonly Regex containerRegex = new Regex("^(?!-)(?!.*--)[a-z0-9-]{3,63}(?<!-)$");

        int maxBlockSize = 4000;

        public AzureStorageSettings(string connectionString, string containerName, int blockSizeInKB)
        {
            if (string.IsNullOrWhiteSpace(connectionString)) throw new ArgumentNullException(nameof(connectionString));
            if (containerRegex.IsMatch(containerName) == false) throw new FormatException("Not supported Azure container name. Check https://blogs.msdn.microsoft.com/jmstall/2014/06/12/azure-storage-naming-rules/");
            var storageAccount = CloudStorageAccount.Parse(connectionString);

            if (ReferenceEquals(storageAccount, null) == true) throw new ArgumentNullException(nameof(storageAccount));
            var blobClient = storageAccount.CreateCloudBlobClient();

            if (ReferenceEquals(blobClient, null) == true) throw new ArgumentNullException(nameof(blobClient));
            Container = blobClient.GetContainerReference(containerName);

            if (ReferenceEquals(Container, null) == true) throw new ArgumentNullException(nameof(Container));
            bool created = Container.CreateIfNotExistsAsync().Result;

            if (blockSizeInKB > maxBlockSize) throw new ArgumentException("Block size can not be more than 4mb");
            BlockSizeInKB = blockSizeInKB;

            UseUrlExpiration(new UrlExpiration());
            UseCacheControlExpiration(new AzureCacheControlExpiration());
        }


        /// <summary>
        /// Not working for URLs with CDN
        /// </summary>
        /// <param name="expiration"></param>
        /// <returns></returns>
        public AzureStorageSettings UseUrlExpiration(UrlExpiration expiration)
        {
            if (ReferenceEquals(expiration, null) == true) throw new ArgumentNullException(nameof(expiration));
            UrlExpiration = expiration;
            return this;
        }

        public AzureStorageSettings UseCacheControlExpiration(AzureCacheControlExpiration expiration)
        {
            if (ReferenceEquals(expiration, null) == true) throw new ArgumentNullException(nameof(expiration));
            CacheControlExpiration = expiration;
            return this;
        }

        public AzureStorageSettings UseFileGenerator(IFileGenerator generator)
        {
            if (ReferenceEquals(generator, null) == true) throw new ArgumentNullException(nameof(generator));
            Generator = generator;
            return this;
        }

        public AzureStorageSettings UseCdn(string cdnUrl)
        {
            CdnUrl = cdnUrl;
            return this;
        }

        public AzureStorageSettings UseMimeTypeResolver(IMimeTypeResolver resolver)
        {
            if (ReferenceEquals(resolver, null) == true) throw new ArgumentNullException(nameof(resolver));
            MimeTypeResolver = resolver;
            return this;
        }
    }
}
