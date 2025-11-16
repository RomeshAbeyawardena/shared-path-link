namespace geo_auth.Configuration;

public record TokenConfiguration
{
    public string? ValidAudience { get; init; }
    public string? ValidIssuer { get; init; }
    public string? SigningKeyId { get; init; }
    public string? SigningKey { get; init; }
    public int? MaximumTokenLifetime { get; init; }
};
