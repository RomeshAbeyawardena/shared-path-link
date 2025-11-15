using Azure.Data.Tables;
using GeoAuth.Infrastructure.Azure.Models;
using GeoAuth.Infrastructure.Filters;
using GeoAuth.Infrastructure.Repositories;
using GeoAuth.Shared;
using GeoAuth.Shared.Requests.MachineToken;
using Microsoft.Extensions.DependencyInjection;
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
        
        if (request.PartitionKey.HasValue)
        {

        }

        if (request.RowKey.HasValue)
        {

        }

        if (request.FromDate.HasValue)
        {

        }

        if (request.ToDate.HasValue)
        {

        }
    }
}
