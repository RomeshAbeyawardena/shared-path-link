using GeoAuth.Shared.Models;
using MediatR;

namespace GeoAuth.Shared.Requests.Tokens;

public record MachineTokenQueryResponse
{

}

public record MachineTokenQueryResult(MachineTokenQueryResponse Result, Exception? Exception) : ResultBase<MachineTokenQueryResponse>(Result, Exception)
{

}

public record ValidateMachineTokenQuery(string Token) : IRequest<MachineTokenQueryResult>
{
}
