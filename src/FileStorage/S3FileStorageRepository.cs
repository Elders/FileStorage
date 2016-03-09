using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using Amazon;
using Amazon.CloudFront;
using Amazon.Runtime;
using Amazon.S3;
using Amazon.S3.Model;
using FileStorage.Extensions;
using FileStorage.FileFormats;

namespace FileStorage
{
    public class S3FileStorageRepository : IFileStorageRepository
    {
        private int urlExpiration;
        private Dictionary<string, IFileFormat> formats;
        private IAmazonS3 client = null;
        private string bucketName;

        public S3FileStorageRepository(string accessKey, string secretKey, string bucketName, int urlExpiration, string region)
        {
            this.bucketName = bucketName;
            this.urlExpiration = urlExpiration;

            var creds = new BasicAWSCredentials(accessKey, secretKey);
            this.client = new AmazonS3Client(creds, RegionEndpoint.GetBySystemName(region));

            ImageResizer.Configuration.Config.Current.UpgradeImageBuilder(new CustomImageBuilder());

            formats = new Dictionary<string, IFileFormat>();
            RegisterFormat(new MobileFull(this));
            RegisterFormat(new MobileThumbnail(this));
            RegisterFormat(new Original(this));
        }

        public void Upload(string fileName, byte[] data, List<FileMeta> metaInfo, string format = "original")
        {
            var metaData = new MetadataCollection();

            var uploadRequest = new PutObjectRequest
            {
                InputStream = new MemoryStream(data),
                BucketName = bucketName,
                Key = format + "/" + fileName
            };

            foreach (var meta in metaInfo)
            {
                uploadRequest.Metadata.Add(meta.Key, meta.Value);
            }

            client.PutObjectAsync(uploadRequest);
        }

        public LocalFile Download(string fileName, string format = "original")
        {
            if (formats.ContainsKey(format) == false)
                throw new NotSupportedException(string.Format("This file format is not supported. {0}", format));

            var formatInstance = formats[format];

            if (formatInstance.FindFile(fileName) == false)
                formatInstance.Generate(fileName);

            GetObjectRequest request = new GetObjectRequest
            {
                BucketName = bucketName,
                Key = format + "/" + fileName,
            };

            var response = this.client.GetObject(request);

            return new LocalFile(response.ResponseStream.ToByteArray(), fileName);
        }

        public string GetFileUri(string fileName, string format = "original")
        {
            try
            {
                using (var textReader = File.OpenText(ConfigurationManager.AppSettings.Get("amazon_cloudfront_privatekey")))
                {
                    var url = AmazonCloudFrontUrlSigner.GetCannedSignedURL(
                     AmazonCloudFrontUrlSigner.Protocol.https,
                     ConfigurationManager.AppSettings.Get("amazon_cloudfront_domain"),
                     textReader,
                     format + "/" + fileName,
                     ConfigurationManager.AppSettings.Get("amazon_cloudfront_keypairid"),
                     DateTime.UtcNow.AddSeconds(urlExpiration));

                    return url;
                }
            }
            catch (Exception ex)
            {
                //log.Error("Unable to get files from Amazon S3.", ex);
                return string.Empty;
            }

            //string urlString = string.Empty;
            //GetPreSignedUrlRequest request = new GetPreSignedUrlRequest
            //{
            //    BucketName = bucketName,
            //    Key = format + "/" + fileName,
            //    Expires = DateTime.UtcNow.AddSeconds(urlExpiration)
            //};

            //urlString = this.client.GetPreSignedURL(request);
            //return urlString;
        }

        public bool FileExists(string fileName, string format = "original")
        {
            var s3FileInfo = new Amazon.S3.IO.S3FileInfo(client, bucketName, format + "/" + fileName);
            if (s3FileInfo.Exists)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public byte[] Generate(byte[] data, string format)
        {
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
