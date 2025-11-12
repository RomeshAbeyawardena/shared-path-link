using GeoAuth.Shared.Models;

namespace GeoAuth.Shared.Requests.MachineToken;

public record AuthenticateMachineResult(MachineToken? Result, Exception? Exception = null) : ResultBase<MachineToken>(Result, Exception)
{

}
