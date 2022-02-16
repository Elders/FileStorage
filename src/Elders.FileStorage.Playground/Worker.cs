using FileStorage.Azure;

namespace Elders.FileStorage.Playground
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private readonly IServiceProvider provider;
        private readonly AzureFileStorageRepository repository;

        public Worker(ILogger<Worker> logger, IServiceProvider provider, AzureFileStorageRepository repository)
        {
            _logger = logger;
            this.provider = provider;
            this.repository = repository;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Starting service...");
            _logger.LogInformation("Service started!");
        }
    }
}
