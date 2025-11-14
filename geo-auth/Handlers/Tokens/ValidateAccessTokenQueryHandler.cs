using GeoAuth.Shared.Exceptions;
using GeoAuth.Shared.Requests.Tokens;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;

namespace geo_auth.Handlers.Tokens;

internal class ValidateAccessTokenQueryHandler(IOptions<TokenConfiguration> tokenConfigurationOptions) : IRequestHandler<ValidateAccessTokenQuery, ValidateAccessTokenResponse>
{
    public async Task<ValidateAccessTokenResponse> Handle(ValidateAccessTokenQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var tokenConfiguration = tokenConfigurationOptions.Value;

            var signingKey = tokenConfiguration.SigningKey ?? throw new ResponseException("Signing key missing", StatusCodes.Status500InternalServerError);
            var key = new SymmetricSecurityKey(Convert.FromBase64String(signingKey))
            {
                KeyId = tokenConfiguration.SigningKeyId ?? throw new ResponseException("Signing key ID missing", StatusCodes.Status500InternalServerError)
            };

            var jwtTokenHandler = new JwtSecurityTokenHandler();
            var result = await jwtTokenHandler.ValidateTokenAsync(request.Token, new TokenValidationParameters
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

            if (!result.IsValid)
            {
                throw new ResponseException("Token is invalid", StatusCodes.Status401Unauthorized);
            }

            if (!result.Claims.TryGetValue("parition-key", out var paritionKey))
            {
                throw new ResponseException("Token is invalid", StatusCodes.Status401Unauthorized);
            }

            if (!result.Claims.TryGetValue("row-key", out var rowKey))
            {
                throw new ResponseException("Token is invalid", StatusCodes.Status401Unauthorized);
            }
            var scopeList = new List<string>();
            if (result.Claims.TryGetValue("scopes", out var scopes))
            {
                var scopesResult = scopes?.ToString();
                if (!string.IsNullOrWhiteSpace(scopesResult))
                {
                    scopeList.AddRange(scopesResult.Split(','));
                }
            }

            return new ValidateAccessTokenResponse(new ValidateAccessTokenResult
            {
                Scopes = scopeList
            });
        }
        catch (Exception exception)
        {
            return new ValidateAccessTokenResponse(null, exception);
        }
    }
}
