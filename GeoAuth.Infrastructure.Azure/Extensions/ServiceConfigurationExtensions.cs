using Azure.Data.Tables;
using Azure.Storage.Queues;
using GeoAuth.Shared;

namespace GeoAuth.Infrastructure.Azure.Extensions;

public static class ServiceConfigurationExtensions
{
    public static Type GetServiceType(this ServiceConfiguration serviceConfiguration) =>
        serviceConfiguration.Type switch
        {
            ClientType.Table => typeof(TableClient),
            ClientType.Queue => typeof(QueueClient),
            _ => throw new ArgumentOutOfRangeException(nameof(serviceConfiguration), "Invalid client type"),
        };
}
