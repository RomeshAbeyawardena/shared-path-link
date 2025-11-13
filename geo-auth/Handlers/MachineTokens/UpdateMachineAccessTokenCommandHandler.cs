using Azure;
using Azure.Data.Tables;
using geo_auth.Extensions;
using geo_auth.Models;
using GeoAuth.Shared.Requests.MachineToken;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace geo_auth.Handlers.MachineTokens
{
    internal class UpdateMachineAccessTokenCommandHandler(ILogger<UpdateMachineAccessTokenCommandHandler> logger,
        [FromKeyedServices(KeyedServices.MachineAccessTokenTable)] TableClient machineAccessTokenTableClient,
        TimeProvider timeProvider) 
        : IRequestHandler<UpdateMachineAccessTokenCommand>
    {
        public async Task Handle(UpdateMachineAccessTokenCommand notification, CancellationToken cancellationToken)
        {
            var response = await machineAccessTokenTableClient.AddEntityAsync(new MachineDataAccessToken
            {
                PartitionKey = notification.PartitionKey,
                RowKey = Guid.NewGuid().ToString(),
                Token = notification.Token,
                ValidFrom = notification.ValidFrom,
                Expires = notification.Expires,
                Timestamp = timeProvider.GetUtcNow(),
                ETag = ETag.All
            }, cancellationToken);

            if (response.IsError)
            {
                logger.LogError("{status}: {reason}", response.Status, response.ReasonPhrase);
            }
        }
    }
}
