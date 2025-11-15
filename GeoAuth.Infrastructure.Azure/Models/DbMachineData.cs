using Azure;
using Azure.Data.Tables;
using GeoAuth.Infrastructure.Models;
using GeoAuth.Shared.Models;

namespace GeoAuth.Infrastructure.Azure.Models;

public class DbMachineData : MappableBase<IMachineData>, IMachineData, ITableEntity
{
    protected override IMachineData Source => this;
    public string? Secret { get; set; }
    public required string PartitionKey { get; set; }
    public required string RowKey { get; set; }
    public DateTimeOffset? Timestamp { get; set; }
    public ETag ETag { get; set; }

    public override void Map(IMachineData source)
    {
        Secret = source.Secret;
        PartitionKey = source.PartitionKey;
        RowKey = source.RowKey;
        Timestamp = source.Timestamp;
    }
}
