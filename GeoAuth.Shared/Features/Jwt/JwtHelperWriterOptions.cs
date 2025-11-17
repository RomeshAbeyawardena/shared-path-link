namespace GeoAuth.Shared.Features.Jwt;

public record JwtHelperWriterOptions(bool EncryptToken = false, 
    IssuerAudienceOptions? IssuerAudienceOptions = null, 
    int? DefaultMaximumTokenLifetime = null) : IJwtHelperOptions
{
    bool IJwtHelperOptions.SupportsEncryption => EncryptToken;
}
