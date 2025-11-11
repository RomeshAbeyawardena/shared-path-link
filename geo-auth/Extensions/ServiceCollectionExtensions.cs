using Microsoft.Extensions.DependencyInjection;

namespace GeoAuth.Shared;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection RegisterHandlers(this IServiceCollection services)
    {
        return services.AddMediatR(c => c.RegisterServicesFromAssemblyContaining<Program>());
    }
}
