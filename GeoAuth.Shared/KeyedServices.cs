namespace GeoAuth.Shared;

public enum ClientType
{
    Table,
    Queue
}

public record ServiceConfiguration(ClientType Type, bool IsEnabled);

public static class KeyedServices
{
    public const string SetupTable = "setup-table";
    public const string MachineAccessTokenQueue = "machine-access-token-queue";
    public const string MachineAccessTokenTable = "machine-access-token-table";
    public const string MachineTable = "machine-table";

    public static readonly IReadOnlyDictionary<string, ServiceConfiguration> Services = new Dictionary<string, ServiceConfiguration>
    {
        { MachineTable, new ServiceConfiguration(ClientType.Table, true) },
        { MachineAccessTokenQueue, new ServiceConfiguration(ClientType.Queue, true) },
        { MachineAccessTokenTable, new ServiceConfiguration(ClientType.Table, true) },
        { SetupTable, new ServiceConfiguration(ClientType.Table, false) }
    };
}

