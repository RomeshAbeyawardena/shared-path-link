using GeoAuth.Shared.Models;

namespace GeoAuth.Shared.Features.Setup;

public class SetupEntity : MappableBase<ISetupEntity>, ISetupEntity
{
    protected override ISetupEntity Source => this;
    public string Key { get; set; } = default!;
    public string Type { get; set; } = default!;
    public bool IsEnabled { get; set; }

    public override void Map(ISetupEntity source)
    {
        Type = source.Type;
        Key = source.Key;
        IsEnabled = source.IsEnabled;
    }
}