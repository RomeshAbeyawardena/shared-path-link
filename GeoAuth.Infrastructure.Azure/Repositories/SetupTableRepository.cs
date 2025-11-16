using Azure.Data.Tables;
using GeoAuth.Infrastructure.Azure.Models;
using GeoAuth.Infrastructure.Filters;
using GeoAuth.Infrastructure.Repositories;
using GeoAuth.Shared;
using GeoAuth.Shared.Features.Setup;
using Microsoft.Extensions.DependencyInjection;
using System.Linq.Expressions;

namespace GeoAuth.Infrastructure.Azure.Repositories;

internal class SetupTableRepository([FromKeyedServices(KeyedServices.SetupTable)]TableClient setupTableClient, TimeProvider timeProvider) 
    : AzureTableRepositoryBase<Infrastructure.Models.SetupEntity, DbSetup, ISetupEntity>(setupTableClient, timeProvider), ISetupRepository
{
    protected override Expression<Func<DbSetup, bool>> BuildExpression<TFilter>(TFilter filter)
    {
        if (filter is not SetupFilter setupFilter)
        {
            throw new InvalidCastException($"Unable to cast {nameof(IFilter)} to {nameof(SetupFilter)}");
        }

        var expressionBuilder = ExpressionBuilder;

        if (!string.IsNullOrWhiteSpace(setupFilter.Key))
        {
            expressionBuilder.And(x => x.RowKey ==  setupFilter.Key);
        }

        if (!string.IsNullOrWhiteSpace(setupFilter.Type))
        {
            expressionBuilder.And(x => x.PartitionKey == setupFilter.Type);
        }

        return expressionBuilder;
    }
}
