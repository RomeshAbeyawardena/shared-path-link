namespace geo_auth.Features.BeginAuthentication;

public record JwtHelperWriterOptions(bool EncryptToken = false, 
    IssuerAudienceOptions? IssuerAudienceOptions = null, 
    int? DefaultMaximumTokenLifetime = null) : IJwtHelperOptions
{
    bool IJwtHelperOptions.SupportsEncryption => EncryptToken;
}
