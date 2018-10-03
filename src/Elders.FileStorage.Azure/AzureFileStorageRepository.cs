using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using FileStorage.Extensions;
using Microsoft.WindowsAzure.Storage.Blob;

namespace FileStorage.Azure
{
    public class AzureFileStorageRepository : IFileStorageRepository
    {
        readonly AzureStorageSettings storageSettings;

        public AzureFileStorageRepository(AzureStorageSettings storageSettings)
        {
            if (ReferenceEquals(storageSettings, null) == true) throw new ArgumentNullException(nameof(storageSettings));
            this.storageSettings = storageSettings;
        }

        public async Task<IFile> DownloadAsync(string fileName, string format = "original")
        {
            if (string.IsNullOrWhiteSpace(fileName)) throw new ArgumentNullException(nameof(fileName));

            var key = GetKey(fileName, format);
            var blockBlob = storageSettings.Container.GetBlockBlobReference(key);

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
            var blockBlob = storageSettings.Container.GetBlockBlobReference(key);
            return blockBlob.ExistsAsync();
        }

        public Task<string> GetFileUriAsync(string fileName, string format = "original")
        {
            if (string.IsNullOrWhiteSpace(fileName)) throw new ArgumentNullException(nameof(fileName));

            var key = GetKey(fileName, format);
            var blockBlob = storageSettings.Container.GetBlockBlobReference(key);
            var sharedAccessSignature = GetSasContainerToken();

            if (string.IsNullOrWhiteSpace(storageSettings.CdnUrl) == false)
                return Task.FromResult(storageSettings.CdnUrl + blockBlob.Uri.AbsolutePath + sharedAccessSignature);

            return Task.FromResult(blockBlob.Uri + sharedAccessSignature);
        }

        public Task UploadAsync(string fileName, byte[] data, IEnumerable<FileMeta> metaInfo, string format = "original")
        {
            if (string.IsNullOrWhiteSpace(fileName)) throw new ArgumentNullException(nameof(fileName));
            if (ReferenceEquals(data, null) == true) throw new ArgumentNullException(nameof(data));
            if (ReferenceEquals(metaInfo, null) == true) throw new ArgumentNullException(nameof(metaInfo));

            var key = GetKey(fileName, format);
            var blockBlob = storageSettings.Container.GetBlockBlobReference(key);

            foreach (var meta in metaInfo)
            {
                // The key must comply with the identifier guidelines
                //if (System.CodeDom.Compiler.CodeGenerator.IsValidLanguageIndependentIdentifier(meta.Key))
                {
                    // The supported characters in the blob metadata must be ASCII characters.
                    // https://github.com/Azure/azure-sdk-for-net/issues/178
                    blockBlob.Metadata.Add(Uri.EscapeUriString(meta.Key), Uri.EscapeUriString(meta.Value));
                }
            }

            if (storageSettings.IsMimeTypeResolverEnabled)
            {
                var contentType = storageSettings.MimeTypeResolver.GetMimeType(data);
                blockBlob.Properties.ContentType = contentType;
            }

            blockBlob.Properties.CacheControl = storageSettings.CacheControlExpiration.CacheControlHeader;

            return blockBlob.UploadFromByteArrayAsync(data, 0, data.Length);
        }

        public Task<Stream> GetStreamAsync(string fileName, IEnumerable<FileMeta> metaInfo, string format = "original")
        {
            if (string.IsNullOrWhiteSpace(fileName)) throw new ArgumentNullException(nameof(fileName));
            if (ReferenceEquals(metaInfo, null) == true) throw new ArgumentNullException(nameof(metaInfo));

            return Task.FromResult<Stream>(new AzureFileStorageStream(storageSettings, fileName, metaInfo, format));
        }

        public Task DeleteAsync(string fileName)
        {
            List<Task> deleteTasks = new List<Task>();
            foreach (var format in storageSettings.Generator.Formats)
            {
                var key = GetKey(fileName, format.Name);
                var blockBlob = storageSettings.Container.GetBlockBlobReference(key);
                Task deleteTask = blockBlob.DeleteIfExistsAsync();
                deleteTasks.Add(deleteTask);
            }

            return Task.WhenAll(deleteTasks.ToArray());
        }

        string GetSasContainerToken()
        {
            var sasConstraints = new SharedAccessBlobPolicy
            {
                SharedAccessExpiryTime = storageSettings.UrlExpiration.InDateTime,
                Permissions = SharedAccessBlobPermissions.Read
            };

            var sasContainerToken = storageSettings.Container.GetSharedAccessSignature(sasConstraints);
            return sasContainerToken;
        }

        string GetKey(string fileName, string format)
        {
            return format + "/" + fileName;
        }
    }
}
