using GeoAuth.Shared.Models;

namespace GeoAuth.Infrastructure.Models;

public interface IMachineData : IMappable<IMachineData>
{
    string? Secret { get; }
    string PartitionKey { get; }
    string RowKey { get; }
    DateTimeOffset? Timestamp { get; }
}
