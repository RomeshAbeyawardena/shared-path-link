using Azure;
using Azure.Data.Tables;
using Azure.Storage.Queues;
using geo_auth.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Text;

namespace geo_auth.Configuration;

public record ColumnDefiniton(string Name, int Length);

internal interface ISetup
{
    Task RunOnceAsync();
}

internal class Setup(ILogger<Setup> logger,
    IOptions<SetupConfiguration> setupOptions,
    [FromKeyedServices(KeyedServices.SetupTable)] TableClient setupTableClient,
    TimeProvider timeProvider, IServiceProvider services) : ISetup
{
    private bool hasRun = false;
    private readonly ReaderWriterLockSlim hasRunLock = new(LockRecursionPolicy.NoRecursion);
    private bool HasRun
    {
        get
        {
            hasRunLock.EnterReadLock();
            var value = hasRun;
            hasRunLock.ExitReadLock();
            return value;
        }

        set
        {
            hasRunLock.EnterWriteLock();
            hasRun = value;
            hasRunLock.ExitWriteLock();
        }
    }

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

    private async static Task<ServiceStatus?> CheckHealthAsync(string key, object client)
    {
        bool? exists;
        try
        {
            if (client is QueueClient queueClient)
            {
                exists = await queueClient.ExistsAsync();
                return new ServiceStatus(key, typeof(QueueClient), exists);
            }

            if (client is TableClient tableClient)
            {
                await tableClient.QueryAsync<TableEntity>().FirstOrDefaultAsync(CancellationToken.None);
                return new ServiceStatus(key, typeof(TableClient), true);
            }

            return null;
        }
        catch (RequestFailedException ex)
        {
            return new ServiceStatus(key, typeof(object), null, ex);
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

    public async Task<IReadOnlyDictionary<string, ServiceStatus>> HealthCheckAsync()
    {
        logger.LogInformation("Initiating health-check...");
        var healthCheckStatuses = new Dictionary<string, ServiceStatus>();
        foreach(var (key, config) in KeyedServices.Services)
        {
            var client = services.GetRequiredKeyedService(config.ServiceType, key);
            var result = await CheckHealthAsync(key, client);
            if (result is null)
            {
                continue;
            }

            healthCheckStatuses.Add(key, result);
        }

        return healthCheckStatuses;
    }

    public void BuildHealthCheckTable(IReadOnlyDictionary<string, ServiceStatus> serviceStatus)
    {
        logger.LogInformation("Building health-check table...");
        var healthCheckTable = new StringBuilder();
        ColumnDefiniton[] columnNames = [
            new ColumnDefiniton("Key", 26),
            new ColumnDefiniton("Type", 12),
            new ColumnDefiniton("Exists", 6), 
            new ColumnDefiniton("Error", 32)];

        for (int i = 0; i < columnNames.Length; i++)
        {
            var column = columnNames[i];

            if (i > 0)
            {
                healthCheckTable.Append('\t');
            }

            healthCheckTable.Append($"{column.Name.ToFixedLength(column.Length)}");
        }

        healthCheckTable.Append(Environment.NewLine);

        foreach(var (key, config) in serviceStatus)
        {
            healthCheckTable.AppendLine($"{key.ToFixedLength(26)}\t{config.Type.Name.ToFixedLength(12)}" +
                $"\t{(config.Exists.GetValueOrDefault() ? "Yes" : "No" ).ToString().ToFixedLength(6)}\t{(config.Exception?.Message ?? "N/A").ToFixedLength(32)}");
        }

        logger.LogInformation("{healthCheck}", healthCheckTable.ToString());
    }

    public bool DetectAndLogFailures(IReadOnlyDictionary<string, ServiceStatus> serviceStatuses)
    {
        if (!serviceStatuses.Any((x) => x.Value.IsError))
        {
            logger.LogInformation("Health checks passed, continuing operation...");
            return false;
        }

        foreach (var (key, serviceStatus) in serviceStatuses)
        {
            logger.LogError(serviceStatus.Exception, "Failure occurred with {key}", key);
        }

        return true;
    }

    public async Task RunOnceAsync()
    {
        if (HasRun)
        {
            logger.LogWarning("Setup has already been run and can not be run again");
            return;
        }

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
                || config.ServiceType == typeof(TableClient) && !setupConfiguration.IncludeTables
                || config.ServiceType == typeof(QueueClient) && !setupConfiguration.IncludeQueues)
            {
                logger.LogWarning("{key} of {type} skipped", key, config.ServiceType.Name);
                continue;
            }
            var client = services.GetRequiredKeyedService(config.ServiceType, key);
            await CreateIfNotExistsAsync(client);
            await SetEntityStatusAsync(key, config);
            configuredServicesCount++;
        }

        if (setupConfiguration.IncludeDevelopmentData)
        {
            await services
                .GetRequiredKeyedService<ISetup>("development")
                .RunOnceAsync();
        }

        logger.LogInformation("Setup completed. {count} services required configuring.", configuredServicesCount > 0 ? configuredServicesCount.ToString() : "No");
        HasRun = true;
    }
}
