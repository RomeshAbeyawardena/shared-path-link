namespace geo_auth.Configuration;

internal record PasswordConfiguration
{
    public string? KnownSecret { get; init; }
    public int? DegreeOfParallelism { get; init; }
    public int? MemorySize { get; init; }
    public int? Iterations { get; init; }
    public int? KeySize { get; init; }
    public int? SaltSize { get; init; }
};
