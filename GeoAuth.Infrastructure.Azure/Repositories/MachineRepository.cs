using Azure.Data.Tables;
using GeoAuth.Infrastructure.Azure.Extensions;
using GeoAuth.Infrastructure.Azure.Models;
using GeoAuth.Infrastructure.Filters;
using GeoAuth.Infrastructure.Models;
using GeoAuth.Infrastructure.Repositories;
using GeoAuth.Shared;
using GeoAuth.Shared.Extensions;
using GeoAuth.Shared.Models;
using Microsoft.Extensions.DependencyInjection;
using System.Linq.Expressions;

namespace GeoAuth.Infrastructure.Azure.Repositories;

internal class MachineRepository([FromKeyedServices(KeyedServices.MachineTable)] TableClient machineTableClient) 
    : AzureTableRepositoryBase<MachineData, DbMachineData, IMachineData>(machineTableClient), IMachineRepository
{
    private static MachineDataFilter? ToFilter(IFilter filter)
    {
        if (filter is not null && filter is MachineDataFilter machineDataFilter)
        {
            return machineDataFilter;
        }

        return null;
    }

    protected override Expression<Func<DbMachineData, bool>> BuildExpression<TFilter>(TFilter filter)
    {
        var request = ToFilter(filter) ?? throw new InvalidCastException($"Expected {nameof(MachineDataFilter)} recieved {filter.GetType().Name}");
        var expressionBuilder = ExpressionBuilder;
        expressionBuilder.Start(x => true);

        if (request.Id.HasValue)
        {
            expressionBuilder.And(x => x.RowKey == request.Id.ToString());
        }

        if (request.MachineId.HasValue)
        {
            expressionBuilder.And(x => x.PartitionKey == request.MachineId.ToString());
        }

        if (!string.IsNullOrWhiteSpace(request.Secret))
        {
            expressionBuilder.And(x => x.Secret == request.Secret);
        }

        return expressionBuilder;
    }
}
