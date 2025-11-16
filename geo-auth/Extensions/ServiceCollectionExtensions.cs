using Azure.Storage.Queues;
using geo_auth.Configuration;
using GeoAuth.Infrastructure.Azure.Configuration;
using GeoAuth.Infrastructure.Azure.Extensions;
using GeoAuth.Shared;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System.Text.Json;

namespace geo_auth.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection RegisterServices(this IServiceCollection services)
    {
        return services
            .AddDataServices()
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
            });
    }

    public static IServiceCollection RegisterHandlers(this IServiceCollection services)
    {
        return services.AddMediatR(c => c.RegisterServicesFromAssemblyContaining<Program>());
    }

    public static IServiceCollection ConfigureOptions(this IServiceCollection services, IConfiguration configuration)
    {
        services
            .ConfigureDataOptions(configuration)
            .AddOptions<PasswordConfiguration>()
            .Bind(configuration.GetSection("password"))
            .ValidateOnStart();

        services.AddOptions<TokenConfiguration>()
            .Bind(configuration.GetSection("token"))
            .ValidateOnStart();

        return services;
    }
}
