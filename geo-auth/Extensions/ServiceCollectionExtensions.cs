using Azure.Data.Tables;
using Azure.Storage.Queues;
using geo_auth.Configuration;
using geo_auth.Handlers.MachineTokens;
using geo_auth.Handlers.Passwords;
using geo_auth.Handlers.Tokens;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System.Text.Json;

namespace geo_auth.Extensions;

public record ServiceConfiguration(Type ServiceType, bool IsEnabled);

public static class KeyedServices
{
    public const string SetupTable = "setup-table";
    public const string MachineAccessTokenQueue = "machine-access-token-queue";
    public const string MachineAccessTokenTable = "machine-access-token-table";
    public const string MachineTable = "machine-table";

    public static readonly IReadOnlyDictionary<string, ServiceConfiguration> Services = new Dictionary<string, ServiceConfiguration>
    {
        { MachineTable, new ServiceConfiguration(typeof(TableClient), true) },
        { MachineAccessTokenQueue, new ServiceConfiguration(typeof(QueueClient), true) },
        { MachineAccessTokenTable, new ServiceConfiguration(typeof(TableClient), true) },
        { SetupTable, new ServiceConfiguration(typeof(TableClient), false) }
    };
}

public static class ServiceCollectionExtensions
{
    public static IServiceCollection RegisterServices(this IServiceCollection services)
    {
        return services
            .AddSingleton<Setup>()
            .AddSingleton(new JsonSerializerOptions(JsonSerializerOptions.Default)
        {
            PropertyNameCaseInsensitive = true
        })
        .AddSingleton(TimeProvider.System)
        .AddKeyedTransient(KeyedServices.MachineAccessTokenQueue, (s, key) =>
        {
            var machineTokenTableConfiguration = s.GetRequiredService<IOptions<MachineTokenTableConfiguration>>()
                .Value;

            var valuesConfiguration = s.GetRequiredService<IOptions<ValuesConfiguration>>()
                .Value;

            var queueClient = new QueueClient(valuesConfiguration.AzureWebJobsStorage, machineTokenTableConfiguration.MachineAccessTokenQueueName, new QueueClientOptions
            {
                MessageEncoding = QueueMessageEncoding.Base64
            });
            
            return queueClient;
        })
        .AddKeyedTransient(KeyedServices.MachineTable, (s, key) =>
        {
            var machineTokenTableConfiguration = s.GetRequiredService<IOptions<MachineTokenTableConfiguration>>()
                .Value;
            
            var valuesConfiguration = s.GetRequiredService<IOptions<ValuesConfiguration>>()
                .Value;

            var tableClient = new TableClient(valuesConfiguration.AzureWebJobsStorage,
                machineTokenTableConfiguration.MachineTokenTableName);
            return tableClient;
        })
        .AddKeyedTransient(KeyedServices.MachineAccessTokenTable, (s, key) => {
            var machineTokenTableConfiguration = s.GetRequiredService<IOptions<MachineTokenTableConfiguration>>()
                .Value;

            var valuesConfiguration = s.GetRequiredService<IOptions<ValuesConfiguration>>()
                .Value;

            var tableClient = new TableClient(valuesConfiguration.AzureWebJobsStorage,
                machineTokenTableConfiguration.MachineAccessTokenTableName);
            return tableClient;
        })
        .AddKeyedTransient(KeyedServices.SetupTable, (s, key) => {
             var setupTableConfiguration = s.GetRequiredService<IOptions<SetupConfiguration>>()
                 .Value;

             var valuesConfiguration = s.GetRequiredService<IOptions<ValuesConfiguration>>()
                 .Value;

             return new TableClient(valuesConfiguration.AzureWebJobsStorage,
                 setupTableConfiguration.SetupTableName);
         });
    }

    public static IServiceCollection RegisterHandlers(this IServiceCollection services)
    {
        return services.AddMediatR(c => c.RegisterServicesFromAssemblyContaining<Program>());
    }

    public static IServiceCollection ConfigureOptions(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddOptions<MachineTokenTableConfiguration>()
            .Bind(configuration.GetSection("machine"))
            .ValidateOnStart();

        services.AddOptions<ValuesConfiguration>()
            .Bind(configuration.GetSection("values"))
            .ValidateOnStart();

        services.AddOptions<PasswordConfiguration>()
            .Bind(configuration.GetSection("password"))
            .ValidateOnStart();

        services.AddOptions<SetupConfiguration>()
            .Bind(configuration.GetSection("Setup"))
            .ValidateOnStart();

        services.AddOptions<TokenConfiguration>()
            .Bind(configuration.GetSection("token"))
            .ValidateOnStart();
        return services;
    }
}
