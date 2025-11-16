using geo_auth.Handlers.Tokens;

namespace geo_auth.Features.BeginAuthentication;

public record AuthTokenResult(string? Token) : IAuthTokenResult
{
}
