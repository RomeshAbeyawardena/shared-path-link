using geo_auth.Handlers.Tokens;
using geo_auth.Models;
using GeoAuth.Shared.Exceptions;
using GeoAuth.Shared.Extensions;
using GeoAuth.Shared.Requests.Input;
using GeoAuth.Shared.Requests.MachineToken;
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
    public static async Task <IResult> BeginAuth(
        [HttpTrigger(AuthorizationLevel.Function, "post")] HttpRequest request,
        FunctionContext executionContext)
    {
        Guid? automationId = GetAutomationId(request.Headers);

        var services = request.HttpContext.RequestServices;
        var mediator = services
            .GetRequiredService<IMediator>();

        var jsonOptions = services.GetRequiredService<JsonSerializerOptions>();
        var cancellationToken = executionContext.CancellationToken;

        try
        {
            var configuration = JsonSerializer.Deserialize<RegisteredMachineConfiguration>(content, jsonOptions);

            var validateRequestResult = await mediator.Send(new ValidateRequestCommand() { 
                AcceptableEncodings = ["jwt"], 
                HttpContext = request.HttpContext }, cancellationToken);

            validateRequestResult.EnsureSuccessOrThrow();

            var machineTokenQueryResult = await mediator.Send(new ValidateMachineTokenQuery(validateRequestResult.Result?.Token
                ?? throw new ResponseException("Token is a required field", StatusCodes.Status400BadRequest)), cancellationToken);

            machineTokenQueryResult.EnsureSuccessOrThrow();

            //var isValid = configuration?.IsRegistered(machineTokenQueryResult.Result );

            //if (!isValid.GetValueOrDefault()){
            //  throw new ResponseException("Invalid request", StatusCodes.Status401Unauthorized);
            //}
            var machineToken = machineTokenQueryResult.Result ??
              throw new ResponseException("Unexpected null object", StatusCodes.Status500InternalServerError);

            var machineTokenAuthenticationResult = await mediator.Send(new AuthenticateMachineQuery(machineToken.MachineId, machineToken.Secret));
            machineTokenAuthenticationResult.EnsureSuccessOrThrow();
            
            return new AuthTokenResponse(null!, automationId);

            
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
