using GeoAuth.Shared.Models;

namespace GeoAuth.Shared.Requests.MachineToken;

public record AuthenticateMachineResult(AuthenticatedMachineToken? Result, Exception? Exception = null) : ResultBase<AuthenticatedMachineToken>(Result, Exception)
{

}
