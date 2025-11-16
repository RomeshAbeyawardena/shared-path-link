using GeoAuth.Shared.Models;
using MediatR;

namespace GeoAuth.Shared.Features.Machines;

public record GetMachineQuery : IRequest<IResult<MachineData>>
{
    public Guid? Id { get; init; }
    public Guid? MachineId { get; init; }
}
