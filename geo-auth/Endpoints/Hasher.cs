using geo_auth.Handlers.Passwords;
using geo_auth.Models;
using GeoAuth.Shared.Exceptions;
using GeoAuth.Shared.Extensions;
using GeoAuth.Shared.Requests.Input;
using GeoAuth.Shared.Requests.Passwords;
using GeoAuth.Shared.Requests.Tokens;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.DependencyInjection;

namespace geo_auth;

public static partial class Endpoints
{
    [Function("hasher")]
    public static async Task<IResult> RunHasherAsync([HttpTrigger(AuthorizationLevel.Function, "post")] HttpRequest request,
        FunctionContext executionContext)
    {
        string[] acceptableEncodings = ["jwt"];
        Guid? automationId = GetAutomationId(request.Headers);

        var mediator = request.HttpContext.RequestServices.GetRequiredService<IMediator>();
        var cancellationToken = executionContext.CancellationToken;
        try
        {
            var result = await AuthoriseOrFail(request, cancellationToken, mediator);

            if (result is not null)
            {
                return result;
            }

            var inputResponse = await mediator.Send(new ValidateRequestCommand { 
                AcceptableEncodings = acceptableEncodings,
                HttpContext = request.HttpContext }, cancellationToken);

            inputResponse.EnsureSuccessOrThrow();

            var data = inputResponse.Result;

            var userDataResponse = await mediator.Send(new ValidateUserQuery(data?.Token 
                ?? throw new ResponseException("Token is a required field", StatusCodes.Status400BadRequest)), cancellationToken);

            userDataResponse.EnsureSuccessOrThrow();

            var user = userDataResponse.Result
                ?? throw new ResponseException("User result object is unexpectedly null", StatusCodes.Status500InternalServerError);

            var hasherResponse = await mediator.Send(new GeneratePasswordHashCommand(user), cancellationToken);

            hasherResponse.EnsureSuccessOrThrow();

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
