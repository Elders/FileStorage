using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using Amazon.CloudFront;
using Amazon.S3;
using Amazon.S3.Model;
using FileStorage.Extensions;
using FileStorage.FileFormats;

namespace FileStorage.AmazonS3
{
    public class S3FileStorageRepository : IFileStorageRepository
    {
        readonly S3FileStorageSettings storageSettings;
        CloudFrontSettings cloudFrontSettings;
        public bool IsCloudFrontEnabled { get { return ReferenceEquals(cloudFrontSettings, null) == false; } }

        public S3FileStorageRepository(S3FileStorageSettings storageSettings)
        {
            if (ReferenceEquals(storageSettings, null) == true) throw new ArgumentNullException(nameof(storageSettings));
            this.storageSettings = storageSettings;
        }

        public void Upload(string fileName, byte[] data, IEnumerable<FileMeta> metaInfo, string format = "original")
        {
            if (string.IsNullOrWhiteSpace(fileName)) throw new ArgumentNullException(nameof(fileName));
            if (ReferenceEquals(data, null) == true) throw new ArgumentNullException(nameof(data));
            if (ReferenceEquals(metaInfo, null) == true) throw new ArgumentNullException(nameof(metaInfo));

            var uploadRequest = new PutObjectRequest
            {
                InputStream = new MemoryStream(data),
                BucketName = storageSettings.BucketName,
                Key = format + "/" + fileName
            };

            if (storageSettings.IsMimeTypeResolverEnabled)
            {
                uploadRequest.ContentType = storageSettings.MimeTypeResolver.GetMimeType(data);
            }

            foreach (var meta in metaInfo)
            {
                uploadRequest.Metadata.Add(Uri.EscapeUriString(meta.Key), Uri.EscapeUriString(meta.Value));
            }

            storageSettings.Client.PutObjectAsync(uploadRequest);
        }

        public IFile Download(string fileName, string format = "original")
        {
            if (string.IsNullOrWhiteSpace(fileName)) throw new ArgumentNullException(nameof(fileName));

            try
            {
                var request = new GetObjectRequest
                {
                    BucketName = storageSettings.BucketName,
                    Key = format + "/" + fileName
                };

                using (var response = storageSettings.Client.GetObject(request))
                {
                    return new LocalFile(response.ResponseStream.ToByteArray(), fileName);
                }

            }

            catch (AmazonS3Exception ex)
            {
                if (ex.StatusCode == HttpStatusCode.NotFound && format != Original.FormatName)
                {
                    if (storageSettings.IsGenerationEnabled == true)
                    {
                        var file = storageSettings.Generator.Generate(Download(fileName).Data, format);
                        return new LocalFile(file.Data, fileName);
                    }

                    throw new FileNotFoundException($"File not found. Plugin in {typeof(IFileGenerator)} to generate it.");
                }
                throw ex;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public string GetFileUri(string fileName, string format = "original")
        {
            if (string.IsNullOrWhiteSpace(fileName)) throw new ArgumentNullException(nameof(fileName));

            string urlString = string.Empty;

            if (IsCloudFrontEnabled)
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
                            storageSettings.UrlExpiration.InDateTime);

                        return urlString;
                    }
                }
                catch (Exception)
                {
                    return string.Empty;
                }
            }
            else
            {
                var request = new GetPreSignedUrlRequest
                {
                    BucketName = storageSettings.BucketName,
                    Key = format + "/" + fileName,
                    Expires = storageSettings.UrlExpiration.InDateTime
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

        public S3FileStorageRepository UseCloudFront(CloudFrontSettings cloudFrontSettings)
        {
            if (ReferenceEquals(cloudFrontSettings, null) == true) throw new ArgumentNullException(nameof(cloudFrontSettings));
            this.cloudFrontSettings = cloudFrontSettings;

            return this;
        }
    }
}
