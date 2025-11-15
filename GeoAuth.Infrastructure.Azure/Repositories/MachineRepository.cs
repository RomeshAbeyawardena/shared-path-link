using Azure.Data.Tables;
using GeoAuth.Infrastructure.Models;
using GeoAuth.Infrastructure.Repositories;
using GeoAuth.Shared;
using Microsoft.Extensions.DependencyInjection;

namespace GeoAuth.Infrastructure.Azure.Repositories;

internal class MachineRepository([FromKeyedServices(KeyedServices.MachineTable)] TableClient machineTableClient) : IMachineRepository
{
    public Task<IEnumerable<MachineData>> FindAsync<TFilter>(TFilter filter, CancellationToken cancellationToken) where TFilter : IFilter
    {
        throw new NotImplementedException();
    }

    public Task<MachineData> GetAsync<TFilter>(TFilter filter, CancellationToken cancellationToken) where TFilter : IFilter
    {
        throw new NotImplementedException();
    }
}
