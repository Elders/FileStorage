using System;

namespace FileStorage.S3Storage
{
    public class CloudFrontSettings
    {
        public string CloudfrontPrivateKeyPath { get; private set; }
        public string CloudfrontDomain { get; private set; }
        public string CloudfrontKeypairid { get; private set; }

        public CloudFrontSettings(string cloudfrontPrivateKeyPath, string cloudfrontDomain, string cloudfrontKeypairid)
        {
            if (string.IsNullOrWhiteSpace(cloudfrontPrivateKeyPath))
                throw new ArgumentNullException(nameof(cloudfrontPrivateKeyPath));

            if (string.IsNullOrWhiteSpace(cloudfrontDomain))
                throw new ArgumentNullException(nameof(cloudfrontDomain));

            if (string.IsNullOrWhiteSpace(cloudfrontKeypairid))
                throw new ArgumentNullException(nameof(cloudfrontKeypairid));

            CloudfrontPrivateKeyPath = cloudfrontPrivateKeyPath;
            CloudfrontDomain = cloudfrontDomain;
            CloudfrontKeypairid = cloudfrontKeypairid;
        }
    }
}