namespace GeoAuth.Infrastructure.Azure.Configuration;

public record MachineTokenTableConfiguration
{
    public string? MachineTokenTableName { get; set; }
    public string? MachineAccessTokenTableName { get; set; }
    public string? MachineAccessTokenQueueName { get; set; }
}
