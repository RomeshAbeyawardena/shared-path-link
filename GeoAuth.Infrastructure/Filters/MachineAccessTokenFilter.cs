using GeoAuth.Shared.Models.Records;

namespace GeoAuth.Infrastructure.Filters;

public interface IMachineAccessTokenFilter : IFilter
{
    public Guid? PartitionKey { get; }
    public Guid? RowKey { get; }
    public DateTimeOffset? FromDate { get;}  
    public DateTimeOffset? ToDate { get; }
}

public record MachineAccessTokenFilter : MappableBase<IMachineAccessTokenFilter>, IFilter<IMachineAccessTokenFilter>, IMachineAccessTokenFilter
{
    protected override IMachineAccessTokenFilter Source => this;

    public Guid? PartitionKey { get; set; }
    public Guid? RowKey { get; set; }
    public DateTimeOffset? FromDate { get; set; }
    public DateTimeOffset? ToDate { get; set; }

    public override void Map(IMachineAccessTokenFilter source)
    {
        PartitionKey = source.PartitionKey;
        RowKey = source.RowKey;
        FromDate = source.FromDate;
        ToDate = source.ToDate;
    }
}
