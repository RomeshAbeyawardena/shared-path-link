using GeoAuth.Shared.Requests.Tokens;

namespace geo_auth.Models;

internal class RegisteredMachineConfiguration
{
    public IEnumerable<MachineData> Machines { get; set; } = [];
    public bool IsRegistered(MachineTokenQueryResponse response)
    {
        return Machines.Any(x => x.MachineId == response.MachineId && x.Secret == response.Secret);
    }
}
