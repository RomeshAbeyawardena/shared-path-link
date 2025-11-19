using GeoAuth.Shared.Extensions;
using GeoAuth.Shared.Models.Records;

namespace geo_auth.Features.BeginAuthentication;

public record MachineTokenDto : MappableBase<IMachineToken>, IMachineToken
{
    Guid? IMachineToken.MachineId => this.GetGuid(MachineId);
    IEnumerable<string> IMachineToken.Scopes => string.IsNullOrWhiteSpace(Scopes) 
        ? [] 
        : [.. Scopes.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)];

    protected override IMachineToken Source => this;
    public string? MachineId { get; set; }
    public string? Secret { get; set; }
    public string? Scopes { get; set; }
    public override void Map(IMachineToken source)
    {
        MachineId = source.MachineId?.ToString();
        Secret = source.Secret;
        Scopes = source.Scopes.Any() ? string.Empty : string.Join(' ',source.Scopes);
    }
}
