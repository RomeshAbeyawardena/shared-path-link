using geo_auth.Configuration;
using GeoAuth.Shared.Exceptions;
using GeoAuth.Shared.Features.Jwt;
using GeoAuth.Shared.Models;
using GeoAuth.Shared.Requests.Tokens;
using MediatR;
using Microsoft.Extensions.Logging;

namespace geo_auth.Handlers.Tokens;

internal class ValidateUserQueryHandler(IJwtHelper jwtHelper,
    ILogger<ValidateUserQueryHandler> logger) : IRequestHandler<ValidateUserQuery, UserResult>
{
    public async Task<UserResult> Handle(ValidateUserQuery request, CancellationToken cancellationToken)
    {
        var userResult = await jwtHelper.ReadTokenAsync<UserDto>(request.Token, jwtHelper.DefaultParameters(true));

       if (userResult.IsSuccess)
       {
            return new UserResult(userResult.Result?.Map<User>());
       }

        if (userResult.Exception is not null)
        {
            logger.LogError(userResult.Exception, "Unable to validate user token");
        }

        return new UserResult(null, userResult.Exception);
        
    }
}
