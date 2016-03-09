using System;
using System.Collections.Generic;
using System.IO;
using Amazon.CloudFront;
using Amazon.S3.Model;
using FileStorage.Extensions;
using FileStorage.FileFormats;

namespace FileStorage.S3Storage
{
    public class S3FileStorageRepository : IFileStorageRepository
    {
        readonly S3FileStorageSettings storageSettings;
        readonly CloudFrontSettings cloudFrontSettings;
        readonly Dictionary<string, IFileFormat> formats;
        readonly bool useCloudfront;

        public S3FileStorageRepository(S3FileStorageSettings storageSettings)
        {
            if (ReferenceEquals(storageSettings, null) == true)
                throw new ArgumentNullException(nameof(storageSettings));

            this.storageSettings = storageSettings;

            ImageResizer.Configuration.Config.Current.UpgradeImageBuilder(new CustomImageBuilder());

            formats = new Dictionary<string, IFileFormat>();
            RegisterFormat(new MobileFull(this));
            RegisterFormat(new MobileThumbnail(this));
            RegisterFormat(new Original(this));
        }

        public S3FileStorageRepository(S3FileStorageSettings storageSettings, CloudFrontSettings cloudFrontSettings)
            : this(storageSettings)
        {
            if (ReferenceEquals(cloudFrontSettings, null) == true)
                throw new ArgumentNullException(nameof(cloudFrontSettings));

            this.cloudFrontSettings = cloudFrontSettings;
            useCloudfront = true;
        }

        public void Upload(string fileName, byte[] data, List<FileMeta> metaInfo, string format = "original")
        {
            if (string.IsNullOrWhiteSpace(fileName))
                throw new ArgumentNullException(nameof(fileName));

            if (ReferenceEquals(data, null) == true)
                throw new ArgumentNullException(nameof(data));

            if (ReferenceEquals(metaInfo, null) == true)
                throw new ArgumentNullException(nameof(metaInfo));

            var metaData = new MetadataCollection();

            var uploadRequest = new PutObjectRequest
            {
                InputStream = new MemoryStream(data),
                BucketName = storageSettings.BucketName,
                Key = format + "/" + fileName
            };

            foreach (var meta in metaInfo)
            {
                uploadRequest.Metadata.Add(meta.Key, meta.Value);
            }

            storageSettings.Client.PutObjectAsync(uploadRequest);
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

            GetObjectRequest request = new GetObjectRequest
            {
                BucketName = storageSettings.BucketName,
                Key = format + "/" + fileName,
            };

            var response = storageSettings.Client.GetObject(request);

            return new LocalFile(response.ResponseStream.ToByteArray(), fileName);
        }

        public string GetFileUri(string fileName, string format = "original")
        {
            if (string.IsNullOrWhiteSpace(fileName))
                throw new ArgumentNullException(nameof(fileName));

            string urlString = string.Empty;

            if (useCloudfront == true)
            {
                try
                {
                    using (var textReader = File.OpenText(cloudFrontSettings.CloudfrontPrivateKeyPath))
                    {
                        urlString = AmazonCloudFrontUrlSigner.GetCannedSignedURL(
                            AmazonCloudFrontUrlSigner.Protocol.https,
                            cloudFrontSettings.CloudfrontDomain,
                            textReader,
                            format + "/" + fileName,
                            cloudFrontSettings.CloudfrontKeypairid,
                            DateTime.UtcNow.AddSeconds(storageSettings.UrlExpiration));

                        return urlString;
                    }
                }
                catch (Exception ex)
                {
                    //log.Error("Unable to get files from Amazon S3.", ex);
                    return string.Empty;
                }
            }

            GetPreSignedUrlRequest request = new GetPreSignedUrlRequest
            {
                BucketName = storageSettings.BucketName,
                Key = format + "/" + fileName,
                Expires = DateTime.UtcNow.AddSeconds(storageSettings.UrlExpiration)
            };

            urlString = storageSettings.Client.GetPreSignedURL(request);
            return urlString;
        }

        public bool FileExists(string fileName, string format = "original")
        {
            if (string.IsNullOrWhiteSpace(fileName))
                throw new ArgumentNullException(nameof(fileName));

            var s3FileInfo = new Amazon.S3.IO.S3FileInfo(storageSettings.Client, storageSettings.BucketName, format + "/" + fileName);
            if (s3FileInfo.Exists)
                return true;

            return false;
        }

        public byte[] Generate(byte[] data, string format)
        {
            if (ReferenceEquals(data, null) == true)
                throw new ArgumentNullException(nameof(data));

            if (formats.ContainsKey(format) == false)
                throw new NotSupportedException(string.Format("This file format is not supported. {0}", format));

            var formatInstance = formats[format];
            var newData = formatInstance.Generate(data);

            return newData;
        }

        private void RegisterFormat(IFileFormat format)
        {
            formats.Add(format.Name, format);
        }
    }
}
