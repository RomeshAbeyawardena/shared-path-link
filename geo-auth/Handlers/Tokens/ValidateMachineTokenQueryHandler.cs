using GeoAuth.Shared.Exceptions;
using GeoAuth.Shared.Requests.Tokens;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Logging;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;

namespace geo_auth.Handlers.Tokens;

internal class ValidateMachineTokenQueryHandler(IOptions<TokenConfiguration> tokenConfigurationOptions, 
    ILogger<ValidateMachineTokenQueryHandler> logger) : IRequestHandler<ValidateMachineTokenQuery, MachineTokenQueryResult>
{
    public async Task<MachineTokenQueryResult> Handle(ValidateMachineTokenQuery request, CancellationToken cancellationToken)
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

            var failureException = new ResponseException("Token is invalid!", StatusCodes.Status406NotAcceptable, token.Exception); ;

            if (!token.IsValid)
            {
            #if DEBUG
                IdentityModelEventSource.ShowPII = true;
#endif
                throw failureException;
            }

            if (!token.Claims.TryGetValue("machineId", out var machineId) || !Guid.TryParse(machineId?.ToString(), out var machId))
            {
                throw failureException;
            }

            if (!token.Claims.TryGetValue("secret", out var secret))
            {
                throw failureException;
            }

            return new MachineTokenQueryResult(
                new MachineTokenQueryResponse(machId, secret?.ToString()));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unable to validate user token");
            return new MachineTokenQueryResult(null, ex);
        }
    }
}
