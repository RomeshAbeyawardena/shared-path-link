using GeoAuth.Shared;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = FunctionsApplication.CreateBuilder(args);

builder.ConfigureFunctionsWebApplication();

builder.Services
    .AddApplicationInsightsTelemetryWorkerService()
    .ConfigureFunctionsApplicationInsights()
    .ConfigureOptions(builder.Configuration)
    .AddHttpContextAccessor()
    .RegisterHandlers();

builder
    .Configuration
    .AddUserSecrets(typeof(Program).Assembly);

builder.Build().Run();
