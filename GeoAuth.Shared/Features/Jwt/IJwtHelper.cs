using GeoAuth.Shared.Models;
using Microsoft.IdentityModel.Tokens;

namespace GeoAuth.Shared.Features.Jwt;

public interface IJwtHelper
{
    ValueTask<IResult<TToken>> ReadTokenAsync<TToken>(string token, TokenValidationParameters tokenValidationParameters);
    IResult<string> WriteToken<TToken>(TToken model, JwtHelperWriterOptions options);
    TokenValidationParameters DefaultParameters(bool requiresSignedCredentials = false, bool requiresEncryptionCredentials = false);
}
