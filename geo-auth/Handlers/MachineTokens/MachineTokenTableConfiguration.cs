namespace geo_auth.Handlers.MachineTokens;

internal record MachineTokenTableConfiguration
{
    public string? MachineTokenTableName { get; set; }
    public string? MachineAccessTokenTableName { get; set; }
    public string? MachineAccessTokenQueueName { get; set; }
}
