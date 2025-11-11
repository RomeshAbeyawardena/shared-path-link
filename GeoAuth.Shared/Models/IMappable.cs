namespace GeoAuth.Shared.Models;

public interface IMappable<T> : ISingularMappable<T>
{
    TResult Map<TResult>(Func<TResult>? instanceFactory = null) where TResult : IMappable<T>;
}
