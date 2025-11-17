using GeoAuth.Shared.Models;
using Microsoft.IdentityModel.Tokens;

namespace geo_auth.Features.BeginAuthentication;

public interface IJwtHelper
{
    ValueTask<IResult<TToken>> ReadToken<TToken>(string token, TokenValidationParameters tokenValidationParameters);
    IResult<string> WriteToken<TToken>(TToken model, JwtHelperWriterOptions options);
}
