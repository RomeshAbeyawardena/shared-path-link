using Azure.Data.Tables;
using geo_auth.Extensions;
using geo_auth.Models;
using GeoAuth.Shared.Requests.MachineToken;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace geo_auth.Handlers.MachineTokens;

internal class GetValidMachineAccessTokenQueryHandler([FromKeyedServices(KeyedServices.MachineAccessTokenTable)] TableClient machineAccessTokenTableClient,
    TimeProvider timeProvider) : IRequestHandler<GetValidMachineAccessTokenQuery, MachineAccessToken?>
{
    public async Task<MachineAccessToken?> Handle(GetValidMachineAccessTokenQuery request, CancellationToken cancellationToken)
    {
        var utcNowDate = timeProvider.GetUtcNow().UtcDateTime;
        var result = await machineAccessTokenTableClient.QueryAsync<MachineDataAccessToken>(
            $"PartitionKey eq '{request.PartitionKey}' AND ValidFrom le datetime'{utcNowDate:O}' AND Expires ge datetime'{utcNowDate:O}'", 1, cancellationToken: cancellationToken)
                .FirstOrDefaultAsync(cancellationToken);
        
        if (result is null)
        {
            return null;
        }

        return result.Map<MachineAccessToken>();

    }
}
