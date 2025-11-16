using GeoAuth.Shared.Features.Setup;
using GeoAuth.Shared.Models.Records;

namespace GeoAuth.Infrastructure.Filters;

public record SetupFilter : MappableBase<ISetupFilter>, ISetupFilter, IFilter
{
    protected override ISetupFilter Source => this;

    public string? Key { get; set; }
    public string? Type { get; set; }

    public override void Map(ISetupFilter source)
    {
        Key = source.Key;
        Type = source.Type;
    }
}
