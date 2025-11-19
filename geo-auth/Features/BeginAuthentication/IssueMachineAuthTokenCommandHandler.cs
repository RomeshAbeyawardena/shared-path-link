using geo_auth.Configuration;
using geo_auth.Handlers.MachineTokens;
using GeoAuth.Infrastructure.Filters;
using GeoAuth.Infrastructure.Repositories;
using GeoAuth.Shared.Exceptions;
using GeoAuth.Shared.Extensions;
using GeoAuth.Shared.Features.BeginAuthentication;
using GeoAuth.Shared.Features.Jwt;
using GeoAuth.Shared.Requests.MachineToken;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;

namespace geo_auth.Features.BeginAuthentication;

internal class IssueMachineAuthTokenCommandHandler(IMachineRepository machineRepository,
    TimeProvider timeProvider, IOptions<TokenConfiguration> tokenConfigurationOptions, 
    IJwtHelper jwtHelper, IMediator mediator)
    : IRequestHandler<IssueMachineAuthTokenCommand, AuthenticateMachineResult>
{
    private string GenerateToken(IssueMachineAuthTokenCommand query, MachineData request)
    {
        var tokenResult = jwtHelper.WriteToken(new MachineTokenDto
        {
            MachineId = request.MachineId.ToString(),
            Scopes = query.Scopes,
            Secret = request.Secret
        }, new JwtHelperWriterOptions(true));

        if (!tokenResult.IsSuccess)
        {
            throw new ResponseException("Unable to write token", 500, tokenResult.Exception);
        }

        return tokenResult.Result;
    }

    public async Task<AuthenticateMachineResult> Handle(IssueMachineAuthTokenCommand request, CancellationToken cancellationToken)
    {
        try
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
                return new AuthenticateMachineResult(new AuthenticatedMachineToken(accessToken.Token));
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

            return new AuthenticateMachineResult(new AuthenticatedMachineToken(newToken));
        }
        catch (Exception ex)
        {
            return new AuthenticateMachineResult(null, ex);
        }
    }
}
