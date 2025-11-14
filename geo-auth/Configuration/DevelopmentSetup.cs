using Azure.Data.Tables;
using geo_auth.Extensions;
using geo_auth.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace geo_auth.Configuration;

internal class DevelopmentSetup([FromKeyedServices(KeyedServices.MachineTable)] TableClient machineTableClient,
    IOptions<SetupConfiguration> options, IHostEnvironment hostEnvironment, ILogger<DevelopmentSetup> logger) : ISetup
{
    public async Task RunOnceAsync(SetupConfiguration? setupConfiguration = null)
    {
        setupConfiguration ??= options.Value;
        if (hostEnvironment.IsDevelopment() && !setupConfiguration.IncludeDevelopmentData)
        {
            logger.LogWarning("Including development data in a production environment is not recommended.");
        }

        if (setupConfiguration.MachineData is null)
        {
            logger.LogWarning("No development data to copy");
            return;
        }

        var clonedMachineData = new MachineData {
            PartitionKey = string.Empty,
            RowKey = string.Empty
        };

        clonedMachineData.Map(setupConfiguration.MachineData);

        await machineTableClient.UpsertEntityAsync(clonedMachineData);
    }

    public Task RunOnceAsync()
    {
        return RunOnceAsync(null);
    }
}
