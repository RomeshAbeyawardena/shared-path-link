using geo_auth.Features.BeginAuthentication;
using geo_auth.Features.ValidateRequest;
using geo_auth.Models;
using GeoAuth.Shared.Exceptions;
using GeoAuth.Shared.Extensions;
using GeoAuth.Shared.Features.BeginAuthentication;
using GeoAuth.Shared.Requests.Tokens;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.DependencyInjection;
using System.Text.Json;

namespace geo_auth;

public static partial class Endpoints
{
    [Function("begin-auth")]
    public static async Task <IResult> BeginAuthAsync(
        [HttpTrigger(AuthorizationLevel.Function, "post")] HttpRequest request,
        FunctionContext executionContext)
    {
        Guid? automationId = GetAutomationId(request.Headers);

        var services = request.HttpContext.RequestServices;
        var mediator = services
            .GetRequiredService<IMediator>();

        var cancellationToken = executionContext.CancellationToken;

        try
        {
            
            var validateRequestResult = await mediator.Send(new ValidateRequestCommand() { 
                AcceptableEncodings = ["jwt"], 
                HttpContext = request.HttpContext }, cancellationToken);

            validateRequestResult.EnsureSuccessOrThrow();

            var machineTokenQueryResult = await mediator.Send(new ValidateMachineTokenQuery(validateRequestResult.Result?.Token
                ?? throw new ResponseException("Token is a required field", StatusCodes.Status400BadRequest)), cancellationToken);

            machineTokenQueryResult.EnsureSuccessOrThrow();

            var machineToken = machineTokenQueryResult.Result ??
              throw new ResponseException("Unexpected null object", StatusCodes.Status500InternalServerError);

            var machineTokenAuthenticationResult = await mediator.Send(new IssueMachineAuthTokenCommand(machineToken.MachineId, machineToken.Secret));
            machineTokenAuthenticationResult.EnsureSuccessOrThrow();
            
            return new AuthTokenResponse(new AuthTokenResult(machineTokenAuthenticationResult.Result?.Token), automationId);
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
