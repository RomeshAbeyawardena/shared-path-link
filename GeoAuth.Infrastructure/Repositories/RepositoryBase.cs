
using System.Linq.Expressions;

namespace GeoAuth.Infrastructure.Repositories;

public abstract class RepositoryBase<T, TDb> : IRepository<T>
{
    protected abstract Expression<Func<TDb, bool>> BuildExpression<TFilter>(TFilter filter)
        where TFilter : IFilter;
    public abstract Task<IEnumerable<T>> FindAsync<TFilter>(TFilter filter, CancellationToken cancellationToken) where TFilter : IFilter;
    public abstract Task<T?> GetAsync<TFilter>(TFilter filter, CancellationToken cancellationToken) where TFilter : IFilter;
}
