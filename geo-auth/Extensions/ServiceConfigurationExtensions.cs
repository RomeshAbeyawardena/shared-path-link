using Azure.Data.Tables;
using Azure.Storage.Queues;
using GeoAuth.Shared;

namespace geo_auth.Extensions;

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
