using geo_auth.Configuration;
using geo_auth.Handlers.MachineTokens;
using GeoAuth.Infrastructure.Filters;
using GeoAuth.Infrastructure.Repositories;
using GeoAuth.Shared.Exceptions;
using GeoAuth.Shared.Extensions;
using GeoAuth.Shared.Features.BeginAuthentication;
using GeoAuth.Shared.Requests.MachineToken;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;

namespace geo_auth.Features.BeginAuthentication;

internal class AuthenticateMachineQueryHandler(IMachineRepository machineRepository,
    TimeProvider timeProvider, IOptions<TokenConfiguration> tokenConfigurationOptions,
    IMediator mediator)
    : IRequestHandler<AuthenticateMachineQuery, AuthenticateMachineResult>
{
    private string GenerateToken(AuthenticateMachineQuery query, MachineData request)
    {
        var tokenConfiguration = tokenConfigurationOptions.Value;

        var signingKey = tokenConfiguration.SigningKey ?? throw new ResponseException("Signing key missing", StatusCodes.Status500InternalServerError);
        var signingKeyBytes = Convert.FromBase64String(signingKey);

        Span<byte> bytes = new(signingKeyBytes);
        var keyBytes = bytes.Slice(0, 32);

        var key = new SymmetricSecurityKey(keyBytes.ToArray())
        {
            KeyId = tokenConfiguration.SigningKeyId ?? throw new ResponseException("Signing key ID missing", StatusCodes.Status500InternalServerError)
        };
        var utcNow = timeProvider.GetUtcNow();

        var descriptor = new SecurityTokenDescriptor();
        descriptor.Issuer = tokenConfiguration.ValidAudience;
        descriptor.Audience = tokenConfiguration.ValidAudience;
        descriptor.Claims = new Dictionary<string, object>
                {
                    { "machine-id", request.MachineId },
                    { "row-key", request.Id },
                    { "scopes",  query.Scopes ?? string.Empty }
                };
        descriptor.NotBefore = utcNow.UtcDateTime;
        descriptor.Expires = utcNow.UtcDateTime.AddHours(tokenConfiguration.MaximumTokenLifetime.GetValueOrDefault(2));
        descriptor.SigningCredentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        descriptor.EncryptingCredentials = new EncryptingCredentials(key, SecurityAlgorithms.Aes256KW, 
            SecurityAlgorithms.Aes256CbcHmacSha512);
        
        var handler = new JwtSecurityTokenHandler();
        var token = handler.CreateEncodedJwt(descriptor);
        return token;
    }

    public async Task<AuthenticateMachineResult> Handle(AuthenticateMachineQuery request, CancellationToken cancellationToken)
    {
        var result = await machineRepository.GetAsync(new MachineDataFilter
        {
            Secret = request.Secret?.Base64Encode(),
            MachineId = request.MachineId
        }, cancellationToken);

        if (result is null)
        {
            return new AuthenticateMachineResult(null, new ResponseException("Machine authentication failed", StatusCodes.Status401Unauthorized));
        }

        var accessToken = await mediator.Send(new GetValidMachineAccessTokenQuery(result.MachineId), cancellationToken);

        if (accessToken is not null)
        {
            //serves existing token
            return new AuthenticateMachineResult(new MachineToken(accessToken.Token));
        }

        var utcNow = timeProvider.GetUtcNow();

        var newToken = GenerateToken(request, result.Map<MachineData>());
        var tokenConfiguration = tokenConfigurationOptions.Value;

        await mediator.Publish(new QueueMachineAccessTokenNotification
        {
            Expires = utcNow.AddHours(tokenConfiguration.MaximumTokenLifetime.GetValueOrDefault(2)),
            Token = newToken,
            ValidFrom = utcNow,
            MachineId = request.MachineId.GetValueOrDefault().ToString(),
        }, cancellationToken);
        
        return new AuthenticateMachineResult(new MachineToken(newToken));
    }
}
