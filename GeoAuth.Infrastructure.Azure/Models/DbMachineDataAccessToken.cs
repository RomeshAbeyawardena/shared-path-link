using Azure;
using Azure.Data.Tables;
using GeoAuth.Shared.Extensions;
using GeoAuth.Shared.Models;
using GeoAuth.Shared.Requests.MachineToken;

namespace GeoAuth.Infrastructure.Azure.Models;

public class DbMachineDataAccessToken : MappableBase<IMachineAccessToken>, IMachineAccessToken, ITableEntity
{
    protected override IMachineAccessToken Source => this;
    public string? Token { get; set; }
    public DateTimeOffset ValidFrom { get; set; }
    public DateTimeOffset Expires { get; set; }
    Guid IMachineAccessToken.MachineId { get => this.GetGuid(PartitionKey);  }
    Guid IMachineAccessToken.Id { get => this.GetGuid(RowKey); }
    public DateTimeOffset? Timestamp { get; set; }
    public ETag ETag { get; set; }
    public string PartitionKey { get; set; } = default!;
    public string RowKey { get; set; } = default!;

    public override void Map(IMachineAccessToken source)
    {
        Token = source.Token;
        ValidFrom = source.ValidFrom;
        Expires = source.Expires;
        PartitionKey = source.MachineId.ToString();
        RowKey = source.Id.ToString();
    }
}
