using GeoAuth.Shared.Requests.MachineToken;
using MediatR;

namespace GeoAuth.Shared.Features.BeginAuthentication;

public record AuthenticateMachineQuery(Guid? MachineId, string? Secret) : IRequest<AuthenticateMachineResult>
{
    public string? Scopes { get; init; }
}
