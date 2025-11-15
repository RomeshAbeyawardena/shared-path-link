using GeoAuth.Infrastructure.Filters;
using GeoAuth.Infrastructure.Repositories;
using GeoAuth.Shared.Requests.MachineToken;
using MediatR;

namespace geo_auth.Handlers.MachineTokens;

internal class GetValidMachineAccessTokenQueryHandler(IMachineAccessTokenRepository machineAccessTokenRepository,
    TimeProvider timeProvider) : IRequestHandler<GetValidMachineAccessTokenQuery, MachineAccessToken?>
{
    public async Task<MachineAccessToken?> Handle(GetValidMachineAccessTokenQuery request, CancellationToken cancellationToken)
    {
        var utcNowDate = timeProvider.GetUtcNow().UtcDateTime;

        var result = await machineAccessTokenRepository.GetAsync(new MachineAccessTokenFilter
        {
            FromDate = utcNowDate,
            ToDate = utcNowDate,
            PartitionKey = request.PartitionKey
        }, cancellationToken);

        if (result is null)
        {
            return null;
        }

        return result.Map<MachineAccessToken>();

    }
}
