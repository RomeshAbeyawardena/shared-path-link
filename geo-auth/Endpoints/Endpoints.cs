using geo_auth.Handlers.Passwords;
using geo_auth.Models;
using GeoAuth.Shared.Models;
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
    public static void EnsureSuccessOrThrow<T>(IResult<T> result)
    {
        if (!result.IsSuccess)
        {
            if (result.Exception is not null)
            {
                throw ResponseException.Transform(result.Exception);
            }

            throw new ResponseException("An unexpected error occurred", StatusCodes.Status500InternalServerError);
        }
    }

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

            EnsureSuccessOrThrow(inputResponse);

            var data = inputResponse.Result;

            var userDataResponse = await mediator.Send(new ValidateUserQuery(data?.Token 
                ?? throw new ResponseException("Token is a required field", StatusCodes.Status400BadRequest)), cancellationToken);

            EnsureSuccessOrThrow(userDataResponse);

            var user = userDataResponse.Result
                ?? throw new ResponseException("User result object is unexpectedly null", StatusCodes.Status500InternalServerError);

            var hasherResponse = await mediator.Send(new GeneratePasswordHashCommand(user), cancellationToken);

            EnsureSuccessOrThrow(hasherResponse);

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
