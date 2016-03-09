using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
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
        readonly IAmazonS3 client;
        readonly int urlExpiration;
        readonly Dictionary<string, IFileFormat> formats;
        readonly string bucketName;
        readonly Regex bucketRegex = new Regex(@"^(?!-)(?!.*--)(?!\.)(?!.*\.\.)[a-z0-9-.]{3,63}(?<!-)(?<!\.)$");
        readonly bool useCloudfront;

        //Cloud Front
        readonly string cloudfrontPrivateKeyPath;
        readonly string cloudfrontDomain;
        readonly string cloudfrontKeypairid;

        public S3FileStorageRepository(string accessKey, string secretKey, string bucketName, string region, int urlExpiration)
        {
            if (string.IsNullOrWhiteSpace(accessKey))
                throw new ArgumentNullException(nameof(accessKey));

            if (string.IsNullOrWhiteSpace(secretKey))
                throw new ArgumentNullException(nameof(secretKey));

            if (string.IsNullOrWhiteSpace(bucketName))
                throw new ArgumentNullException(nameof(bucketName));

            if (string.IsNullOrWhiteSpace(region))
                throw new ArgumentNullException(nameof(region));

            if (bucketRegex.IsMatch(bucketName) == false)
                throw new FormatException("Not supported Amazon bucket name. Check http://docs.aws.amazon.com/awscloudtrail/latest/userguide/cloudtrail-s3-bucket-naming-requirements.html");

            this.bucketName = bucketName;
            this.urlExpiration = urlExpiration;

            var creds = new BasicAWSCredentials(accessKey, secretKey);
            client = new AmazonS3Client(creds, RegionEndpoint.GetBySystemName(region));

            if (ReferenceEquals(client, null) == true)
                throw new ArgumentNullException(nameof(client));

            ImageResizer.Configuration.Config.Current.UpgradeImageBuilder(new CustomImageBuilder());

            formats = new Dictionary<string, IFileFormat>();
            RegisterFormat(new MobileFull(this));
            RegisterFormat(new MobileThumbnail(this));
            RegisterFormat(new Original(this));
        }

        public S3FileStorageRepository(string accessKey, string secretKey, string bucketName, string region, int urlExpiration,
            bool useCloudfront, string cloudfrontPrivateKeyPath, string cloudfrontDomain, string cloudfrontKeypairid)
            : this(accessKey, secretKey, bucketName, region, urlExpiration)
        {
            if (string.IsNullOrWhiteSpace(cloudfrontPrivateKeyPath))
                throw new ArgumentNullException(nameof(cloudfrontPrivateKeyPath));

            if (string.IsNullOrWhiteSpace(cloudfrontDomain))
                throw new ArgumentNullException(nameof(cloudfrontDomain));

            if (string.IsNullOrWhiteSpace(cloudfrontKeypairid))
                throw new ArgumentNullException(nameof(cloudfrontKeypairid));

            this.useCloudfront = useCloudfront;
            this.cloudfrontPrivateKeyPath = cloudfrontPrivateKeyPath;
            this.cloudfrontDomain = cloudfrontDomain;
            this.cloudfrontKeypairid = cloudfrontKeypairid;
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
                throw new NotSupportedException($"This file format is not supported. {format}");

            if (string.IsNullOrWhiteSpace(fileName))
                throw new ArgumentNullException(nameof(fileName));

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
            if (string.IsNullOrWhiteSpace(fileName))
                throw new ArgumentNullException(nameof(fileName));

            string urlString = string.Empty;

            if (useCloudfront == true)
            {
                try
                {
                    using (var textReader = File.OpenText(cloudfrontPrivateKeyPath))
                    {
                        urlString = AmazonCloudFrontUrlSigner.GetCannedSignedURL(
                            AmazonCloudFrontUrlSigner.Protocol.https,
                            cloudfrontDomain,
                            textReader,
                            format + "/" + fileName,
                            cloudfrontKeypairid,
                            DateTime.UtcNow.AddSeconds(urlExpiration));

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
                BucketName = bucketName,
                Key = format + "/" + fileName,
                Expires = DateTime.UtcNow.AddSeconds(urlExpiration)
            };

            urlString = client.GetPreSignedURL(request);
            return urlString;
        }

        public bool FileExists(string fileName, string format = "original")
        {
            if (string.IsNullOrWhiteSpace(fileName))
                throw new ArgumentNullException(nameof(fileName));

            var s3FileInfo = new Amazon.S3.IO.S3FileInfo(client, bucketName, format + "/" + fileName);
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
