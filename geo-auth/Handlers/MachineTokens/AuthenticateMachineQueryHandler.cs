using Azure;
using Azure.Data.Tables;
using geo_auth.Extensions;
using geo_auth.Handlers.Tokens;
using geo_auth.Models;
using GeoAuth.Shared.Exceptions;
using GeoAuth.Shared.Requests.MachineToken;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;

namespace geo_auth.Handlers.MachineTokens;

internal class AuthenticateMachineQueryHandler([FromKeyedServices(KeyedServices.MachineTable)] TableClient machineTableClient,
    TimeProvider timeProvider, IOptions<TokenConfiguration> tokenConfigurationOptions,
    IMediator mediator)
    : IRequestHandler<AuthenticateMachineQuery, AuthenticateMachineResult>
{
    private string GenerateToken(MachineData request)
    {
        var tokenConfiguration = tokenConfigurationOptions.Value;

        var signingKey = tokenConfiguration.SigningKey ?? throw new ResponseException("Signing key missing", StatusCodes.Status500InternalServerError);
        var key = new SymmetricSecurityKey(Convert.FromBase64String(signingKey))
        {
            KeyId = tokenConfiguration.SigningKeyId ?? throw new ResponseException("Signing key ID missing", StatusCodes.Status500InternalServerError)
        };
        var utcNow = timeProvider.GetUtcNow();

        var descriptor = new SecurityTokenDescriptor
        {
            Issuer = tokenConfiguration.ValidAudience,
            Audience = tokenConfiguration.ValidAudience,
            Claims =
                {
                    { "partition-key", request.PartitionKey },
                    { "row-key", request.RowKey }
                },
            Expires = utcNow.UtcDateTime,
            SigningCredentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256)
        };

        var handler = new JsonWebTokenHandler();
        return handler.CreateToken(descriptor);
    }

    public async Task<AuthenticateMachineResult> Handle(AuthenticateMachineQuery request, CancellationToken cancellationToken)
    {

        var result = await machineTableClient.QueryAsync<MachineData>($"PartitionKey eq '{request.MachineId}' AND Secret eq '{request.Secret}'", 1,
            cancellationToken: cancellationToken).FirstOrDefaultAsync(cancellationToken);

        if (result is null)
        {
            return new AuthenticateMachineResult(null, new ResponseException("Machine authentication failed", StatusCodes.Status401Unauthorized));
        }

        var accessToken = await mediator.Send(new GetValidMachineAccessTokenQuery(result.PartitionKey), cancellationToken);

        if (accessToken is not null)
        {
            //serves existing token
            return new AuthenticateMachineResult(new MachineToken(accessToken.Token));
        }

        var utcNow = timeProvider.GetUtcNow();

        var newToken = GenerateToken(result);
        var tokenConfiguration = tokenConfigurationOptions.Value;

        await mediator.Publish(new QueueMachineAccessTokenNotification
        {
            Expires = utcNow.AddHours(tokenConfiguration.MaximumTokenLifetime.GetValueOrDefault(2)),
            Token = newToken,
            ValidFrom = utcNow,
            PartitionKey = request.MachineId.GetValueOrDefault().ToString(),
            
        }, cancellationToken);
        
        return new AuthenticateMachineResult(new MachineToken(newToken));
    }
}
