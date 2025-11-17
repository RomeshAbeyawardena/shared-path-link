namespace GeoAuth.Shared.Features.Jwt;

public record IssuerAudienceOptions(string? AudienceOverride = null, string? IssuerOverride = null);
