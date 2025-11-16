using GeoAuth.Shared.Exceptions;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Text;

namespace geo_auth.Features.ValidateRequest
{
    internal class ValidateRequestCommandHandler(ILogger<ValidateRequestCommandHandler> logger) : IRequestHandler<ValidateRequestCommand, ValidateRequestResult>
    {
        public async Task<ValidateRequestResult> Handle(ValidateRequestCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var context = request.HttpContext
                    ?? throw new ResponseException("Unexpected error: Context unavailable", StatusCodes.Status500InternalServerError);

                if (context.Request.QueryString.HasValue)
                {
                    throw new ResponseException("Sensitive parameters should not be passed in the URL of a public method. If using a JWT token, safely encode it and include the header 'Accept-Encoding: jwt'. Supply the token as a 'token' field in the request body or as form data to ensure secure transmission.", StatusCodes.Status400BadRequest);
                }

                if (!context.Request.IsHttps)
                {
                    var caller = context.Request.Headers["X-Forwarded-For"].FirstOrDefault() ?? context.Connection.RemoteIpAddress?.ToString();
                    logger.LogWarning("Insecure channel from {Caller}. Avoid transmitting PII. Consider encrypting JWT before sending it.", caller);
                }
                
                var acceptedEncodings = context.Request.Headers.AcceptEncoding.Where(x => !string.IsNullOrWhiteSpace(x))
                    .SelectMany(x => x!.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries));

                if (!acceptedEncodings.Any(request.AcceptableEncodings.Contains))
                {
                    var helpText = new StringBuilder();

                    foreach (var acceptEncoding in request.AcceptableEncodings)
                    {
                        helpText.AppendLine($"'Accept-Encoding': '{acceptEncoding.ToLower()}'");
                    }

                    throw new ResponseException($"An acceptable encoding type was not specified. Include one of these headers {helpText} in your request.", 
                        StatusCodes.Status422UnprocessableEntity);
                }


                ValidateRequest? data = null;
                var requiredException = new ResponseException("Token is a required field", StatusCodes.Status400BadRequest);
                if (context.Request.HasFormContentType)
                {
                    if (!context.Request.Form.TryGetValue("token", out var token))
                    {
                        throw requiredException;
                    }

                    data = new ValidateRequest { Token = token };
                }
                else if (context.Request.HasJsonContentType())
                {
                    data = await context.Request.ReadFromJsonAsync<ValidateRequest>(cancellationToken)
                        ?? throw requiredException;
                }

                return new ValidateRequestResult(data
                        ?? throw new ResponseException("Unexpected error: input payload unavailable", StatusCodes.Status500InternalServerError));
            }
            catch (Exception exception)
            {
                logger.LogError(exception, "Unable to validate request");
                return new ValidateRequestResult(default, exception);
            }
        }
    }
}
