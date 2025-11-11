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

        if (token.Claims.TryGetValue(ClaimTypes.NameIdentifier, out var sub) && Guid.TryParse(sub?.ToString(), out var sid))
        {
            user.Id = sid;
        }

        if (token.Claims.TryGetValue(ClaimTypes.Email, out var email))
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

            var userData = await mediator.Send(new ValidateUserQuery(data?.Token 
                ?? throw new ResponseException("Token is a required field", StatusCodes.Status400BadRequest));

            var configuration = request.HttpContext.RequestServices.GetRequiredService<IConfiguration>();

            var user = await ProcessTokenAsync(configuration,
                data ?? throw requiredException, executionContext.CancellationToken)
                ?? throw new ResponseException("Unable to validate token", StatusCodes.Status400BadRequest);

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
