using Azure.Data.Tables;
using GeoAuth.Infrastructure.Azure.Extensions;
using GeoAuth.Infrastructure.Azure.Models;
using GeoAuth.Infrastructure.Filters;
using GeoAuth.Infrastructure.Models;
using GeoAuth.Infrastructure.Repositories;
using GeoAuth.Shared;
using GeoAuth.Shared.Extensions;
using Microsoft.Extensions.DependencyInjection;

namespace GeoAuth.Infrastructure.Azure.Repositories;

internal class MachineRepository([FromKeyedServices(KeyedServices.MachineTable)] TableClient machineTableClient) : IMachineRepository
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

    public async Task<IEnumerable<MachineData>> FindAsync<TFilter>(TFilter filter, CancellationToken cancellationToken) where TFilter : IFilter
    {
        var machineDataList = new List<MachineData>();
        var request = ToFilter(filter) ?? throw new InvalidCastException($"Expected {nameof(MachineDataFilter)} recieved {filter.GetType().Name}");

        var query = $"PartitionKey eq '{request.MachineId}' AND Secret eq '{request.Secret?.Base64Encode()}'";
        var results = machineTableClient.QueryAsync<DbMachineData>(query, 1,
            cancellationToken: cancellationToken);

        await foreach (var result in results)
        {
            machineDataList.Add(result.Map<MachineData>());
        }

        return machineDataList;
    }

    public async Task<MachineData?> GetAsync<TFilter>(TFilter filter, CancellationToken cancellationToken) where TFilter : IFilter
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
