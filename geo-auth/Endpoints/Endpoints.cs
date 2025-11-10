using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;

namespace geo_auth;

public static class Endpoints
{
    [Function("PasswordSalter")]
    public static async Task<IActionResult> RunAsync([HttpTrigger(AuthorizationLevel.Function, "get", "post")] HttpRequest request,
        FunctionContext executionContext,
        CancellationToken cancellationToken)
    {
        await Task.CompletedTask;
        return new JsonResult(true);
    }
}
