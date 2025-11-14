using Azure.Core;
using geo_auth.Models;
using GeoAuth.Shared.Exceptions;
using GeoAuth.Shared.Requests.Tokens;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace geo_auth;

public static partial class Endpoints
{
    public static Guid? GetAutomationId(IHeaderDictionary headers)
    {
        return headers.TryGetValue("automation-id", out var automationIdValue)
            && Guid.TryParse(automationIdValue, out var id) ? id : null;
    }

    public static async Task<IResult?> AuthoriseOrFail(HttpRequest request, CancellationToken cancellationToken, IMediator? mediator = null)
    {
        try
        {
            mediator ??= request.HttpContext.RequestServices.GetRequiredService<IMediator>();
            var authorisationHeader = request.Headers.Authorization.FirstOrDefault();

            if (string.IsNullOrWhiteSpace(authorisationHeader))
            {
                throw new ResponseException("No authorisation header passed", StatusCodes.Status401Unauthorized);
            }

            var accessTokenResult = await mediator.Send(new ValidateAccessTokenQuery(authorisationHeader), cancellationToken);

            if (!accessTokenResult.IsSuccess)
            {
                throw new ResponseException("Not authorised", StatusCodes.Status401Unauthorized);
            }

            return null;
        }
        catch (Exception ex)
        {
            return new StandardErrorResponse(ex, StatusCodes.Status500InternalServerError);
        }
    }
}
