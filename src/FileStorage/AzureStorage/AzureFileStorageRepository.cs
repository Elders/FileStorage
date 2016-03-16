using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.WindowsAzure.Storage.Blob;
using FileStorage.Extensions;
using FileStorage.FileFormats;

namespace FileStorage.AzureStorage
{
    public class AzureFileStorageRepository : IFileStorageRepository
    {
        readonly AzureStorageSettings storageSettings;
        readonly Dictionary<string, IFileFormat> formats;

        public AzureFileStorageRepository(AzureStorageSettings storageSettings)
        {
            if (ReferenceEquals(storageSettings, null) == true) throw new ArgumentNullException(nameof(storageSettings));
            this.storageSettings = storageSettings;

            ImageResizer.Configuration.Config.Current.UpgradeImageBuilder(new CustomImageBuilder());
            formats = new Dictionary<string, IFileFormat>();
        }

        public IFile Download(string fileName, string format = "original")
        {
            if (formats.ContainsKey(format) == false) throw new NotSupportedException($"This file format is not supported. {format}");
            if (string.IsNullOrWhiteSpace(fileName)) throw new ArgumentNullException(nameof(fileName));

            var formatInstance = formats[format];
            if (formatInstance.FindFile(fileName) == false)
                formatInstance.Generate(fileName);

            var key = GetKey(fileName, format);
            var blockBlob = storageSettings.Container.GetBlockBlobReference(key);

            using (var memoryStream = new MemoryStream())
            {
                blockBlob.DownloadToStream(memoryStream);
                memoryStream.Position = 0;

                return new LocalFile(memoryStream.ToByteArray(), fileName);
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

        public byte[] Generate(byte[] data, string format)
        {
            if (ReferenceEquals(data, null) == true) throw new ArgumentNullException(nameof(data));
            if (formats.ContainsKey(format) == false) throw new NotSupportedException($"This file format is not supported. {format}");

            var formatInstance = formats[format];
            var newData = formatInstance.Generate(data);

            return newData;
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

            blockBlob.UploadFromByteArrayAsync(data, 0, data.Length);

            foreach (var meta in metaInfo)
            {
                blockBlob.Metadata.Add(meta.Key, meta.Value);
            }

            blockBlob.SetMetadataAsync();
        }

        string GetSasContainerToken()
        {
            var sasConstraints = new SharedAccessBlobPolicy
            {
                SharedAccessExpiryTime = DateTime.UtcNow.AddSeconds(storageSettings.UrlExpiration),
                Permissions = SharedAccessBlobPermissions.Read
            };

            var sasContainerToken = storageSettings.Container.GetSharedAccessSignature(sasConstraints);
            return sasContainerToken;
        }

        string GetKey(string fileName, string format)
        {
            return format + "/" + fileName;
        }

        public void RegisterFormat(IFileFormat format)
        {
            formats.Add(format.Name, format);
        }
    }
}
