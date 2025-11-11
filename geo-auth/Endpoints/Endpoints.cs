using geo_auth.Handlers.Passwords;
using geo_auth.Models;
using GeoAuth.Shared.Requests.Input;
using GeoAuth.Shared.Requests.Passwords;
using GeoAuth.Shared.Requests.Tokens;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.Functions.Worker;

using Microsoft.Extensions.DependencyInjection;

namespace geo_auth;

public static class Endpoints
{
    [Function("hasher")]
    public static async Task<IResult> RunAsync([HttpTrigger(AuthorizationLevel.Function, "get", "post")] HttpRequest request,
        FunctionContext executionContext)
    {
        string[] acceptableEncodings = ["jwt"];
        Guid? automationId = request.Headers.TryGetValue("automation-id", out var automationIdValue)
            && Guid.TryParse(automationIdValue, out var id) ? id : null;

        var mediator = request.HttpContext.RequestServices.GetRequiredService<IMediator>();
        var cancellationToken = executionContext.CancellationToken;
        try
        {
            var inputResponse = await mediator.Send(new ValidateRequestCommand { 
                AcceptableEncodings = acceptableEncodings,
                HttpContext = request.HttpContext }, cancellationToken);
            
            if (!inputResponse.IsSuccess)
            {
                if (inputResponse.Exception is not null)
                {
                    throw ResponseException.Transform(inputResponse.Exception);
                }

                throw new ResponseException("An unexpected error occurred", StatusCodes.Status500InternalServerError);
            }

            var data = inputResponse.Result;

            var userDataResponse = await mediator.Send(new ValidateUserQuery(data?.Token 
                ?? throw new ResponseException("Token is a required field", StatusCodes.Status400BadRequest)), cancellationToken);

            if (!userDataResponse.IsSuccess)
            {
                if (userDataResponse.Exception is not null)
                {
                    throw ResponseException.Transform(userDataResponse.Exception);
                }

                throw new ResponseException("An unexpected error occurred", StatusCodes.Status500InternalServerError);
            }

            var user = userDataResponse.Result
                ?? throw new ResponseException("User result object is unexpectedly null", StatusCodes.Status500InternalServerError);

            var hasherResponse = await mediator.Send(new GeneratePasswordHashCommand(user), cancellationToken);

            if (!hasherResponse.IsSuccess)
            {
                if (hasherResponse.Exception is not null)
                {
                    throw ResponseException.Transform(hasherResponse.Exception);
                }

                throw new ResponseException("An unexpected error occurred", StatusCodes.Status500InternalServerError);
            }

            return new PasswordHashResponse(hasherResponse.Result
                ?? throw new ResponseException("Hasher result object is unexpectedly null", StatusCodes.Status500InternalServerError), automationId);
        }
        catch (ResponseException ex)
        {
            return new StandardErrorResponse(ex, automationId);
        }
        catch (Exception ex)
        {
            return new StandardErrorResponse(ex, StatusCodes.Status500InternalServerError, automationId);
        }
    }
}
