namespace GeoAuth.Shared.Requests.MachineToken
{
    public interface IMachineAccessToken
    {
        string? Token { get; }
        DateTimeOffset ValidFrom { get; }
        DateTimeOffset Expires { get; }
        string PartitionKey { get; }
        string RowKey { get; }
        DateTimeOffset? Timestamp { get; }
    }
}
