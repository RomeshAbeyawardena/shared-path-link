using GeoAuth.Shared.Models;

namespace GeoAuth.Shared.Extensions;

public static class MappableBaseExtensions
{
#pragma warning disable IDE0060 //Target is not needed here.
    public static Guid? GetGuid<T>(this IMappable<T> target, string? value) => string.IsNullOrWhiteSpace(value) || !Guid.TryParse(value, out var id) ? null : id;
#pragma warning restore
}
