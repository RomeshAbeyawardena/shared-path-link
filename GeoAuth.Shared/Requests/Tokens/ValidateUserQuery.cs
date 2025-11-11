using GeoAuth.Shared.Models;
using MediatR;

namespace GeoAuth.Shared.Requests.Tokens
{
    public record ValidateUserQuery(string Token) : IRequest<UserResult>
    {
    }
}
