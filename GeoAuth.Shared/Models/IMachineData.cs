namespace GeoAuth.Shared.Models;

public interface IMachineData : IMappable<IMachineData>
{
    string? Secret { get; }
    Guid MachineId { get; }
    Guid Id { get; }
    DateTimeOffset? Timestamp { get; }
}
