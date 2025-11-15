using Azure.Data.Tables;
using GeoAuth.Infrastructure.Azure.Models;
using GeoAuth.Infrastructure.Filters;
using GeoAuth.Infrastructure.Repositories;
using GeoAuth.Shared;
using GeoAuth.Shared.Requests.MachineToken;
using Microsoft.Extensions.DependencyInjection;
using System.Data.Entity.Core.Common.CommandTrees.ExpressionBuilder;
using System.Linq.Expressions;

namespace GeoAuth.Infrastructure.Azure.Repositories;

internal class MachineAccessTokenRepository([FromKeyedServices(KeyedServices.MachineAccessTokenTable)] TableClient machineAccessTokenTableClient) 
    : AzureTableRepositoryBase<MachineAccessToken, DbMachineDataAccessToken, IMachineAccessToken>(machineAccessTokenTableClient), IMachineAccessTokenRepository
{
    public static MachineAccessTokenFilter? ToFilter(IFilter filter)
    {
        if (filter is not null && filter is MachineAccessTokenFilter machineDataFilter)
        {
            return machineDataFilter;
        }

        return null;
    }

    protected override Expression<Func<DbMachineDataAccessToken, bool>> BuildExpression<TFilter>(TFilter filter)
    {
        var request = ToFilter(filter) ?? throw new InvalidCastException($"Expected {nameof(MachineDataFilter)} recieved {filter.GetType().Name}");

        var expressionBuilder = ExpressionBuilder;

        if (request.RowKey.HasValue)
        {
            expressionBuilder.And(x => x.RowKey == request.RowKey.ToString());
        }

        if (request.PartitionKey.HasValue)
        {
            expressionBuilder.And(x => x.PartitionKey == request.PartitionKey.ToString());
        }

        bool useSeparateExpression = request.FromDate.HasValue && request.ToDate.HasValue;
        //supports condition grouping
        var target = useSeparateExpression ? ExpressionBuilder : expressionBuilder;

        if (request.FromDate.HasValue)
        {
            target.And(x => x.ValidFrom <=  request.FromDate.Value);
        }

        if (request.ToDate.HasValue)
        {
            target.And(x => x.Expires >= request.ToDate.Value);
        }

        if (useSeparateExpression)
        {
            expressionBuilder.And(target);
        }

        return ExpressionBuilder;
    }
}
