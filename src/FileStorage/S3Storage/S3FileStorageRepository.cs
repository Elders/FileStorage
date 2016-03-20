using System;
using System.Collections.Generic;
using System.IO;
using Amazon.CloudFront;
using Amazon.S3.Model;
using FileStorage.Extensions;
using FileStorage.FileFormats;

namespace FileStorage.S3Storage
{
    //public class StorageSettings
    //{ }

    //public class GG
    //{
    //    public GG()
    //    {
    //        var repository = new StorageSettings().UseS3Storage(x =>
    //        {
    //            return x.
    //        });
    //    }
    //}

    //public static class SettingsExtentions
    //{
    //    //public static S3FileStorageRepository UseS3Storage(this StorageSettings self, Func<StorageSettings, S3FileStorageSettings> settings)
    //    //{
    //    //    return new S3FileStorageRepository(settings());
    //    //}

    //    //public static S3FileStorageSettings UseS3Bucket(this StorageSettings self, string accessKey, string secretKey, string bucketName, string region, int urlExpiration)
    //    //{
    //    //    return new S3FileStorageSettings(accessKey, secretKey, bucketName, region, urlExpiration);
    //    //}

    //    public static S3FileStorageSettings UseCloudFront(this S3FileStorageSettings self, string cloudfrontPrivateKeyPath, string cloudfrontDomain, string cloudfrontKeypairid)
    //    {
    //        self.Cdn = new CloudFrontSettings(cloudfrontPrivateKeyPath, cloudfrontDomain, cloudfrontKeypairid);

    //        return self;
    //    }
    //}

    public class S3FileStorageRepository : IFileStorageRepository
    {
        readonly S3FileStorageSettings storageSettings;
        CloudFrontSettings cloudFrontSettings;
        readonly Dictionary<string, IFileFormat> formats;

        public S3FileStorageRepository(S3FileStorageSettings storageSettings)
        {
            if (ReferenceEquals(storageSettings, null) == true) throw new ArgumentNullException(nameof(storageSettings));

            this.storageSettings = storageSettings;
            formats = new Dictionary<string, IFileFormat>();
        }

        public void Upload(string fileName, byte[] data, IEnumerable<FileMeta> metaInfo, string format = "original")
        {
            if (string.IsNullOrWhiteSpace(fileName)) throw new ArgumentNullException(nameof(fileName));
            if (ReferenceEquals(data, null) == true) throw new ArgumentNullException(nameof(data));
            if (ReferenceEquals(metaInfo, null) == true) throw new ArgumentNullException(nameof(metaInfo));

            var metaData = new MetadataCollection();
            var contentType = MimeTypes.GetMimeType(new FileInfo(fileName).Extension);

            var uploadRequest = new PutObjectRequest
            {
                InputStream = new MemoryStream(data),
                BucketName = storageSettings.BucketName,
                Key = format + "/" + fileName,
                ContentType = contentType
            };

            foreach (var meta in metaInfo)
            {
                uploadRequest.Metadata.Add(meta.Key, meta.Value);
            }

            storageSettings.Client.PutObjectAsync(uploadRequest);
        }

        public IFile Download(string fileName, string format = "original")
        {
            if (formats.ContainsKey(format) == false) throw new NotSupportedException($"This file format is not supported. {format}");
            if (string.IsNullOrWhiteSpace(fileName)) throw new ArgumentNullException(nameof(fileName));

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
            if (string.IsNullOrWhiteSpace(fileName)) throw new ArgumentNullException(nameof(fileName));

            string urlString = string.Empty;

            if (ReferenceEquals(cloudFrontSettings, null) == false)
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
            else
            {
                var request = new GetPreSignedUrlRequest
                {
                    BucketName = storageSettings.BucketName,
                    Key = format + "/" + fileName,
                    Expires = DateTime.UtcNow.AddSeconds(storageSettings.UrlExpiration)
                };

                urlString = storageSettings.Client.GetPreSignedURL(request);
                return urlString;
            }

        }

        public bool FileExists(string fileName, string format = "original")
        {
            if (string.IsNullOrWhiteSpace(fileName)) throw new ArgumentNullException(nameof(fileName));

            var s3FileInfo = new Amazon.S3.IO.S3FileInfo(storageSettings.Client, storageSettings.BucketName, format + "/" + fileName);
            if (s3FileInfo.Exists)
                return true;

            return false;
        }

        public byte[] Generate(byte[] data, string format)
        {
            if (ReferenceEquals(data, null) == true) throw new ArgumentNullException(nameof(data));
            if (formats.ContainsKey(format) == false) throw new NotSupportedException(string.Format("This file format is not supported. {0}", format));

            var formatInstance = formats[format];
            var newData = formatInstance.Generate(data);

            return newData;
        }

        public void RegisterFormat(IFileFormat format)
        {
            formats.Add(format.Name, format);
        }

        public void UseCloudFront(CloudFrontSettings cloudFrontSettings)
        {
            if (ReferenceEquals(cloudFrontSettings, null) == true) throw new ArgumentNullException(nameof(cloudFrontSettings));
            this.cloudFrontSettings = cloudFrontSettings;
        }
    }
}
