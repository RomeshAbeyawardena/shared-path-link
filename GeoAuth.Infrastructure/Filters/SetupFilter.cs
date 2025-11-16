using GeoAuth.Shared.Features.Setup;
using GeoAuth.Shared.Models.Records;

namespace GeoAuth.Infrastructure.Filters;

public record SetupFilter : MappableBase<ISetupFilter>, ISetupFilter, IFilter
{
    protected override ISetupFilter Source => this;



    public override void Map(ISetupFilter source)
    {
        throw new NotImplementedException();
    }
}
