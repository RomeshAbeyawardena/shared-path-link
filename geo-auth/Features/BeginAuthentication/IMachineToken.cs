using GeoAuth.Shared.Models;

namespace geo_auth.Features.BeginAuthentication;

public interface IMachineToken : IMappable<IMachineToken>
{
    Guid? MachineId { get; }
    string? Secret { get; }
    IEnumerable<string> Scopes { get; }
}

public class MachineToken : MappableBase<IMachineToken>, IMachineToken
{
    protected override IMachineToken Source => this;
    public Guid? MachineId { get; set; }
    public string? Secret { get; set; }
    public IEnumerable<string> Scopes { get; set; } = [];

    public override void Map(IMachineToken source)
    {
        MachineId = source.MachineId;
        Secret = source.Secret;
        Scopes = source.Scopes;
    }
}
