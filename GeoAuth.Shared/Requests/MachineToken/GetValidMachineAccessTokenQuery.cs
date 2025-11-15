using MediatR;

namespace GeoAuth.Shared.Requests.MachineToken
{
    public record GetValidMachineAccessTokenQuery(Guid PartitionKey) : IRequest<MachineAccessToken?>
    {
    }
}
