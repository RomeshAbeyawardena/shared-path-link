using MediatR;

namespace GeoAuth.Shared.Requests.MachineToken;

public record AuthenticateMachineQuery(Guid? MachineId, string? Secret) : IRequest<AuthenticateMachineResult>
{

}
