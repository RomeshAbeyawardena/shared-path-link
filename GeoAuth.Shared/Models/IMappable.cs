namespace GeoAuth.Shared.Models;

public interface IMappable<T> : ISingularMappable<T>
{
    TResult Map<TResult>() where TResult : IMappable<T>, new();
    TResult Map<TResult>(Func<TResult> instanceFactory) where TResult : IMappable<T>;
}
