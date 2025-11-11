using geo_auth.Models;
using GeoAuth.Shared.Requests.Input;
using MediatR;
using Microsoft.AspNetCore.Http;
using System.Threading;

namespace geo_auth.Handlers.Input
{
    internal class ValidateRequestCommandHandler(IHttpContextAccessor contextAccessor) : IRequestHandler<ValidateRequestCommand, PasswordHasherRequestResult>
    {
        public async Task<PasswordHasherRequestResult> Handle(ValidateRequestCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var context = contextAccessor?.HttpContext 
                    ?? throw new ResponseException("Unexpected error: Context unavailable", StatusCodes.Status500InternalServerError);

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
                return new PasswordHasherRequestResult(default, exception);
            }
        }
    }
}
