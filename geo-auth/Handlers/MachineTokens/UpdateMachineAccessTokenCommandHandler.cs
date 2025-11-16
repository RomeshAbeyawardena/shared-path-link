using GeoAuth.Infrastructure.Repositories;
using GeoAuth.Shared.Requests.MachineToken;
using MediatR;
using Microsoft.Extensions.Logging;

namespace geo_auth.Handlers.MachineTokens
{
    internal class UpdateMachineAccessTokenCommandHandler(ILogger<UpdateMachineAccessTokenCommandHandler> logger,
        IMachineAccessTokenRepository machineAccessTokenRepository,
        TimeProvider timeProvider) 
        : IRequestHandler<UpdateMachineAccessTokenCommand>
    {
        public async Task Handle(UpdateMachineAccessTokenCommand notification, CancellationToken cancellationToken)
        {
            var response = await machineAccessTokenRepository.UpsertAsync(new MachineAccessToken
            {
                MachineId = notification.PartitionKey,
                Id = Guid.NewGuid().ToString(),
                Token = notification.Token,
                ValidFrom = notification.ValidFrom,
                Expires = notification.Expires,
                Timestamp = timeProvider.GetUtcNow(),
            }, cancellationToken);

            if (!response.IsSuccess)
            {
                logger.LogError(response.Exception, "Unable to amend access token record");
            }
        }
    }
}
