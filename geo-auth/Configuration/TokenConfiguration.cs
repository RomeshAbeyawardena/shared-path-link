using GeoAuth.Shared.Features.Jwt;

namespace geo_auth.Configuration;

public record TokenConfiguration : ITokenConfiguration
{
    public string? ValidAudience { get; init; }
    public string? ValidIssuer { get; init; }
    public string? EncryptionKey { get; init; }
    public string? EncryptionKeyId { get; init; }
    public string? SigningKeyId { get; init; }
    public string? SigningKey { get; init; }
    public int? MaximumTokenLifetime { get; init; }
};
