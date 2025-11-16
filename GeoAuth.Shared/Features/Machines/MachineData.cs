using GeoAuth.Shared.Models;

namespace GeoAuth.Shared.Features.Machines;

public record MachineData : Shared.Models.Records.MappableBase<IMachineData>, IMachineData
{
    protected override IMachineData Source => this;
    public string? Secret { get; set; }
    public Guid MachineId { get; set; } = default!;
    public Guid Id { get; set; } = default!;
    public DateTimeOffset? Timestamp { get; set; }

    public override void Map(IMachineData source)
    {
        Secret = source.Secret;
        MachineId = source.MachineId;
        Id = source.Id;
        Timestamp = source.Timestamp;
    }
}
