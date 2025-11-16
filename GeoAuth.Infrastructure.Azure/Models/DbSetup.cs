using Azure;
using Azure.Data.Tables;
using GeoAuth.Shared.Features.Setup;
using GeoAuth.Shared.Models.Records;

namespace GeoAuth.Infrastructure.Azure.Models;

public record DbSetup : MappableBase<ISetupEntity>, ITableEntity, ISetupEntity
{
    protected override ISetupEntity Source => this;
    public string MachineId { get; set; } = default!;
    public string Id { get; set; } = default!;
    public DateTimeOffset? Timestamp { get; set; }
    public ETag ETag { get; set; }
    string ISetupEntity.Key { get => Id; }
    string ISetupEntity.Type { get => MachineId; }
    public bool IsEnabled { get; set; }

    public override void Map(ISetupEntity source)
    {
        MachineId = source.Type;
        Id = source.Key;
        IsEnabled = source.IsEnabled;
    }
}
