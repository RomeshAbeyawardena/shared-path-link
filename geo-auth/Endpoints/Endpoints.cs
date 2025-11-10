using geo_auth.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.Functions.Worker;

namespace geo_auth;

internal record PasswordSalterResponse : StandardResponse<PasswordSalterResponse>
{
    protected override PasswordSalterResponse? Result => this;
}

public static class Endpoints
{
    [Function("password-salter")]
    public static async Task<IResult> RunAsync([HttpTrigger(AuthorizationLevel.Function, "get", "post")] HttpRequest request,
        FunctionContext executionContext)
    {
        if (request.QueryString.HasValue)
        {
            return new StandardErrorResponse(
                "Sensitive parameters should not be passed in the URL of a public method. If using a JWT token, safely encode it and include the header 'Accept-Encoding: jwt'. Supply the token as a 'token' field in the request body or as form data to ensure secure transmission.", 400);
        }
        await Task.CompletedTask;
        return new PasswordSalterResponse();
    }
}
