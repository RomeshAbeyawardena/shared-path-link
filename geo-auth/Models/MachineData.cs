using Azure;
using Azure.Data.Tables;

namespace geo_auth.Models;

internal record MachineData : ITableEntity
{
    public Guid MachineId { get; set; }
    public string? Secret { get; set; }
    public required string PartitionKey { get; set; }
    public required string RowKey { get; set; }
    public DateTimeOffset? Timestamp { get; set; }
    public ETag ETag { get; set; }
}

internal record MachineDataAccessToken : ITableEntity
{
    public Guid MachineId { get; set; }
    public string? Token { get; set; }
    public DateTimeOffset ValidFrom { get; set; }
    public DateTimeOffset Expires { get; set; }
    public required string PartitionKey { get; set; }
    public required string RowKey { get; set; }
    public DateTimeOffset? Timestamp { get; set; }
    public ETag ETag { get; set; }
}