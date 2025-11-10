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
        string[] acceptableEncodings = ["jwt"];
        try
        {
            if (request.QueryString.HasValue)
            {
                throw new ResponseException("Sensitive parameters should not be passed in the URL of a public method. If using a JWT token, safely encode it and include the header 'Accept-Encoding: jwt'. Supply the token as a 'token' field in the request body or as form data to ensure secure transmission.", StatusCodes.Status400BadRequest);
            }

            if (!request.Headers.AcceptEncoding.Any(acceptableEncodings.Contains))
            {
                throw new ResponseException("An acceptable content type was not specified. Include the header 'Accept-Encoding: jwt' in your request.", StatusCodes.Status422UnprocessableEntity);
            }

            await Task.CompletedTask;
            return new PasswordSalterResponse();
        }
        catch (ResponseException ex)
        {
            return new StandardErrorResponse(ex);
        }
        catch (Exception ex)
        {
            return new StandardErrorResponse(ex, StatusCodes.Status400BadRequest);
        }
    }
}
