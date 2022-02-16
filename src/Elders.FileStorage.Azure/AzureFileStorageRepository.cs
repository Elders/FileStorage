using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using FileStorage.Extensions;
using Microsoft.Extensions.Options;
using Microsoft.WindowsAzure.Storage.Blob;

namespace FileStorage.Azure
{
    public class AzureFileStorageRepository : IFileStorageRepository
    {
        private readonly AzureStorageClient azureStorageClient;
        private readonly IFileGenerator generator;
        private readonly AzureStorageOptions options;

        public AzureFileStorageRepository(IOptionsMonitor<AzureStorageOptions> optionsMonitor, AzureStorageClient azureStorageClient, IFileGenerator generator)
        {
            this.options = optionsMonitor.CurrentValue;
            this.azureStorageClient = azureStorageClient;
            this.generator = generator;
        }

        public async Task<IFile> DownloadAsync(string fileName, string format = "original")
        {
            if (string.IsNullOrWhiteSpace(fileName)) throw new ArgumentNullException(nameof(fileName));

            var key = GetKey(fileName, format);
            var blockBlob = azureStorageClient.Container.GetBlockBlobReference(key);

            using (var memoryStream = new MemoryStream())
            {
                await blockBlob.DownloadToStreamAsync(memoryStream).ConfigureAwait(false);
                memoryStream.Position = 0;

                return new LocalFile(memoryStream.ToByteArray(), fileName);
            }
        }

        public Task<bool> FileExistsAsync(string fileName, string format = "original")
        {
            if (string.IsNullOrWhiteSpace(fileName)) throw new ArgumentNullException(nameof(fileName));

            var key = GetKey(fileName, format);
            var blockBlob = azureStorageClient.Container.GetBlockBlobReference(key);
            return blockBlob.ExistsAsync();
        }

        public Task<string> GetFileUriAsync(string fileName, string format = "original")
        {
            if (string.IsNullOrWhiteSpace(fileName)) throw new ArgumentNullException(nameof(fileName));

            var key = GetKey(fileName, format);
            var blockBlob = azureStorageClient.Container.GetBlockBlobReference(key);
            var sharedAccessSignature = GetSasContainerToken();

            return Task.FromResult(options.CdnUrl + blockBlob.Uri.AbsolutePath + sharedAccessSignature);
        }

        public Task UploadAsync(string fileName, byte[] data, IEnumerable<FileMeta> metaInfo, string format = "original")
        {
            if (string.IsNullOrWhiteSpace(fileName)) throw new ArgumentNullException(nameof(fileName));
            if (ReferenceEquals(data, null) == true) throw new ArgumentNullException(nameof(data));
            if (ReferenceEquals(metaInfo, null) == true) throw new ArgumentNullException(nameof(metaInfo));

            var key = GetKey(fileName, format);
            var blockBlob = azureStorageClient.Container.GetBlockBlobReference(key);

            foreach (var meta in metaInfo)
            {
                //  https://github.com/Azure/azure-sdk-for-net/issues/178
                //  Important to note that using ascii encoding will replace all non-ascii characters with '?'(63)
                string encodedKey = Encoding.ASCII.GetString(Encoding.ASCII.GetBytes(meta.Key));
                string encodedValue = Encoding.ASCII.GetString(Encoding.ASCII.GetBytes(meta.Value));

                blockBlob.Metadata.Add(encodedKey, encodedValue);
            }

            blockBlob.Properties.ContentType = data.GetMimeType();
            blockBlob.Properties.CacheControl = azureStorageClient.CacheControlExpiration.CacheControlHeader;

            return blockBlob.UploadFromByteArrayAsync(data, 0, data.Length);
        }

        public Stream GetStream(string fileName, IEnumerable<FileMeta> metaInfo, string format = "original")
        {
            if (string.IsNullOrWhiteSpace(fileName)) throw new ArgumentNullException(nameof(fileName));
            if (ReferenceEquals(metaInfo, null) == true) throw new ArgumentNullException(nameof(metaInfo));
            var blockBlob = azureStorageClient.Container.GetBlockBlobReference(format + "/" + fileName);

            return new AzureFileStorageStream(blockBlob, metaInfo);
        }

        public Task DeleteAsync(string fileName)
        {
            List<Task> deleteTasks = new List<Task>();
            foreach (var format in generator.Formats)
            {
                var key = GetKey(fileName, format.Name);
                var blockBlob = azureStorageClient.Container.GetBlockBlobReference(key);
                Task deleteTask = blockBlob.DeleteIfExistsAsync();
                deleteTasks.Add(deleteTask);
            }

            return Task.WhenAll(deleteTasks.ToArray());
        }

        string GetSasContainerToken()
        {
            if (azureStorageClient.UrlExpiration.IsEnabled == false)
                return string.Empty;

            var sasConstraints = new SharedAccessBlobPolicy
            {
                SharedAccessExpiryTime = azureStorageClient.UrlExpiration.InDateTime,
                Permissions = SharedAccessBlobPermissions.Read
            };

            //var sasConstraints = new BlobSasBuilder
            //{
            //    ExpiresOn = azureStorageClient.UrlExpiration.InDateTime,
            //};
            //sasConstraints.SetPermissions(BlobAccountSasPermissions.Read);

            var sasContainerToken = azureStorageClient.Container.GetSharedAccessSignature(sasConstraints);
            return sasContainerToken;
        }

        string GetKey(string fileName, string format)
        {
            return format + "/" + fileName;
        }
    }
}
