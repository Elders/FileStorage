using System;
using System.Text.RegularExpressions;
using FileGenerator;
using FileStorage.MimeTypes;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;

namespace FileStorage.AzureStorage
{
    public class AzureStorageSettings : IFileStorageSettings<AzureStorageSettings>
    {
        public CloudBlobContainer Container { get; private set; }
        public UrlExpiration UrlExpiration { get; set; }
        public string CdnUrl { get; private set; }
        public IFileGenerator Generator { get; private set; }
        public bool IsGenerationEnabled { get { return ReferenceEquals(Generator, null) == false; } }
        public IMimeTypeResolver MimeTypeResolver { get; private set; }
        public bool IsMimeTypeResolverEnabled { get { return ReferenceEquals(MimeTypeResolver, null) == false; } }

        readonly Regex containerRegex = new Regex("^(?!-)(?!.*--)[a-z0-9-]{3,63}(?<!-)$");

        public AzureStorageSettings(string connectionString, string containerName)
        {
            if (string.IsNullOrWhiteSpace(connectionString)) throw new ArgumentNullException(nameof(connectionString));
            if (containerRegex.IsMatch(containerName) == false) throw new FormatException("Not supported Azure container name. Check https://blogs.msdn.microsoft.com/jmstall/2014/06/12/azure-storage-naming-rules/");
            var storageAccount = CloudStorageAccount.Parse(connectionString);

            if (ReferenceEquals(storageAccount, null) == true) throw new ArgumentNullException(nameof(storageAccount));
            var blobClient = storageAccount.CreateCloudBlobClient();

            if (ReferenceEquals(blobClient, null) == true) throw new ArgumentNullException(nameof(blobClient));
            Container = blobClient.GetContainerReference(containerName);

            if (ReferenceEquals(Container, null) == true) throw new ArgumentNullException(nameof(Container));
            Container.CreateIfNotExists();

            UseUrlExpiration(new UrlExpiration());
        }

        public AzureStorageSettings UseUrlExpiration(UrlExpiration expiration)
        {
            if (ReferenceEquals(expiration, null) == true) throw new ArgumentNullException(nameof(expiration));
            UrlExpiration = expiration;
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