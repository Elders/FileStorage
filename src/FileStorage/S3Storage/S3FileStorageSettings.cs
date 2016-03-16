using System;
using System.Text.RegularExpressions;
using Amazon;
using Amazon.Runtime;
using Amazon.S3;

namespace FileStorage.S3Storage
{
    public class S3FileStorageSettings
    {
        // AmazonS3 thread safe https://forums.aws.amazon.com/thread.jspa?threadID=78026&tstart=0
        public IAmazonS3 Client { get; private set; }
        public string BucketName { get; private set; }
        public int UrlExpiration { get; set; }

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
        }
    }
}