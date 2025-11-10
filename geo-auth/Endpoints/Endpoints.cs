using geo_auth.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.Functions.Worker;

namespace geo_auth;

internal record PasswordSalterResponse : StandardResponse<PasswordSalterResponse>
{
    public PasswordSalterResponse()
    {
        
    }

    public PasswordSalterResponse(Guid? automationId) : base(automationId)
    {
        
    }

    protected override PasswordSalterResponse? Result => this;
}

public record PasswordSalterRequest
{
    public string? Token { get; set; }
}

public static class Endpoints
{
    public static async Task ProcessToken(PasswordSalterRequest request, CancellationToken cancellationToken)
    {
        //TODO!
    }

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

            var acceptedEncodings = request.Headers.AcceptEncoding.SelectMany(x => x.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries));

            if (!acceptableEncodings.Any(acceptableEncodings.Contains))
            {
                throw new ResponseException("An acceptable content type was not specified. Include the header 'Accept-Encoding: jwt' in your request.", StatusCodes.Status422UnprocessableEntity);
            }

            Guid? automationId = request.Headers.TryGetValue("automation-id", out var automationIdValue) 
                && Guid.TryParse(automationIdValue, out var id) ? id : null;

            PasswordSalterRequest? data = null;
            var requiredException = new ResponseException("Token is a required field", StatusCodes.Status400BadRequest);
            if (request.HasFormContentType)
            {
                if (!request.Form.TryGetValue("token", out var token))
                {
                    throw requiredException;
                }

                data = new PasswordSalterRequest { Token = token };
            }
            else if (request.HasJsonContentType())
            {
                data = await request.ReadFromJsonAsync<PasswordSalterRequest>(executionContext.CancellationToken)
                    ?? throw requiredException;
            }

            await ProcessToken(data ?? throw requiredException, executionContext.CancellationToken);
            return new PasswordSalterResponse(automationId);
        }
        catch (ResponseException ex)
        {
            return new StandardErrorResponse(ex);
        }
        catch (Exception ex)
        {
            return new StandardErrorResponse(ex, StatusCodes.Status500InternalServerError);
        }
    }
}
