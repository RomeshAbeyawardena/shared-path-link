using geo_auth.Handlers.Passwords;
using geo_auth.Handlers.Tokens;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace geo_auth.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection RegisterHandlers(this IServiceCollection services)
    {
        return services.AddMediatR(c => c.RegisterServicesFromAssemblyContaining<Program>());
    }

    public static IServiceCollection ConfigureOptions(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddOptions<PasswordConfiguration>()
            .Bind(configuration.GetSection("password"))
            .ValidateOnStart();

        services.AddOptions<TokenConfiguration>()
            .Bind(configuration.GetSection("token"))
            .ValidateOnStart();
        return services;
    }
}
