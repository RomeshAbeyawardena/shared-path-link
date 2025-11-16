using GeoAuth.Shared.Models;

namespace geo_auth.Handlers.MachineTokens;

internal record MachineData : GeoAuth.Shared.Models.Records.MappableBase<IMachineData>, IMachineData
{
    protected override IMachineData Source => this;
    public string? Secret { get; set; }
    public Guid MachineId { get; set; }
    public Guid Id { get; set; }
    public DateTimeOffset? Timestamp { get; set; }

    public override void Map(IMachineData source)
    {
        Secret = source.Secret;
        MachineId = source.MachineId;
        Id = source.Id;
        Timestamp = source.Timestamp;
    }
}
