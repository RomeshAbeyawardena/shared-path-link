using GeoAuth.Shared.Models;

namespace geo_auth.Features.BeginAuthentication;

public interface IMachineToken : IMappable<IMachineToken>
{
    Guid? MachineId { get; }
    string? Secret { get; }
    IEnumerable<string> Scopes { get; }
}
