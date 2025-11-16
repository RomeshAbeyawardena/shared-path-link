using GeoAuth.Shared.Models;

namespace GeoAuth.Shared.Features.Setup;

public interface ISetupEntity : IMappable<ISetupEntity>
{
    string Key { get; }
    string Type { get; }
    bool IsEnabled { get; }
}
