using Microsoft.Extensions.DependencyInjection;
using OptionsExtensions;

namespace FileStorage.Azure
{
    public static class AzureFileStorageServiceCollectionExtensions
    {
        public static IServiceCollection AddAzureStorage(this IServiceCollection services)
        {
            services.AddFileStorage();

            services.AddOption<AzureStorageOptions, AzureStorageOptionsProvider>();
            services.AddSingleton<AzureStorageClient>();
            services.AddSingleton<AzureFileStorageRepository>();
            services.AddSingleton<IFileStorageRepository, AzureFileStorageRepository>();

            return services;
        }
    }
}
