using GeoAuth.Infrastructure.Models;

namespace GeoAuth.Infrastructure.Azure.Setups;

internal record SetupConfiguration
{
    public string? SetupTableName { get; init; }
    public bool IncludeQueues { get; init; }
    public bool IncludeTables { get; init; }
    public bool IncludeDevelopmentData { get; init; }
    public MachineData? MachineData { get; init; }
}
