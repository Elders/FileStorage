using System;

namespace FileStorage.Azure
{
    public class AzureCacheControlExpiration
    {
        public ulong InSeconds { get; private set; }

        /// <summary>
        /// Default time is 7 days. Minumum time is 300 seconds
        /// </summary>
        /// <param name="seconds"></param>
        public AzureCacheControlExpiration(ulong seconds = 259200)
        {
            if (seconds < 300) throw new ArgumentException("Minumum Cache Control expiration time is 300 seconds", nameof(seconds));

            InSeconds = seconds;
        }

        public string CacheControlHeader { get { return $"public, max-age={InSeconds}"; } }
    }
}
