namespace GeoAuth.Shared.Requests.MachineToken
{
    public interface IMachineAccessToken
    {
        string? Token { get; }
        DateTimeOffset ValidFrom { get; }
        DateTimeOffset Expires { get; }
        Guid MachineId { get; }
        Guid Id { get; }
        DateTimeOffset? Timestamp { get; }
    }
}
