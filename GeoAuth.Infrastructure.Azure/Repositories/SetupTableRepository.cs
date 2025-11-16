using Azure.Data.Tables;
using GeoAuth.Infrastructure.Azure.Models;
using GeoAuth.Shared;
using GeoAuth.Shared.Features.Setup;
using Microsoft.Extensions.DependencyInjection;
using System.Linq.Expressions;

namespace GeoAuth.Infrastructure.Azure.Repositories;

internal class SetupTableRepository([FromKeyedServices(KeyedServices.SetupTable)]TableClient setupTableClient) 
    : AzureTableRepositoryBase<Infrastructure.Models.SetupEntity, DbSetup, ISetupEntity>(setupTableClient)
{
    protected override Expression<Func<DbSetup, bool>> BuildExpression<TFilter>(TFilter filter)
    {
        throw new NotImplementedException();
    }
}
