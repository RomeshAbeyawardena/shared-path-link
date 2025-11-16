using geo_auth.Configuration;
using geo_auth.Extensions;
using GeoAuth.Infrastructure.Azure.Setups;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

var builder = FunctionsApplication.CreateBuilder(args);

builder.ConfigureFunctionsWebApplication();

builder.Services
    .AddApplicationInsightsTelemetryWorkerService()
    .ConfigureFunctionsApplicationInsights()
    .ConfigureOptions(builder.Configuration)
    .RegisterServices()
    .RegisterHandlers();

builder
    .Configuration
    .AddEnvironmentVariables()
    .AddUserSecrets(typeof(Program).Assembly);

builder.Logging.AddConsole();

var app = builder.Build();
var setup = app.Services.GetRequiredKeyedService<ISetup>(string.Empty);

await setup.RunOnceAsync();

if (setup is IHealthCheckSetup defaultSetup)
{
    var healthCheck = await defaultSetup.HealthCheckAsync();
    defaultSetup.BuildHealthCheckTable(healthCheck);

    if (!defaultSetup.DetectAndLogFailures(healthCheck))
    {
        await app.RunAsync();
    }
}