using Azure.Data.Tables;
using GeoAuth.Infrastructure.Azure.Configuration;
using GeoAuth.Infrastructure.Azure.Setups;
using GeoAuth.Shared;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace GeoAuth.Infrastructure.Azure.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddDataServices(this IServiceCollection services)
    {
        return services
           .AddKeyedSingleton<ISetup, Setup>(string.Empty)
           .AddKeyedSingleton<ISetup, DevelopmentSetup>("development")
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

    public static IServiceCollection ConfigureDataOptions(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddOptions<MachineTokenTableConfiguration>()
            .Bind(configuration.GetSection("machine"))
            .ValidateOnStart();

        services.AddOptions<ValuesConfiguration>()
            .Bind(configuration.GetSection("values"))
            .ValidateOnStart();

        services.AddOptions<SetupConfiguration>()
            .Bind(configuration.GetSection("Setup"))
            .ValidateOnStart();

        return services;
    }
}
