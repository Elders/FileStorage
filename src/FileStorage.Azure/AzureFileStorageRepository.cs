﻿using System;
using System.Collections.Generic;
using System.IO;
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

        public IFile Download(string fileName, string format = "original")
        {
            if (string.IsNullOrWhiteSpace(fileName)) throw new ArgumentNullException(nameof(fileName));

            var key = GetKey(fileName, format);
            var blockBlob = storageSettings.Container.GetBlockBlobReference(key);

            try
            {
                using (var memoryStream = new MemoryStream())
                {
                    blockBlob.DownloadToStream(memoryStream);
                    memoryStream.Position = 0;

                    return new LocalFile(memoryStream.ToByteArray(), fileName);
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public bool FileExists(string fileName, string format = "original")
        {
            if (string.IsNullOrWhiteSpace(fileName)) throw new ArgumentNullException(nameof(fileName));

            var key = GetKey(fileName, format);
            var blockBlob = storageSettings.Container.GetBlockBlobReference(key);
            var result = blockBlob.Exists();

            return result;
        }

        public string GetFileUri(string fileName, string format = "original")
        {
            if (string.IsNullOrWhiteSpace(fileName)) throw new ArgumentNullException(nameof(fileName));

            var key = GetKey(fileName, format);
            var blockBlob = storageSettings.Container.GetBlockBlobReference(key);
            var sharedAccessSignature = GetSasContainerToken();

            if (string.IsNullOrWhiteSpace(storageSettings.CdnUrl) == false)
                return storageSettings.CdnUrl + blockBlob.Uri.AbsolutePath + sharedAccessSignature;

            return blockBlob.Uri + sharedAccessSignature;
        }

        public void Upload(string fileName, byte[] data, IEnumerable<FileMeta> metaInfo, string format = "original")
        {
            if (string.IsNullOrWhiteSpace(fileName)) throw new ArgumentNullException(nameof(fileName));
            if (ReferenceEquals(data, null) == true) throw new ArgumentNullException(nameof(data));
            if (ReferenceEquals(metaInfo, null) == true) throw new ArgumentNullException(nameof(metaInfo));

            var key = GetKey(fileName, format);
            var blockBlob = storageSettings.Container.GetBlockBlobReference(key);

            foreach (var meta in metaInfo)
            {
                // The key must comply with the identifier guidelines
                if (System.CodeDom.Compiler.CodeGenerator.IsValidLanguageIndependentIdentifier(meta.Key))
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

            blockBlob.UploadFromByteArrayAsync(data, 0, data.Length);
        }

        public Stream GetStream(string fileName, IEnumerable<FileMeta> metaInfo, string format = "original")
        {
            if (string.IsNullOrWhiteSpace(fileName)) throw new ArgumentNullException(nameof(fileName));
            if (ReferenceEquals(metaInfo, null) == true) throw new ArgumentNullException(nameof(metaInfo));

            return new AzureFileStorageStream(storageSettings, fileName, metaInfo, format);
        }

        public void Delete(string fileName)
        {
            foreach (var format in storageSettings.Generator.Formats)
            {
                var key = GetKey(fileName, format.Name);
                var blockBlob = storageSettings.Container.GetBlockBlobReference(key);
                blockBlob.DeleteIfExistsAsync();
            }
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
