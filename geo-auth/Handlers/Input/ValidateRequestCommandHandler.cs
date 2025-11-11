using GeoAuth.Shared.Exceptions;
using GeoAuth.Shared.Requests.Input;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace geo_auth.Handlers.Input
{
    internal class ValidateRequestCommandHandler(ILogger<ValidateRequestCommandHandler> logger) : IRequestHandler<ValidateRequestCommand, PasswordHasherRequestResult>
    {
        public async Task<PasswordHasherRequestResult> Handle(ValidateRequestCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var context = request.HttpContext
                    ?? throw new ResponseException("Unexpected error: Context unavailable", StatusCodes.Status500InternalServerError);

                if (context.Request.QueryString.HasValue)
                {
                    throw new ResponseException("Sensitive parameters should not be passed in the URL of a public method. If using a JWT token, safely encode it and include the header 'Accept-Encoding: jwt'. Supply the token as a 'token' field in the request body or as form data to ensure secure transmission.", StatusCodes.Status400BadRequest);
                }

                var acceptedEncodings = context.Request.Headers.AcceptEncoding.Where(x => !string.IsNullOrWhiteSpace(x))
                    .SelectMany(x => x!.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries));

                if (!acceptedEncodings.Any(request.AcceptableEncodings.Contains))
                {
                    throw new ResponseException("An acceptable content type was not specified. Include the header 'Accept-Encoding: jwt' in your request.", StatusCodes.Status422UnprocessableEntity);
                }


                PasswordHasherRequest? data = null;
                var requiredException = new ResponseException("Token is a required field", StatusCodes.Status400BadRequest);
                if (context.Request.HasFormContentType)
                {
                    if (!context.Request.Form.TryGetValue("token", out var token))
                    {
                        throw requiredException;
                    }

                    data = new PasswordHasherRequest { Token = token };
                }
                else if (context.Request.HasJsonContentType())
                {
                    data = await context.Request.ReadFromJsonAsync<PasswordHasherRequest>(cancellationToken)
                        ?? throw requiredException;
                }

                return new PasswordHasherRequestResult(data 
                    ?? throw new ResponseException("Unexpected error: input payload unavailable", StatusCodes.Status500InternalServerError));
            }
            catch (Exception exception)
            {
                logger.LogError(exception, "Unable to validate request");
                return new PasswordHasherRequestResult(default, exception);
            }
        }
    }
}
