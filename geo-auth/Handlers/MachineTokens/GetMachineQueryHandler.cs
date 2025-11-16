using GeoAuth.Infrastructure.Filters;
using GeoAuth.Infrastructure.Repositories;
using GeoAuth.Shared.Features.Machines;
using GeoAuth.Shared.Models;
using MediatR;

namespace geo_auth.Handlers.MachineTokens
{
    internal class GetMachineQueryHandler(IMachineRepository machineRepository) 
        : IRequestHandler<GetMachineQuery, IResult<GeoAuth.Shared.Features.Machines.MachineData>>
    {
        public async Task<IResult<GeoAuth.Shared.Features.Machines.MachineData>> Handle(GetMachineQuery request, CancellationToken cancellationToken)
        {
            var result = await machineRepository.GetAsync(new MachineDataFilter
            {

            }, cancellationToken);

            if (result is null)
            {
                return Result.Failed<GeoAuth.Shared.Features.Machines.MachineData>(new NullReferenceException("Entity not found"));
            }

            return Result.Sucessful(result.Map<GeoAuth.Shared.Features.Machines.MachineData>());
        }
    }
}
