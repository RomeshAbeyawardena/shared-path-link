using Azure;
using Azure.Data.Tables;
using GeoAuth.Shared.Models;
using GeoAuth.Shared.Requests.MachineToken;

namespace GeoAuth.Infrastructure.Azure.Models;

public class DbMachineDataAccessToken : MappableBase<IMachineAccessToken>, IMachineAccessToken, ITableEntity
{
    protected override IMachineAccessToken Source => this;
    public string? Token { get; set; }
    public DateTimeOffset ValidFrom { get; set; }
    public DateTimeOffset Expires { get; set; }
    public required string PartitionKey { get; set; }
    public string RowKey { get; set; } = default!;
    public DateTimeOffset? Timestamp { get; set; }
    public ETag ETag { get; set; }

    public override void Map(IMachineAccessToken source)
    {
        Token = source.Token;
        ValidFrom = source.ValidFrom;
        Expires = source.Expires;
        PartitionKey = source.PartitionKey;
        RowKey = source.RowKey;
        Timestamp = source.Timestamp;
    }
}
