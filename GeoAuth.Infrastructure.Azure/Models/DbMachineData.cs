using Azure;
using Azure.Data.Tables;
using GeoAuth.Shared.Models;
using GeoAuth.Shared.Extensions;

namespace GeoAuth.Infrastructure.Azure.Models;

public class DbMachineData : MappableBase<IMachineData>, IMachineData, ITableEntity
{
    protected override IMachineData Source => this;
    public string? Secret { get; set; }
    
    public DateTimeOffset? Timestamp { get; set; }
    public ETag ETag { get; set; }
    public string PartitionKey { get; set; } = default!;
    public string RowKey { get; set; } = default!;
    Guid IMachineData.MachineId { get => this.GetGuid(PartitionKey); }
    Guid IMachineData.Id { get => this.GetGuid(RowKey); }

    public override void Map(IMachineData source)
    {
        Secret = source.Secret;
        PartitionKey = source.MachineId.ToString();
        RowKey = source.Id.ToString();
        Timestamp = source.Timestamp;
    }
}
