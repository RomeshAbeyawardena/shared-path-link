using geo_auth.Models;
using GeoAuth.Shared.Exceptions;
using GeoAuth.Shared.Extensions;
using GeoAuth.Shared.Requests.Input;
using GeoAuth.Shared.Requests.Tokens;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.DependencyInjection;
using System.Text.Json.Serialization;

namespace geo_auth;

internal interface IAuthTokenResult
{
    string? Token { get; }
}

internal record AuthTokenResponse : MappableStandardResponse<IAuthTokenResult, AuthTokenResponse>, IAuthTokenResult
{
    protected override IAuthTokenResult Source => this;
    protected override AuthTokenResponse? Result => this;

    public AuthTokenResponse(IAuthTokenResult response, Guid? automationId)
        : base(response, automationId)
    {
        
    }

    [JsonIgnore]
    public string? Token { get; private set; }

    public override void Map(IAuthTokenResult source)
    {
        Token = source.Token;
    }
}

public static partial class Endpoints
{
    [Function("auth")]
    public static async Task <IResult> BeginAuth([HttpTrigger(AuthorizationLevel.Function, "post")] HttpRequest request,
        FunctionContext executionContext)
    {
        Guid? automationId = GetAutomationId(request.Headers);

        var mediator = request.HttpContext.RequestServices
            .GetRequiredService<IMediator>();

        var cancellationToken = executionContext.CancellationToken;

        try
        {
            var validateRequestResult = await mediator.Send(new ValidateRequestCommand(), cancellationToken);

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
