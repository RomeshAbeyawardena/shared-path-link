using geo_auth.Models;

namespace geo_auth.Configuration;

internal record SetupConfiguration
{
    public string? SetupTableName { get; init; }
    public bool IncludeQueues { get; init; }
    public bool IncludeTables { get; init; }
    public bool IncludeDevelopmentData { get; init; }
    public MachineData? MachineData { get; init; }
}
