using Azure.Storage.Queues;
using GeoAuth.Shared.Requests.MachineToken;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using System.Text.Json;

namespace geo_auth.Handlers.MachineTokens
{
    internal class QueueMachineQueryAccessTokenNotificationHandler([FromKeyedServices("machine-access-token")] QueueClient queueClient,
        JsonSerializerOptions jsonSerializerOptions) 
        : INotificationHandler<QueueMachineAccessTokenNotification>
    {
        public async Task Handle(QueueMachineAccessTokenNotification notification, CancellationToken cancellationToken)
        {
            using var stream = new MemoryStream();
            await JsonSerializer.SerializeAsync(stream, notification, jsonSerializerOptions, cancellationToken);
            stream.Position = 0;
            using var textReader = new StreamReader(stream);
            await queueClient.SendMessageAsync(textReader.ReadToEnd(), cancellationToken);
        }
    }
}
