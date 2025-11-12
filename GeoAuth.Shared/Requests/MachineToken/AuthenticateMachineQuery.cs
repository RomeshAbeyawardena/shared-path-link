using GeoAuth.Shared.Models;
using MediatR;

namespace GeoAuth.Shared.Requests.MachineToken;

public record MachineToken
{

}

public record AuthenticateMachineResult(MachineToken? Result, Exception? Exception = null) : ResultBase<MachineToken>(Result, Exception)
{

}

public record AuthenticateMachineQuery(Guid? MachineId, string? Secret) : IRequest<AuthenticateMachineResult>
{

}
