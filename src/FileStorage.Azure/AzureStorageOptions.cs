using Microsoft.Extensions.Configuration;
using OptionsExtensions;
using System.ComponentModel.DataAnnotations;

namespace FileStorage.Azure
{
    public class AzureStorageOptions
    {
        [Required]
        public string ConnectionString { get; set; }

        [Required]
        public string Container { get; set; }

        public uint UrlTtlInSeconds { get; set; } = 0;

        public uint CacheControlExpiration { get; set; } = 259200;

        [Required]
        public string CdnUrl { get; set; }
    }

    public class AzureStorageOptionsProvider : OptionsProviderBase<AzureStorageOptions>
    {
        public AzureStorageOptionsProvider(IConfiguration configuration) : base(configuration) { }

        public override void Configure(AzureStorageOptions options)
        {
            configuration.GetSection("AzureStorage").Bind(options);
        }
    }
}
