using geo_auth.Configuration;
using GeoAuth.Shared.Exceptions;
using GeoAuth.Shared.Models;
using GeoAuth.Shared.Requests.Tokens;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Logging;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace geo_auth.Handlers.Tokens;

internal class ValidateUserQueryHandler(IOptions<TokenConfiguration> tokenConfigurationOptions,
    ILogger<ValidateUserQueryHandler> logger) : IRequestHandler<ValidateUserQuery, UserResult>
{
    public async Task<UserResult> Handle(ValidateUserQuery request, CancellationToken cancellationToken)
    {
        var tokenConfiguration = tokenConfigurationOptions.Value;

        try
        {
            var signingKey = tokenConfiguration.SigningKey ?? throw new ResponseException("Signing key missing", StatusCodes.Status500InternalServerError);
            var key = new SymmetricSecurityKey(Convert.FromBase64String(signingKey))
            {
                KeyId = tokenConfiguration.SigningKeyId ?? throw new ResponseException("Signing key ID missing", StatusCodes.Status500InternalServerError)
            };

            var token = await new JwtSecurityTokenHandler().ValidateTokenAsync(request.Token, new TokenValidationParameters
            {
                ValidateAudience = true,
                ValidAudience = tokenConfiguration.ValidAudience,
                ValidateIssuer = true,
                ValidIssuer = tokenConfiguration.ValidIssuer,
                ValidateIssuerSigningKey = true,
                ValidAlgorithms = [SecurityAlgorithms.HmacSha256],
                IssuerSigningKey = key,
                //ValidateTokenReplay = true
            });

            var user = new User();

            if (!token.IsValid)
            {
#if DEBUG
                IdentityModelEventSource.ShowPII = true;
#endif
                throw new ResponseException("Token is invalid!", StatusCodes.Status406NotAcceptable, token.Exception);
            }

            if (token.Claims.TryGetValue("clientId", out var clientId) && Guid.TryParse(clientId?.ToString(), out var cid))
            {
                user.ClientId = cid;
            }

            if (token.Claims.TryGetValue(ClaimTypes.NameIdentifier, out var sub) && Guid.TryParse(sub?.ToString(), out var sid))
            {
                user.Id = sid;
            }

            if (token.Claims.TryGetValue(ClaimTypes.Email, out var email))
            {
                user.Email = email?.ToString();
            }

            if (token.Claims.TryGetValue("name", out var name))
            {
                user.Name = name?.ToString();
            }

            if (token.Claims.TryGetValue("secret", out var secret))
            {
                user.Secret = secret?.ToString();
            }

            if (token.Claims.TryGetValue("salt", out var salt))
            {
                user.Salt = salt?.ToString();
            }

            return new UserResult(user);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unable to validate user token");
            return new UserResult(null, ex);
        }
    }
}
