namespace GeoAuth.Infrastructure;

public interface IRepository<T>
{
    Task<T?> GetAsync<TFilter>(TFilter filter, CancellationToken cancellationToken)
        where TFilter: IFilter;

    Task<IEnumerable<T>> FindAsync<TFilter>(TFilter filter, CancellationToken cancellationToken)
        where TFilter : IFilter;
}
