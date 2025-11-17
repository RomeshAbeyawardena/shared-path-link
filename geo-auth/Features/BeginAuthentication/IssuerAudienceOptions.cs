namespace geo_auth.Features.BeginAuthentication;

public record IssuerAudienceOptions(string? AudienceOverride = null, string? IssuerOverride = null);
