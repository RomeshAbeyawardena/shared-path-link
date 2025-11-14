using Azure.Data.Tables;
using Azure.Storage.Queues;
using geo_auth.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace geo_auth;

public record SetupConfiguration
{
    public bool IncludeQueues { get; init; }
    public bool IncludeTables { get; init; }
}

public class Setup(ILogger<Setup> logger,
    IOptions<SetupConfiguration> setupOptions,
    IServiceProvider services)
{
    public void RunOnce()
    {
        var setupConfiguration = setupOptions.Value;

        logger.LogTrace("Setup configuration: {configuration}", setupConfiguration);

        logger.LogInformation("Registered key services: {count}", KeyedServices.Services.Count);
        foreach (var (key, type) in KeyedServices.Services)
        {
            logger.LogInformation("Setting up: {key} of {type}", key, type.Name);
            if ((type == typeof(TableClient) && !setupConfiguration.IncludeTables)
                || (type == typeof(QueueClient) && !setupConfiguration.IncludeQueues))
            {
                logger.LogWarning("{key} of {type} skipped", key, type.Name);
                continue;
            }

            services.GetRequiredKeyedService(type, key);
        }

    }
}
