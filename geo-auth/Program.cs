using geo_auth.Configuration;
using geo_auth.Extensions;
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
var setup = app.Services.GetRequiredService<Setup>();

await setup.RunOnceAsync();
var healthCheck = await setup.HealthCheckAsync();
setup.BuildHealthCheckTable(healthCheck);

if (!setup.DetectAndLogFailures(healthCheck))
{
    await app.RunAsync();
}