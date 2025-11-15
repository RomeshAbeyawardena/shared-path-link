namespace GeoAuth.Shared.Models;

public interface IMachineData : IMappable<IMachineData>
{
    string? Secret { get; }
    Guid PartitionKey { get; }
    Guid RowKey { get; }
    DateTimeOffset? Timestamp { get; }
}
