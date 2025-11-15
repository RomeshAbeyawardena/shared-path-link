using Azure.Data.Tables;
using GeoAuth.Infrastructure.Azure.Extensions;
using GeoAuth.Infrastructure.Repositories;
using GeoAuth.Shared.Models;

namespace GeoAuth.Infrastructure.Azure.Repositories
{
    internal abstract class AzureTableRepositoryBase<T,TDb, TContract>(TableClient tableClient) : RepositoryBase<T, TDb>
        where T : IMappable<TContract>, TContract
        where TDb : class, IMappable<TContract>, ITableEntity, TContract
    {
        public override async Task<IEnumerable<T>> FindAsync<TFilter>(TFilter filter, CancellationToken cancellationToken)
        {
            var resultList = new List<T>();

            var results = tableClient.QueryAsync(BuildExpression(filter), 1,
                cancellationToken: cancellationToken);

            await foreach (var result in results)
            {
                resultList.Add(result.Map<T>());
            }

            return resultList;
        }

        public override async Task<T?> GetAsync<TFilter>(TFilter filter, CancellationToken cancellationToken)
        {
            var result = await tableClient.QueryAsync(BuildExpression(filter), 1,
            cancellationToken: cancellationToken).FirstOrDefaultAsync(cancellationToken);

            if (result is not null)
            {
                return result.Map<T>();
            }

            return default;
        }
    }
}
