using Azure;
using Azure.Data.Tables;
using GeoAuth.Infrastructure.Azure.Extensions;
using GeoAuth.Infrastructure.Repositories;
using GeoAuth.Shared.Models;
using LinqKit;

namespace GeoAuth.Infrastructure.Azure.Repositories
{
    internal abstract class AzureTableRepositoryBase<T,TDb, TContract>(TableClient tableClient, TimeProvider timeProvider) 
        : RepositoryBase<T, TDb>
        , ICreateableRepository
        where T : IMappable<TContract>, TContract
        where TDb : class, IMappable<TContract>, ITableEntity, TContract
    {
        protected static ExpressionStarter<TDb> ExpressionBuilder => PredicateBuilder.New<TDb>();

        public async Task<IResult<int>> CreateIfNotExistsAsync(CancellationToken cancellationToken)
        {
            var result = await tableClient.CreateIfNotExistsAsync(cancellationToken);
            var rawResponse = result.GetRawResponse();
            if (rawResponse.IsError)
            {
                return Result.Failed<int>(new Exception(rawResponse.ReasonPhrase));
            }

            return Result.Sucessful(rawResponse.Status);
        }

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
            var mappedEntry = entry.Map<TDb>();

            mappedEntry.Id ??= Guid.NewGuid().ToString();

            mappedEntry.Timestamp = timeProvider.GetUtcNow();
            mappedEntry.ETag = ETag.All;

            using var result = await tableClient.UpsertEntityAsync(mappedEntry, cancellationToken: cancellationToken);

            if (result.IsError)
            {
                return Result.Failed<int>(new Exception($"{result.Status}: {result.ReasonPhrase}"));
            }

            return Result.Sucessful(result.Status);
        }
    }
}
