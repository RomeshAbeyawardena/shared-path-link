using Azure.Data.Tables;
using GeoAuth.Infrastructure.Azure.Extensions;
using GeoAuth.Infrastructure.Azure.Models;
using GeoAuth.Infrastructure.Filters;
using GeoAuth.Infrastructure.Models;
using GeoAuth.Infrastructure.Repositories;
using GeoAuth.Shared;
using GeoAuth.Shared.Extensions;
using LinqKit;
using Microsoft.Extensions.DependencyInjection;
using System.Linq.Expressions;

namespace GeoAuth.Infrastructure.Azure.Repositories;

internal class MachineRepository([FromKeyedServices(KeyedServices.MachineTable)] TableClient machineTableClient) : RepositoryBase<MachineData, DbMachineData>, IMachineRepository
{
    private static MachineDataFilter? ToFilter<TFilter>(TFilter filter)
        where TFilter : IFilter
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
        var expressionBuilder = PredicateBuilder.New<DbMachineData>();
        expressionBuilder.Start(x => true);

        if (request.RowKey.HasValue)
        {
            expressionBuilder.And(x => x.RowKey == request.RowKey.ToString());
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

    public override async Task<IEnumerable<MachineData>> FindAsync<TFilter>(TFilter filter, CancellationToken cancellationToken)
    {
        var machineDataList = new List<MachineData>();
        
        var results = machineTableClient.QueryAsync(BuildExpression(filter), 1,
            cancellationToken: cancellationToken);

        await foreach (var result in results)
        {
            machineDataList.Add(result.Map<MachineData>());
        }

        return machineDataList;
    }

    public override async Task<MachineData?> GetAsync<TFilter>(TFilter filter, CancellationToken cancellationToken)
    {
        var request = ToFilter(filter) ?? throw new InvalidCastException($"Expected {nameof(MachineDataFilter)} recieved {filter.GetType().Name}");

        var query = $"PartitionKey eq '{request.MachineId}' AND Secret eq '{request.Secret?.Base64Encode()}'";
        var result = await machineTableClient.QueryAsync<DbMachineData>(query, 1,
            cancellationToken: cancellationToken).FirstOrDefaultAsync(cancellationToken);

        if (result is not null)
        {
            return result.Map<MachineData>();
        }

        return null;
    }
}
