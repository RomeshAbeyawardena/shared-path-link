using MediatR;

namespace GeoAuth.Shared.Requests.MachineToken;

public record UpdateMachineQueryAccessTokenNotification : INotification
{
    public string? Token { get; set; }
    public DateTimeOffset ValidFrom { get; set; }
    public DateTimeOffset Expires { get; set; }
    public required string PartitionKey { get; set; }
}