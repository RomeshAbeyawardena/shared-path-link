using GeoAuth.Shared.Extensions;
using GeoAuth.Shared.Features.Jwt;
using GeoAuth.Shared.Requests.Tokens;
using MediatR;
using Microsoft.Extensions.Logging;

namespace geo_auth.Features.BeginAuthentication;

internal class ValidateMachineTokenQueryHandler(IJwtHelper jwtHelper, 
    ILogger<ValidateMachineTokenQueryHandler> logger) : IRequestHandler<ValidateMachineTokenQuery, MachineTokenQueryResult>
{
    public async Task<MachineTokenQueryResult> Handle(ValidateMachineTokenQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var tokenResult = await jwtHelper.ReadTokenAsync<MachineTokenDto>(request.Token, jwtHelper.DefaultParameters(true, true));

            tokenResult.EnsureSuccessOrThrow();

            var tokenClaims = tokenResult.Result!.Map<MachineToken>();

            return new MachineTokenQueryResult(
                new MachineTokenQueryResponse(tokenClaims.MachineId, tokenClaims.Secret));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unable to validate user token");
            return new MachineTokenQueryResult(null, ex);
        }
    }
}
