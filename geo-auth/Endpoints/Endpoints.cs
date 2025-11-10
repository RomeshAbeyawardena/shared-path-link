using geo_auth.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Logging;
using Microsoft.IdentityModel.Tokens;
using System.Diagnostics;
using System.IdentityModel.Tokens.Jwt;

namespace geo_auth;

public static class Endpoints
{
    public static async Task<User> ProcessTokenAsync(IConfiguration configuration, PasswordSalterRequest request, CancellationToken cancellationToken)
    {
        //TODO!
        var signingKey = configuration["SigningKey"] ?? throw new ResponseException("Signing key missing", StatusCodes.Status500InternalServerError);
        var key = new SymmetricSecurityKey(Convert.FromBase64String(signingKey))
        {
            KeyId = configuration["SigningKeyId"] ?? throw new ResponseException("Signing key ID missing", StatusCodes.Status500InternalServerError)
        };

        var token = await new JwtSecurityTokenHandler().ValidateTokenAsync(request.Token, new TokenValidationParameters
        {
            ValidateAudience = true,
            ValidAudience = configuration["ValidAudience"],
            ValidateIssuer = true,
            ValidIssuer = configuration["ValidIssuer"],
            ValidateIssuerSigningKey = true,
            ValidAlgorithms = [SecurityAlgorithms.HmacSha256],
            IssuerSigningKey = key,
            //ValidateTokenReplay = true
        });

        var user = new User();

        if (!token.IsValid)
        {
            IdentityModelEventSource.ShowPII = true;
            throw new ResponseException("Token is invalid!", StatusCodes.Status406NotAcceptable, token.Exception);
        }

        if (token.Claims.TryGetValue("clientId", out var clientId) && Guid.TryParse(clientId?.ToString(), out var cid))
        {
            user.ClientId = cid;
        }

        if (token.Claims.TryGetValue("sub", out var sub) && Guid.TryParse(sub?.ToString(), out var sid))
        {
            user.Id = sid;
        }

        if (token.Claims.TryGetValue("email", out var email))
        {
            user.Email = email?.ToString();
        }

        if (token.Claims.TryGetValue("name", out var name))
        {
            user.Name = name?.ToString();
        }

        if (token.Claims.TryGetValue("secret", out var secret))
        {
            user.Secret = secret?.ToString();
        }

        if (token.Claims.TryGetValue("salt", out var salt))
        {
            user.Salt = salt?.ToString();
        }

        return user;
    }

    [Function("hasher")]
    public static async Task<IResult> RunAsync([HttpTrigger(AuthorizationLevel.Function, "get", "post")] HttpRequest request,
        FunctionContext executionContext)
    {
        string[] acceptableEncodings = ["jwt"];
        Guid? automationId = request.Headers.TryGetValue("automation-id", out var automationIdValue)
            && Guid.TryParse(automationIdValue, out var id) ? id : null;
        try
        {
            if (request.QueryString.HasValue)
            {
                throw new ResponseException("Sensitive parameters should not be passed in the URL of a public method. If using a JWT token, safely encode it and include the header 'Accept-Encoding: jwt'. Supply the token as a 'token' field in the request body or as form data to ensure secure transmission.", StatusCodes.Status400BadRequest);
            }

            var acceptedEncodings = request.Headers.AcceptEncoding.Where(x => !string.IsNullOrWhiteSpace(x))
                .SelectMany(x => x!.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries));

            if (!acceptedEncodings.Any(acceptableEncodings.Contains))
            {
                throw new ResponseException("An acceptable content type was not specified. Include the header 'Accept-Encoding: jwt' in your request.", StatusCodes.Status422UnprocessableEntity);
            }

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

            var configuration = request.HttpContext.RequestServices.GetRequiredService<IConfiguration>();

            var user = await ProcessTokenAsync(configuration,
                data ?? throw requiredException, executionContext.CancellationToken)
                ?? throw new ResponseException("Unable to validate token", StatusCodes.Status400BadRequest);



            return new PasswordSalterResponse(automationId);
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
