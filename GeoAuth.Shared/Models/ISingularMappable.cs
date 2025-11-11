namespace GeoAuth.Shared.Models;

public interface ISingularMappable<T>
{
    void Map(T source);
}
