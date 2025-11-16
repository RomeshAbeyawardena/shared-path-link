using GeoAuth.Shared.Models;

namespace GeoAuth.Infrastructure;

public interface IRepository<T>
{
    Task<IResult<int>> UpsertAsync(T entry, CancellationToken cancellationToken);

    Task<T?> GetAsync<TFilter>(TFilter filter, CancellationToken cancellationToken)
        where TFilter: IFilter;

    Task<IEnumerable<T>> FindAsync<TFilter>(TFilter filter, CancellationToken cancellationToken)
        where TFilter : IFilter;
}

public interface ICreateableRepository
{
    Task<IResult<int>> CreateIfNotExistsAsync(CancellationToken cancellationToken);
}