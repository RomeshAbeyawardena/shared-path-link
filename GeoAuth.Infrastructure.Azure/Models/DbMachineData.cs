using Azure;
using Azure.Data.Tables;
using GeoAuth.Shared.Models;

namespace GeoAuth.Infrastructure.Azure.Models;

public class DbMachineData : MappableBase<IMachineData>, IMachineData, ITableEntity
{
    protected override IMachineData Source => this;
    public string? Secret { get; set; }
    public required Guid Id { get; set; }
    public required Guid MachineId { get; set; }
    public DateTimeOffset? Timestamp { get; set; }
    public ETag ETag { get; set; }
    string ITableEntity.PartitionKey { 
        get => MachineId.ToString(); 
        set 
        {
            if (Guid.TryParse(value, out var id))
            {
                MachineId = id;
            }
        } 
    }

    string ITableEntity.RowKey { 
        get => Id.ToString(); 
        set
        {
            if (Guid.TryParse(value, out var id))
            {
                Id = id;
            }
        }
    }

    public override void Map(IMachineData source)
    {
        Secret = source.Secret;
        MachineId = source.MachineId;
        Id = source.Id;
        Timestamp = source.Timestamp;
    }
}
