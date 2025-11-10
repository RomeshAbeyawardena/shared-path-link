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
        
        await Task.CompletedTask;
        return new PasswordSalterResponse();
    }
}
