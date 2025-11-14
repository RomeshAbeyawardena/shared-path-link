using GeoAuth.Shared.Models;
using GeoAuth.Shared.Requests.Tokens;
using MediatR;

namespace GeoAuth.Shared.Requests.Tokens;

public interface IValidateAccessTokenResult
{
    IEnumerable<string> Scopes { get; }
}

public record ValidateAccessTokenResult : IValidateAccessTokenResult
{
    public IEnumerable<string> Scopes { get; init; }
}

public record ValidateAccessTokenResponse(ValidateAccessTokenResult? Result, 
    Exception? Exception = null) : ResultBase<ValidateAccessTokenResult>(Result, Exception);


public record ValidateAccessTokenQuery(string Token) : IRequest<ValidateAccessTokenResponse>
{
}
