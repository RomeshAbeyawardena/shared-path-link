using Azure;
using Azure.Data.Tables;
using geo_auth.Models;
using GeoAuth.Shared.Requests.MachineToken;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace geo_auth.Handlers.MachineTokens
{
    internal class UpdateMachineQueryAccessTokenNotificationHandler([FromKeyedServices("machine-access-token")] TableClient machineAccessTokenTableClient,
        TimeProvider timeProvider) 
        : INotificationHandler<UpdateMachineQueryAccessTokenNotification>
    {
        public async Task Handle(UpdateMachineQueryAccessTokenNotification notification, CancellationToken cancellationToken)
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
                
            }
        }
    }
}
