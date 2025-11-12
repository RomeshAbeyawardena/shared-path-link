using Azure.Data.Tables;
using geo_auth.Handlers.MachineTokens;
using geo_auth.Handlers.Passwords;
using geo_auth.Handlers.Tokens;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System.Text.Json;

namespace geo_auth.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection RegisterServices(this IServiceCollection services)
    {
        return services.AddSingleton(new JsonSerializerOptions(JsonSerializerOptions.Default)
        {
            PropertyNameCaseInsensitive = true
        }).AddKeyedTransient("machine-token",(s, key) =>
        {
            var machineTokenTableConfiguration = s.GetRequiredService<IOptions<MachineTokenTableConfiguration>>()
                .Value;
            
            var valuesConfiguration = s.GetRequiredService<IOptions<ValuesConfiguration>>()
                .Value;

            var tableClient = new TableClient(valuesConfiguration.AzureWebJobsStorage,
                machineTokenTableConfiguration.MachineTokenTableName);

            return tableClient;
        }).AddKeyedTransient("machine-access-token", (s, key) => {
            var machineTokenTableConfiguration = s.GetRequiredService<IOptions<MachineTokenTableConfiguration>>()
                .Value;

            var valuesConfiguration = s.GetRequiredService<IOptions<ValuesConfiguration>>()
                .Value;

            var tableClient = new TableClient(valuesConfiguration.AzureWebJobsStorage,
                machineTokenTableConfiguration.MachineAccessTokenTableName);

            return tableClient;
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
            .Bind(configuration.GetSection("Values"))
            .ValidateOnStart();

        services.AddOptions<PasswordConfiguration>()
            .Bind(configuration.GetSection("password"))
            .ValidateOnStart();

        services.AddOptions<TokenConfiguration>()
            .Bind(configuration.GetSection("token"))
            .ValidateOnStart();
        return services;
    }
}
