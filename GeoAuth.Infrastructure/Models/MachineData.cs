using GeoAuth.Shared.Models;

namespace GeoAuth.Infrastructure.Models;

public record MachineData : Shared.Models.Records.MappableBase<IMachineData>, IMachineData
{
    protected override IMachineData Source => this;
    public string? Secret { get; set; }
    public Guid PartitionKey { get; set; } = default!;
    public Guid RowKey { get; set; } = default!;
    public DateTimeOffset? Timestamp { get; set; }

    public override void Map(IMachineData source)
    {
        Secret = source.Secret;
        PartitionKey = source.PartitionKey;
        RowKey = source.RowKey;
        Timestamp = source.Timestamp;
    }
}
