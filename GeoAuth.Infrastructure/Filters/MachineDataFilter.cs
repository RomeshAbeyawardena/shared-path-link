using GeoAuth.Shared.Models.Records;

namespace GeoAuth.Infrastructure.Filters;

public interface IMachineDataFilter: IFilter<IMachineDataFilter>
{

}

public record MachineDataFilter : MappableBase<IMachineDataFilter>, IMachineDataFilter
{
    protected override IMachineDataFilter Source => this;

    public override void Map(IMachineDataFilter source)
    {
        
    }
}
