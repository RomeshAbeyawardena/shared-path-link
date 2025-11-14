using Azure;
using Azure.Data.Tables;
using Azure.Storage.Queues;
using geo_auth.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace geo_auth;

public record SetupConfiguration
{
    public string? SetupTableName { get; init; }
    public bool IncludeQueues { get; init; }
    public bool IncludeTables { get; init; }
}

public record SetupTableEntity : ITableEntity
{
    public string PartitionKey { get; set; } = default!;
    public string RowKey { get; set; } = default!;
    public DateTimeOffset? Timestamp { get; set; }
    public ETag ETag { get; set; }
}

public class Setup(ILogger<Setup> logger,
    IOptions<SetupConfiguration> setupOptions,
    [FromKeyedServices(KeyedServices.SetupTable)] TableClient setupTableClient,
    TimeProvider timeProvider,
    IServiceProvider services)
{
    private static async Task CreateIfNotExistsAsync(object client)
    {
        if (client is TableClient tableClient)
        {
            await tableClient.CreateIfNotExistsAsync();
        }
        else if (client is QueueClient queueClient)
        {
            await queueClient.CreateIfNotExistsAsync();
        }
    }

    private Task<SetupTableEntity?> GetSetupTableEntity(string key, ServiceConfiguration serviceConfiguration) => setupTableClient.QueryAsync<SetupTableEntity>(
            $"RowKey eq '{key}' and PartitionKey eq '{serviceConfiguration.ServiceType.Name}'", 1)
            .FirstOrDefaultAsync();

    private async Task<bool> GetEntityStatusAsync(string key, 
        ServiceConfiguration serviceConfiguration)
    {
        return await GetSetupTableEntity(key, serviceConfiguration) is not null;
    }

    private async Task SetEntityStatusAsync(string key, ServiceConfiguration serviceConfiguration)
    {
        var entity = await GetSetupTableEntity(key, serviceConfiguration);

        if (entity is null)
        {
            var response = await setupTableClient.UpsertEntityAsync(new SetupTableEntity
            {
                RowKey = key,
                PartitionKey = serviceConfiguration.ServiceType.Name,
                Timestamp = timeProvider.GetUtcNow(),
                ETag = ETag.All
            });

            if (response.IsError)
            {
                logger.LogError("Upsert failed: {reasonPhrase}", response.ReasonPhrase);
            }
        }
    }

    public async Task RunOnceAsync()
    {
        var setupConfiguration = setupOptions.Value;

        await setupTableClient.CreateIfNotExistsAsync();

        logger.LogTrace("Setup configuration: {configuration}", setupConfiguration);
        int configuredServicesCount = 0;
        logger.LogInformation("Registered key services: {count}", KeyedServices.Services.Count);
        foreach (var (key, config) in KeyedServices.Services)
        {
            logger.LogInformation("Setting up: {key} of {type}", key, config.ServiceType.Name);
            if (!config.IsEnabled 
                || await GetEntityStatusAsync(key, config)
                || (config.ServiceType == typeof(TableClient) && !setupConfiguration.IncludeTables)
                || (config.ServiceType == typeof(QueueClient) && !setupConfiguration.IncludeQueues))
            {
                logger.LogWarning("{key} of {type} skipped", key, config.ServiceType.Name);
                continue;
            }

            var client = services.GetRequiredKeyedService(config.ServiceType, key);
            await CreateIfNotExistsAsync(client);
            await SetEntityStatusAsync(key, config);
        }
        logger.LogInformation("Setup completed. {count} services required configuring.", configuredServicesCount > 0 ? configuredServicesCount.ToString() : "No");
    }
}
