using GeoAuth.Shared.Models;
using MediatR;

namespace GeoAuth.Shared.Requests.Passwords
{
    public record GeneratePasswordHashCommand(User User) : IRequest<PasswordHashResult>
    {
    }
}
