namespace geo_auth.Handlers.Tokens;

public record TokenConfiguration(string? ValidAudience,
    string? ValidIssuer,
    string? SigningKeyId,
    string? SigningKey);
