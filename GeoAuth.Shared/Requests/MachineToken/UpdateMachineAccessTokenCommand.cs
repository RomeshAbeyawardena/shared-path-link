using MediatR;

namespace GeoAuth.Shared.Requests.MachineToken;

public record UpdateMachineAccessTokenCommand : IRequest
{
    public string? Token { get; set; }
    public DateTimeOffset ValidFrom { get; set; }
    public DateTimeOffset Expires { get; set; }
    public Guid MachineId { get; set; }
}