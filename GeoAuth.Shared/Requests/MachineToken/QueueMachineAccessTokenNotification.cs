using MediatR;

namespace GeoAuth.Shared.Requests.MachineToken;

public record QueueMachineAccessTokenNotification : INotification
{
    public string? Token { get; set; }
    public DateTimeOffset ValidFrom { get; set; }
    public DateTimeOffset Expires { get; set; }
    public required string MachineId { get; set; }
}
