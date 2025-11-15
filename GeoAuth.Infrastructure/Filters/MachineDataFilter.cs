using GeoAuth.Shared.Models.Records;

namespace GeoAuth.Infrastructure.Filters;

public interface IMachineDataFilter: IFilter<IMachineDataFilter>
{
    Guid? RowKey { get; }
    Guid? MachineId { get; }
    string? Secret { get;}
}

public record MachineDataFilter : MappableBase<IMachineDataFilter>, IMachineDataFilter
{
    protected override IMachineDataFilter Source => this;
    
    public Guid? RowKey { get; set; }
    public Guid? MachineId { get; set; }
    public string? Secret { get; set; }

    public override void Map(IMachineDataFilter source)
    {
        MachineId = source.MachineId;
        Secret = source.Secret;
    }
}
