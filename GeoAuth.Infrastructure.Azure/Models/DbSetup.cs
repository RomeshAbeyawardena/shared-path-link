using Azure;
using Azure.Data.Tables;
using GeoAuth.Shared.Features.Setup;
using GeoAuth.Shared.Models.Records;

namespace GeoAuth.Infrastructure.Azure.Models;

public record DbSetup : MappableBase<ISetupEntity>, ITableEntity, ISetupEntity
{
    protected override ISetupEntity Source => this;
    public string PartitionKey { get; set; } = default!;
    public string RowKey { get; set; } = default!;
    public DateTimeOffset? Timestamp { get; set; }
    public ETag ETag { get; set; }
    string ISetupEntity.Key { get => RowKey; }
    string ISetupEntity.Type { get => PartitionKey; }
    public bool IsEnabled { get; set; }

    public override void Map(ISetupEntity source)
    {
        PartitionKey = source.Type;
        RowKey = source.Key;
        IsEnabled = source.IsEnabled;
    }
}
