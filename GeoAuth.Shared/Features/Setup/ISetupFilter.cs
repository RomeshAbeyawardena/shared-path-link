using GeoAuth.Shared.Models;

namespace GeoAuth.Shared.Features.Setup;

public interface ISetupFilter : IMappable<ISetupFilter>
{
    string? Key { get; }
    string? Type { get; }
}
