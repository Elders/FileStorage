using Elders.FileStorage.Playground;
using FileStorage.Azure;

IHost host = Host.CreateDefaultBuilder(args)
            .ConfigureServices(services =>
            {
                services.AddHostedService<Worker>();
                services.AddOptions();
                services.AddAzureStorage();
            })
            .Build();

await host.RunAsync();
