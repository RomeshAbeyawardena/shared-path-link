using GeoAuth.Shared.Models;

namespace GeoAuth.Shared.Extensions;

public static class MappableBaseExtensions
{
    public static Guid GetGuid<T>(this IMappable<T> target, string? value) => string.IsNullOrWhiteSpace(value) || !Guid.TryParse(value, out var id) ? Guid.Empty : id;
}
