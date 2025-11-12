using MediatR;

namespace GeoAuth.Shared.Requests.MachineToken
{
    public record GetValidMachineAccessTokenQuery(string PartitionKey) : IRequest<MachineAccessToken?>
    {
    }
}
