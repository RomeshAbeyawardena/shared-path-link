using GeoAuth.Infrastructure.Filters;
using GeoAuth.Infrastructure.Models;
using GeoAuth.Infrastructure.Repositories;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace GeoAuth.Infrastructure.Azure.Setups;

internal class DevelopmentSetup(IMachineRepository machineRepository,
    IOptions<SetupConfiguration> options, IHostEnvironment hostEnvironment, ILogger<DevelopmentSetup> logger) : ISetup
{
    public async Task RunOnceAsync(SetupConfiguration? setupConfiguration = null)
    {
        setupConfiguration ??= options.Value;
        if (hostEnvironment.IsDevelopment() && !setupConfiguration.IncludeDevelopmentData)
        {
            logger.LogWarning("Including development data in a production environment is not recommended.");
        }

        logger.LogInformation("Within seeding data operation");

        if (setupConfiguration.MachineData is null)
        {
            logger.LogWarning("No development data to seed");
            return;
        }

        var clonedMachineData = new MachineData {
            PartitionKey = Guid.Empty,
            RowKey = Guid.Empty
        };

        clonedMachineData.Map(setupConfiguration.MachineData);

        var result = await machineRepository.GetAsync(new MachineDataFilter
        {
            RowKey = clonedMachineData.RowKey,
            MachineId = clonedMachineData.PartitionKey
        }, CancellationToken.None);

        if (result is null)
        {
            await machineRepository.UpsertAsync(clonedMachineData, CancellationToken.None);
            logger.LogInformation("Development data seeded");
        }
        else
        {
            logger.LogInformation("Seeding data operation skipped! Reason: Potentially exists and is not stale, evaluate inserted data before proceeding.");
        }
    }

    public Task RunOnceAsync()
    {
        return RunOnceAsync(null);
    }
}
