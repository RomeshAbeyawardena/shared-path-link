using Azure;
using Azure.Data.Tables;
using Azure.Storage.Queues;
using geo_auth.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Formats.Asn1;

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
    public string? Key { get; set; }
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

        if (client is QueueClient queueClient)
        {
            await queueClient.CreateIfNotExistsAsync();
        }
    }

    private async Task<bool> GetEntityStatusAsync(string key, 
        ServiceConfiguration serviceConfiguration)
    {
        return await setupTableClient.QueryAsync<SetupTableEntity>(
            $"Key eq '{key}' and PartitionKey eq '{serviceConfiguration.ServiceType.Name}'", 1)
            .FirstOrDefaultAsync() is not null;
    }

    private async Task SetEntityStatusAsync(string key, ServiceConfiguration serviceConfiguration)
    {
        var entity = await setupTableClient.QueryAsync<SetupTableEntity>(
            $"Key eq '{key}' and PartitionKey eq '{serviceConfiguration.ServiceType.Name}'", 1)
            .FirstOrDefaultAsync();

        if (entity is null)
        {
            await setupTableClient.AddEntityAsync(new SetupTableEntity
            {
                Key = key,
                PartitionKey = serviceConfiguration.ServiceType.Name,
                RowKey = Guid.NewGuid().ToString(),
                Timestamp = timeProvider.GetUtcNow(),
                ETag = ETag.All
            });
        }
    }

    public async Task RunOnceAsync()
    {
        var setupConfiguration = setupOptions.Value;

        setupTableClient.CreateIfNotExists();

        logger.LogTrace("Setup configuration: {configuration}", setupConfiguration);

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

    }
}
