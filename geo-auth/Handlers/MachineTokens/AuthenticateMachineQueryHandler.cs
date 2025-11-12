using Azure.Data.Tables;
using geo_auth.Models;
using geo_auth.Extensions;
using GeoAuth.Shared.Requests.MachineToken;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using GeoAuth.Shared.Exceptions;
using Microsoft.AspNetCore.Http;

namespace geo_auth.Handlers.MachineTokens;

internal class AuthenticateMachineQueryHandler([FromKeyedServices("machine-token")] TableClient tableClient) : IRequestHandler<AuthenticateMachineQuery, AuthenticateMachineResult>
{
    public async Task<AuthenticateMachineResult> Handle(AuthenticateMachineQuery request, CancellationToken cancellationToken)
    {

        var result = await tableClient.QueryAsync<MachineData>($"MachineId eq '{request.MachineId}' AND Secret eq '{request.Secret}'", 1, cancellationToken: cancellationToken).FirstOrDefaultAsync(cancellationToken);
        
        if (result is null)
        {
            return new AuthenticateMachineResult(null, new ResponseException("Machine authentication failed", StatusCodes.Status401Unauthorized));
        }

        return new AuthenticateMachineResult(new MachineToken(""));
    }
}
