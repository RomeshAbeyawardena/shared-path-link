using geo_auth.Models;
using GeoAuth.Shared.Requests.Tokens;
using Konscious.Security.Cryptography;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Logging;
using Microsoft.IdentityModel.Tokens;
using System.Diagnostics;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace geo_auth;

public static class Endpoints
{
    public static byte[] GenerateSalt(int size = 16)
    {
        var salt = new byte[size];
        RandomNumberGenerator.Fill(salt);
        return salt;
    }

    [Function("hasher")]
    public static async Task<IResult> RunAsync([HttpTrigger(AuthorizationLevel.Function, "get", "post")] HttpRequest request,
        FunctionContext executionContext)
    {
        string[] acceptableEncodings = ["jwt"];
        Guid? automationId = request.Headers.TryGetValue("automation-id", out var automationIdValue)
            && Guid.TryParse(automationIdValue, out var id) ? id : null;

        var mediator = request.HttpContext.RequestServices.GetRequiredService<IMediator>();

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

            var userDataResponse = await mediator.Send(new ValidateUserQuery(data?.Token 
                ?? throw new ResponseException("Token is a required field", StatusCodes.Status400BadRequest)));

            if (!userDataResponse.IsSuccess)
            {
                if (userDataResponse.Exception is not null)
                {
                    if (userDataResponse.Exception is ResponseException responseException)
                    {
                        throw responseException;
                    }

                    throw new ResponseException(userDataResponse.Exception, StatusCodes.Status400BadRequest);
                }

                throw new ResponseException("An unexpected error occurred", StatusCodes.Status500InternalServerError);
            }

            var user = userDataResponse.Result
                ?? throw new ResponseException("User object is unexpectedly null", StatusCodes.Status500InternalServerError);

            if (string.IsNullOrWhiteSpace(user.Salt))
            {
                user.Salt = Convert.ToBase64String(GenerateSalt());
            }

            var hashedPassword = new Argon2id(Encoding.UTF8.GetBytes(user.Secret
                ?? throw new ResponseException("Secret must not be empty", StatusCodes.Status400BadRequest)))
            {
                KnownSecret = Encoding.UTF8.GetBytes(configuration["KnownSecret"] 
                    ?? throw new ResponseException("KnownSecret is empty", StatusCodes.Status500InternalServerError)),
                Salt = Convert.FromBase64String(user.Salt),
                DegreeOfParallelism = 4,
                MemorySize = 65536,
                Iterations = 4
            }.GetBytes(32);

            return new PasswordSalterResponse(automationId)
            {
                Hash = Convert.ToBase64String(hashedPassword),
                Salt = user.Salt
            };
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
