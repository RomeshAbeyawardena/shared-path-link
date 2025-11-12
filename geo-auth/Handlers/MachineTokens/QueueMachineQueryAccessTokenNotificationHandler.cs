using Azure.Storage.Queues;
using GeoAuth.Shared.Requests.MachineToken;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using System.Text.Json;

namespace geo_auth.Handlers.MachineTokens
{
    internal class QueueMachineQueryAccessTokenNotificationHandler([FromKeyedServices("machine-access-token")] QueueClient queueClient) : INotificationHandler<QueueMachineQueryAccessTokenNotification>
    {
        public async Task Handle(QueueMachineQueryAccessTokenNotification notification, CancellationToken cancellationToken)
        {
            using var stream = new MemoryStream();
            await JsonSerializer.SerializeAsync(stream, notification, cancellationToken: cancellationToken);
            using var textReader = new StreamReader(stream);
            await queueClient.SendMessageAsync(textReader.ReadToEnd(), cancellationToken);
        }
    }
}
