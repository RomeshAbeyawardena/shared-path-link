namespace GeoAuth.Shared.Features.Jwt;

public interface ITokenConfiguration
{
    string? SigningKey { get; }
    string? SigningKeyId { get; }
    string? EncryptionKey { get; }
    string? EncryptionKeyId { get; }
    string? ValidAudience { get; }
    string? ValidIssuer { get; }
    int? MaximumTokenLifetime { get; }
}
