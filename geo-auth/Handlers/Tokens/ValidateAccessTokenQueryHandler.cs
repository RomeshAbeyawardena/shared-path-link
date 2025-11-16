using geo_auth.Configuration;
using GeoAuth.Shared.Exceptions;
using GeoAuth.Shared.Features.Machines;
using GeoAuth.Shared.Requests.Tokens;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;

namespace geo_auth.Handlers.Tokens;

internal class ValidateAccessTokenQueryHandler(IOptions<TokenConfiguration> tokenConfigurationOptions, IMediator mediator) 
    : IRequestHandler<ValidateAccessTokenQuery, ValidateAccessTokenResponse>
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
                ValidIssuer = tokenConfiguration.ValidAudience,
                ValidateIssuerSigningKey = true,
                ValidAlgorithms = [SecurityAlgorithms.HmacSha256],
                IssuerSigningKey = key,
                //ValidateTokenReplay = true
            });

            var defaultException = new ResponseException("Token is invalid", StatusCodes.Status401Unauthorized);

            if (!result.IsValid)
            {
                throw defaultException;
            }

            if (!result.Claims.TryGetValue("machine-id", out var machineId) || !Guid.TryParse(machineId?.ToString(), out var id))
            {
                throw defaultException;
            }

            if (!result.Claims.TryGetValue("row-key", out var machineKey) || !Guid.TryParse(machineId?.ToString(), out var mKey))
            {
                throw defaultException;
            }

            var machine = await mediator.Send(new GetMachineQuery { Id = id, MachineId = mKey }, cancellationToken) ?? throw defaultException;
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
