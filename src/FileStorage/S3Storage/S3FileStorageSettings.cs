using System;
using System.Text.RegularExpressions;
using Amazon;
using Amazon.Runtime;
using Amazon.S3;
using FileStorage.Generators;

namespace FileStorage.S3Storage
{
    public class S3FileStorageSettings : IFileStorageSettings<S3FileStorageSettings>
    {
        // AmazonS3 thread safe https://forums.aws.amazon.com/thread.jspa?threadID=78026&tstart=0
        public IAmazonS3 Client { get; private set; }
        public string BucketName { get; private set; }
        public UrlExpiration UrlExpiration { get; private set; }
        public IFileGenerator Generator { get; private set; }
        public bool IsGenerationEnabled { get { return ReferenceEquals(Generator, null) == false; } }

        readonly Regex bucketRegex = new Regex(@"^(?!-)(?!.*--)(?!\.)(?!.*\.\.)[a-z0-9-.]{3,63}(?<!-)(?<!\.)$");

        public S3FileStorageSettings(string accessKey, string secretKey, string region, string bucketName)
        {
            if (string.IsNullOrWhiteSpace(accessKey)) throw new ArgumentNullException(nameof(accessKey));
            if (string.IsNullOrWhiteSpace(secretKey)) throw new ArgumentNullException(nameof(secretKey));
            if (string.IsNullOrWhiteSpace(region)) throw new ArgumentNullException(nameof(region));
            if (bucketRegex.IsMatch(bucketName) == false) throw new FormatException("Not supported Amazon bucket name. Check http://docs.aws.amazon.com/awscloudtrail/latest/userguide/cloudtrail-s3-bucket-naming-requirements.html");

            BucketName = bucketName;

            var creds = new BasicAWSCredentials(accessKey, secretKey);
            Client = new AmazonS3Client(creds, RegionEndpoint.GetBySystemName(region));

            if (ReferenceEquals(Client, null) == true) throw new ArgumentNullException(nameof(Client));

            UseUrlExpiration(new UrlExpiration());
        }

        public S3FileStorageSettings UseUrlExpiration(UrlExpiration expiration)
        {
            if (ReferenceEquals(expiration, null) == true) throw new ArgumentNullException(nameof(expiration));
            UrlExpiration = expiration;
            return this;
        }

        public S3FileStorageSettings UseFileGenerator(IFileGenerator generator)
        {
            if (ReferenceEquals(generator, null) == true) throw new ArgumentNullException(nameof(generator));
            Generator = generator;
            return this;
        }
    }
}