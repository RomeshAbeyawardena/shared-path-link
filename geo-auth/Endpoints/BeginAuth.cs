using geo_auth.Handlers.Tokens;
using geo_auth.Models;
using GeoAuth.Shared.Exceptions;
using GeoAuth.Shared.Extensions;
using GeoAuth.Shared.Requests.Input;
using GeoAuth.Shared.Requests.Tokens;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.DependencyInjection;
using System.Text.Json;

namespace geo_auth;

internal record MachineData
{
    public Guid MachineId { get; set; }
    public string? Secret { get; set; }
}

internal record BlobConfiguration
{
    public IEnumerable<MachineData> Machines { get; set; } = [];
}

public static partial class Endpoints
{
    [Function("begin-auth")]
    public static async Task <IResult> BeginAuth(
        [HttpTrigger(AuthorizationLevel.Function, "post")] HttpRequest request,
        [BlobInput("geo-auth/config.json", Connection = "AzureWebJobsStorage")] Stream content,
        FunctionContext executionContext)
    {
        Guid? automationId = GetAutomationId(request.Headers);

        var mediator = request.HttpContext.RequestServices
            .GetRequiredService<IMediator>();

        var cancellationToken = executionContext.CancellationToken;

        try
        {
            var jsonOptions = new JsonSerializerOptions(JsonSerializerOptions.Default)
            {
                PropertyNameCaseInsensitive = true
            };

            var configuration = JsonSerializer.Deserialize<BlobConfiguration>(content, jsonOptions);

            var validateRequestResult = await mediator.Send(new ValidateRequestCommand() { HttpContext = request.HttpContext }, cancellationToken);

            validateRequestResult.EnsureSuccessOrThrow();

            var machineTokenQueryResult = await mediator.Send(new ValidateMachineTokenQuery(validateRequestResult.Result?.Token
                ?? throw new ResponseException("Token is a required field", StatusCodes.Status500InternalServerError)), cancellationToken);

            machineTokenQueryResult.EnsureSuccessOrThrow();

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
