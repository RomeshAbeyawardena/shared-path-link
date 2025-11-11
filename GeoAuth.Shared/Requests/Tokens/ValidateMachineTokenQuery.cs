using GeoAuth.Shared.Models;
using MediatR;

namespace GeoAuth.Shared.Requests.Tokens;

public record MachineTokenQueryResponse(Guid? MachineId, string? Secret)
{

}

public record MachineTokenQueryResult(MachineTokenQueryResponse? Result, Exception? Exception = null) : ResultBase<MachineTokenQueryResponse>(Result, Exception)
{

}

public record ValidateMachineTokenQuery(string Token) : IRequest<MachineTokenQueryResult>
{
}
