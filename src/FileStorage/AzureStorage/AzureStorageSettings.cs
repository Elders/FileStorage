using System;
using System.Text.RegularExpressions;
using ImageResizer;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;

namespace FileStorage.AzureStorage
{
    public class AzureStorageSettings
    {
        public CloudBlobContainer Container { get; private set; }
        public int UrlExpiration { get; set; }
        public string CdnUrl { get; set; }
        public ImageBuilder ImageBuilder { get; set; }

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
        }
    }
}