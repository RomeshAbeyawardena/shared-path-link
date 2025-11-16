using GeoAuth.Shared.Requests.MachineToken;

namespace geo_auth.Handlers.MachineTokens;

public record MachineDataAccessToken : GeoAuth.Shared.Models.Records.MappableBase<IMachineAccessToken>, IMachineAccessToken
{
    protected override IMachineAccessToken Source => this;
    public string? Token { get; set; }
    public DateTimeOffset ValidFrom { get; set; }
    public DateTimeOffset Expires { get; set; }
    public Guid MachineId { get; set; }
    public Guid Id { get; set; } = default!;
    public DateTimeOffset? Timestamp { get; set; }

    public override void Map(IMachineAccessToken source)
    {
        Token = source.Token;
        ValidFrom = source.ValidFrom;
        Expires = source.Expires;
        MachineId = source.MachineId;
        Id = source.Id;
        Timestamp = source.Timestamp;
    }
}
