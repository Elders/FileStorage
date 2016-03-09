using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using FileStorage.Extensions;
using FileStorage.FileFormats;

namespace FileStorage
{
    public class AzureFileStorageRepository : IFileStorageRepository
    {
        readonly CloudStorageAccount storageAccount;
        readonly CloudBlobClient blobClient;
        readonly CloudBlobContainer container;

        readonly Dictionary<string, IFileFormat> formats;
        readonly int urlExpirationInSeconds;
        readonly Regex containerRegex = new Regex("^(?!-)(?!.*--)[a-z0-9-]{3,63}(?<!-)$");

        public AzureFileStorageRepository(string connectionString, string containerName, int urlExpirationInSeconds)
        {
            if (string.IsNullOrWhiteSpace(connectionString))
                throw new ArgumentNullException(nameof(connectionString));

            if (string.IsNullOrWhiteSpace(containerName))
                throw new ArgumentNullException(nameof(containerName));

            if (containerRegex.IsMatch(containerName) == false)
                throw new FormatException("Not supported Azure container name. Check https://blogs.msdn.microsoft.com/jmstall/2014/06/12/azure-storage-naming-rules/");

            storageAccount = CloudStorageAccount.Parse(connectionString);
            if (ReferenceEquals(storageAccount, null) == true)
                throw new ArgumentNullException(nameof(storageAccount));

            blobClient = storageAccount.CreateCloudBlobClient();
            if (ReferenceEquals(blobClient, null) == true)
                throw new ArgumentNullException(nameof(blobClient));

            container = blobClient.GetContainerReference(containerName);
            if (ReferenceEquals(container, null) == true)
                throw new ArgumentNullException(nameof(container));

            container.CreateIfNotExists();

            this.urlExpirationInSeconds = urlExpirationInSeconds;

            ImageResizer.Configuration.Config.Current.UpgradeImageBuilder(new CustomImageBuilder());

            formats = new Dictionary<string, IFileFormat>();
            RegisterFormat(new MobileFull(this));
            RegisterFormat(new MobileThumbnail(this));
            RegisterFormat(new Original(this));
        }

        public LocalFile Download(string fileName, string format = "original")
        {
            if (formats.ContainsKey(format) == false)
                throw new NotSupportedException($"This file format is not supported. {format}");

            if (string.IsNullOrWhiteSpace(fileName))
                throw new ArgumentNullException(nameof(fileName));

            var formatInstance = formats[format];

            if (formatInstance.FindFile(fileName) == false)
                formatInstance.Generate(fileName);

            var key = GetKey(fileName, format);
            var blockBlob = container.GetBlockBlobReference(key);

            using (var memoryStream = new MemoryStream())
            {
                blockBlob.DownloadToStream(memoryStream);
                memoryStream.Position = 0;

                return new LocalFile(memoryStream.ToByteArray(), fileName);
            }
        }

        public bool FileExists(string fileName, string format = "original")
        {
            if (string.IsNullOrWhiteSpace(fileName))
                throw new ArgumentNullException(nameof(fileName));

            var key = GetKey(fileName, format);
            var blockBlob = container.GetBlockBlobReference(key);
            var result = blockBlob.Exists();

            return result;
        }

        public byte[] Generate(byte[] data, string format)
        {
            if (ReferenceEquals(data, null) == true)
                throw new ArgumentNullException(nameof(data));

            if (formats.ContainsKey(format) == false)
                throw new NotSupportedException($"This file format is not supported. {format}");

            var formatInstance = formats[format];
            var newData = formatInstance.Generate(data);

            return newData;
        }

        public string GetFileUri(string fileName, string format = "original")
        {
            if (string.IsNullOrWhiteSpace(fileName))
                throw new ArgumentNullException(nameof(fileName));

            var key = GetKey(fileName, format);
            var blockBlob = container.GetBlockBlobReference(key);
            var sas = GetSasContainerToken();

            return blockBlob.Uri + sas;
        }

        public void Upload(string fileName, byte[] data, List<FileMeta> metaInfo, string format = "original")
        {
            if (string.IsNullOrWhiteSpace(fileName))
                throw new ArgumentNullException(nameof(fileName));

            if (ReferenceEquals(data, null) == true)
                throw new ArgumentNullException(nameof(data));

            if (ReferenceEquals(metaInfo, null) == true)
                throw new ArgumentNullException(nameof(metaInfo));

            var key = GetKey(fileName, format);
            var blockBlob = container.GetBlockBlobReference(key);

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
                SharedAccessExpiryTime = DateTime.UtcNow.AddSeconds(urlExpirationInSeconds),
                Permissions = SharedAccessBlobPermissions.Read
            };

            var sasContainerToken = container.GetSharedAccessSignature(sasConstraints);
            return sasContainerToken;
        }

        string GetKey(string fileName, string format)
        {
            return format + "/" + fileName;
        }

        void RegisterFormat(IFileFormat format)
        {
            formats.Add(format.Name, format);
        }
    }
}
