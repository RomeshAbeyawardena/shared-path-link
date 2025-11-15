using Azure.Data.Tables;
using GeoAuth.Infrastructure.Azure.Extensions;
using GeoAuth.Infrastructure.Repositories;
using GeoAuth.Shared.Models;
using LinqKit;

namespace GeoAuth.Infrastructure.Azure.Repositories
{
    internal abstract class AzureTableRepositoryBase<T,TDb, TContract>(TableClient tableClient) : RepositoryBase<T, TDb>
        where T : IMappable<TContract>, TContract
        where TDb : class, IMappable<TContract>, ITableEntity, TContract
    {
        protected static ExpressionStarter<TDb> ExpressionBuilder => PredicateBuilder.New<TDb>();

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

        public override async Task<IResult<int>> UpsertAsync(T entry, CancellationToken cancellationToken)
        {
            using var result = await tableClient.UpsertEntityAsync(entry.Map<TDb>(), cancellationToken: cancellationToken);

            if (result.IsError)
            {
                return Result.Failed<int>(new Exception($"{result.Status}: {result.ReasonPhrase}"));
            }

            return Result.Sucessful(result.Status);
        }
    }
}
