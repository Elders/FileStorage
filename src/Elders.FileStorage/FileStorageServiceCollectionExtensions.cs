using Microsoft.Extensions.DependencyInjection;

namespace FileStorage
{
    public static class FileStorageServiceCollectionExtensions
    {
        public static IServiceCollection AddFileStorage(this IServiceCollection services)
        {
            services.AddSingleton<IFileGenerator, FileGenerator>();
            services.AddSingleton<FileGenerator>();

            return services;
        }
    }
}
