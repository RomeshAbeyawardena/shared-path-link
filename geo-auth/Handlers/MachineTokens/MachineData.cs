using GeoAuth.Shared.Models;

namespace geo_auth.Handlers.MachineTokens;

internal record MachineData : GeoAuth.Shared.Models.Records.MappableBase<IMachineData>, IMachineData
{
    protected override IMachineData Source => this;
    public string? Secret { get; set; }
    public required string PartitionKey { get; set; }
    public required string RowKey { get; set; }
    public DateTimeOffset? Timestamp { get; set; }

    public override void Map(IMachineData source)
    {
        Secret = source.Secret;
        PartitionKey = source.PartitionKey;
        RowKey = source.RowKey;
        Timestamp = source.Timestamp;
    }
}
