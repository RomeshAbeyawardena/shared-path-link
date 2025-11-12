using geo_auth.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace geo_auth;

public class Setup(ILogger<Setup> logger, IServiceProvider services)
{
    public void RunOnce()
    {
        if (logger.IsEnabled(LogLevel.Information))
        {
            logger.LogInformation("Enabled");
        }
        logger.LogInformation("Registered key services: {count}", KeyedServices.Services.Count);
        foreach (var (key, type) in KeyedServices.Services)
        {
            logger.LogInformation("Setting up: {key} of {type}", key, type.Name);
            services.GetRequiredKeyedService(type, key);
        }

    }
}
