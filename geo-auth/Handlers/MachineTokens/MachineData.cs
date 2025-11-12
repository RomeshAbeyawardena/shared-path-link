using Azure;
using Azure.Data.Tables;
using GeoAuth.Shared.Models.Records;
using GeoAuth.Shared.Requests.MachineToken;

namespace geo_auth.Models;

internal record MachineData : ITableEntity
{
    public string? Secret { get; set; }
    public required string PartitionKey { get; set; }
    public required string RowKey { get; set; }
    public DateTimeOffset? Timestamp { get; set; }
    public ETag ETag { get; set; }
}

public record MachineDataAccessToken : MappableBase<IMachineAccessToken>, IMachineAccessToken, ITableEntity
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
